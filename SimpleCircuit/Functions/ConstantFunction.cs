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

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override double Value => _value;

        /// <summary>
        /// Gets a flag that shows whether or not the function is a constant value.
        /// </summary>
        /// <value>
        ///   <c>true</c> the function is constant; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstant => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantFunction"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ConstantFunction(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Sets up the function for the specified solver.
        /// </summary>
        /// <param name="coefficient">The coefficient.</param>
        /// <param name="equations">The produced equations for each unknown.</param>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations) { }

        /// <summary>
        /// Creates a row equation at the specified row, in the specified solver.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="solver">The solver.</param>
        /// <returns>
        /// The row equation.
        /// </returns>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver) => new RowEquation(row, solver, _value);

        /// <summary>
        /// Tries to resolve unknowns.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if one ore more unknowns were resolved; otherwise, <c>false</c>.
        /// </returns>
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
