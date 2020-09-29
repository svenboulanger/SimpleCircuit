using SimpleCircuit.Algebra;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that represents a constant value.
    /// </summary>
    /// <seealso cref="Function" />
    public class ConstantFunction : Function
    {
        private readonly double _value;
        private class RowEquation : IRowEquation
        {
            private readonly Element<double> _rhs;
            public double Value { get; }
            public RowEquation(int row, ISparseSolver<double> solver, double value)
            {
                _rhs = null;
                if (!value.IsZero())
                    _rhs = solver.GetElement(row);
                Value = value;
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                    _rhs?.Subtract(derivative * Value);
            }
            public void Update() { }
        }

        /// <inheritdoc/>
        public override double Value => _value;

        /// <inheritdoc/>
        public override bool IsConstant => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantFunction"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ConstantFunction(double value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations) { }

        /// <inheritdoc/>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver) => new RowEquation(row, solver, _value);

        /// <inheritdoc/>
        public override bool Resolve(double value) => false;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Value.ToString("G3");
    }
}
