using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// Represents the exponential of a contribution.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class SkewedContribution : IContribution
    {
        private double _dfdx;
        private readonly IContribution _a;
        private readonly Element<double> _rhs;
        private readonly static double _factor = Math.Log(2);

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _a.Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _a.Solver;

        /// <inheritdoc/>
        public IEnumerable<int> Unknowns => _a.Unknowns;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkewedContribution"/> class.
        /// </summary>
        /// <param name="a">The variable.</param>
        public SkewedContribution(IContribution a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _rhs = _a.Solver.GetElement(_a.Row);
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            if (rhs == null && _rhs != null)
            {
                rhs = _rhs;
                rhs.Subtract(derivative * Value);
            }
            _a.Add(derivative * _dfdx, rhs);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _a.Update(solution);
            var f = Math.Exp(_a.Value);
            Value = Math.Log(1 + f) / _factor;
            _dfdx = 1 / (1 + 1 / f) / _factor;
        }
    }
}
