using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// Represents a multiplication of two contributions.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class MultipliedContribution : IContribution
    {
        private readonly IContribution _a, _b;
        private readonly Element<double> _rhs;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _a.Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _a.Solver;

        /// <inheritdoc/>
        public IEnumerable<int> Unknowns => _a.Unknowns.Union(_b.Unknowns);

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipliedContribution"/> class.
        /// </summary>
        /// <param name="a">The first contribution.</param>
        /// <param name="b">The second contribution.</param>
        public MultipliedContribution(IContribution a, IContribution b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            if (_a.Row != _b.Row || _a.Solver != _b.Solver)
                throw new ArgumentException("Contributions do not use the same row or solver.");
            _rhs = _a.Solver.GetElement(_a.Row);
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            // This is an inherently nonlinear thing, so let's define the rhs if it doesn't exist
            if (rhs == null && _rhs != null)
            {
                rhs = _rhs;
                rhs.Subtract(derivative * Value);
            }
            _a.Add(derivative * _b.Value, rhs);
            _b.Add(derivative * _a.Value, rhs);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _a.Update(solution);
            _b.Update(solution);
            Value = _a.Value * _b.Value;
        }
    }
}
