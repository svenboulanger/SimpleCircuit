using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that describes the addition.
    /// </summary>
    /// <seealso cref="Function" />
    public class Addition : Function
    {
        private readonly Function _a, _b;

        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _a, _b;
            public double Value { get; private set; }
            public RowEquation(IRowEquation a, IRowEquation b)
            {
                _a = a;
                _b = b;
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                _a.Apply(derivative, rhs);
                _b.Apply(derivative, rhs);
            }
            public void Update()
            {
                _a.Update();
                _b.Update();
                Value = _a.Value + _b.Value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override double Value => _a.Value + _b.Value;

        /// <summary>
        /// Gets a flag that shows whether or not the function is a constant value.
        /// </summary>
        /// <value>
        ///   <c>true</c> the function is constant; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstant => _a.IsConstant && _b.IsConstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="Addition"/> class.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        public Addition(Function a, Function b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
        }

        /// <summary>
        /// Sets up the function for the specified solver.
        /// </summary>
        /// <param name="coefficient">The coefficient.</param>
        /// <param name="equations">The produced equations for each unknown.</param>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (coefficient == null)
            {
                _a.Differentiate(null, equations);
                _b.Differentiate(null, equations);
            }
            else
            {
                _a.Differentiate(coefficient, equations);
                _b.Differentiate(coefficient, equations);
            }
        }

        /// <summary>
        /// Creates a row equation at the specified row, in the specified solver.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="solver">The solver.</param>
        /// <returns>The row equation.</returns>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
        {
            return new RowEquation(
                _a.CreateEquation(row, mapper, solver),
                _b.CreateEquation(row, mapper, solver));
        }

        /// <summary>
        /// Tries to resolve unknowns.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if one ore more unknowns were resolved; otherwise, <c>false</c>.
        /// </returns>
        public override bool Resolve(double value)
        {
            if (_a.IsConstant)
                return _b.Resolve(value - _a.Value);
            if (_b.IsConstant)
                return _a.Resolve(value - _b.Value);
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{_a}+{_b}";
    }
}
