using SimpleCircuit.Algebra;
using SimpleCircuit.Contributors;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// An <see cref="IContribution"/> that represents a constant.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class ConstantContribution : IContribution
    {
        private readonly Element<double> _rhs;

        /// <inheritdoc/>
        public double Value { get; }

        /// <inheritdoc/>
        public int Row { get; }

        /// <inheritdoc/>
        public ISparseSolver<double> Solver { get; }

        /// <inheritdoc/>
        public IEnumerable<int> Unknowns
        {
            get
            {
                yield break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantContribution"/> class.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="row">The row.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The unknown type.</param>
        public ConstantContribution(ISparseSolver<double> solver, int row, double value, UnknownTypes type)
        {
            Row = row;
            Solver = solver ?? throw new ArgumentNullException(nameof(solver));
            switch (type)
            {
                case UnknownTypes.Angle: Value = Utility.Wrap(value); break;
                case UnknownTypes.Length: Value = Math.Max(value, 0); break;
                default:
                    Value = value;
                    break;
            }
            if (!value.Equals(0.0))
                _rhs = solver.GetElement(row);
        }

        /// <inheritdoc/>
        void IContribution.Update(IVector<double> solution) { }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            // There are no derivatives, so we skip the jacobian

            // The right-hand side vector may still be an issue though...
            if (rhs == null)
                _rhs?.Subtract(derivative * Value);
        }
    }
}
