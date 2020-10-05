using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that represents squaring.
    /// </summary>
    /// <seealso cref="Function" />
    public class Squared : Function
    {
        private readonly Function _a;
        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _a;
            private readonly Element<double> _rhs;
            public double Value { get; private set; }
            public RowEquation(IRowEquation a, ISparseSolver<double> solver, int row)
            {
                _a = a;
                _rhs = solver.GetElement(row);
            }

            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                {
                    rhs = _rhs;
                    rhs.Subtract(derivative * Value);
                }
                _a.Apply(derivative * 2 * _a.Value, rhs);
            }
            public void Update()
            {
                _a.Update();
                Value = _a.Value * _a.Value;
            }
        }

        /// <inheritdoc/>
        public override double Value => _a.Value * _a.Value;

        /// <inheritdoc/>
        public override bool IsConstant => _a.IsConstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="Squared"/> class.
        /// </summary>
        /// <param name="a">The argument.</param>
        public Squared(Function a)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
        }

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (_a.IsConstant)
                return;
            if (coefficient == null)
                _a.Differentiate(2.0 * _a, equations);
            else
                _a.Differentiate(coefficient * 2.0 * _a, equations);
        }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver) => new RowEquation(_a.CreateEquation(row, mapper, solver), solver, row);

        /// <inheritdoc/>
        public override bool Resolve(double value)
        {
            if (value.IsZero())
                return _a.Resolve(0.0);
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"({_a})^2";
    }
}
