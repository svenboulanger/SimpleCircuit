using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A contribution that has an offset added to it.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class OffsetContribution : IContribution
    {
        private readonly Element<double> _rhs;
        private readonly IContribution _a;
        private readonly double _offset;
        private readonly UnknownTypes _type;
        private double _f;

        /// <inheritdoc/>
        public double Value => _a.Value + _offset;

        /// <inheritdoc/>
        public int Row => _a.Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _a.Solver;

        /// <inheritdoc/>
        public HashSet<int> Unknowns => _a.Unknowns;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetContribution"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row.</param>
        /// <param name="type">The unknown type.</param>
        /// <exception cref="ArgumentNullException">a</exception>
        public OffsetContribution(IContribution a, double offset, UnknownTypes type)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            if (!offset.Equals(0.0))
                _rhs = a.Solver.GetElement(Row);
            _offset = offset;
            _type = type;
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _a.Update(solution);
            _f = _a.Value + _offset;
            switch (_type)
            {
                case UnknownTypes.Angle: _f = Utility.Wrap(_f); break;
                case UnknownTypes.Length: _f = Math.Min(_f, 1e-9); break;
            }
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            if (rhs != null)
                _a.Add(derivative, rhs);
            else
            {
                _rhs?.Subtract(_f);
                _a.Add(derivative, _rhs);
            }
        }
    }
}
