using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that represents the division.
    /// </summary>
    /// <seealso cref="Function" />
    public class Division : Function
    {
        private readonly Function _a, _b;
        private class RowEquation : IRowEquation
        {
            private readonly IRowEquation _a, _b;
            private readonly Element<double> _rhs;
            public double Value { get; private set; }
            public RowEquation(IRowEquation a, IRowEquation b, ISparseSolver<double> solver, int row)
            {
                _a = a;
                _b = b;
                _rhs = solver.GetElement(row);
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                {
                    rhs = _rhs;
                    rhs.Subtract(derivative * Value);
                }
                _a.Apply(derivative / _b.Value, rhs);
                _b.Apply(-derivative * Value / _b.Value, rhs);
            }
            public void Update()
            {
                _a.Update();
                _b.Update();
                Value = _a.Value / _b.Value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override double Value => _a.Value / _b.Value;

        /// <summary>
        /// Gets a flag that shows whether or not the function is a constant value.
        /// </summary>
        /// <value>
        ///   <c>true</c> the function is constant; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstant => _a.IsConstant && _b.IsConstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="Division"/> class.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <exception cref="ArgumentNullException">
        /// a
        /// or
        /// b
        /// </exception>
        public Division(Function a, Function b)
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
                if (!_a.IsConstant)
                    _a.Differentiate(1.0 / _b, equations);
                if (!_b.IsConstant)
                    _b.Differentiate(_a / _b / _b, equations);
            }
            else
            {
                if (!_a.IsConstant)
                    _a.Differentiate(coefficient / _b, equations);
                if (!_b.IsConstant)
                    _b.Differentiate(-coefficient * _a / _b / _b, equations);
            }
        }

        /// <summary>
        /// Creates a row equation at the specified row, in the specified solver.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="solver">The solver.</param>
        /// <returns>
        /// The row equation.
        /// </returns>
        public override IRowEquation CreateEquation(int row, UnknownMap mapper, ISparseSolver<double> solver)
        {
            var a = _a.CreateEquation(row, mapper, solver);
            var b = _b.CreateEquation(row, mapper, solver);
            return new RowEquation(a, b, solver, row);
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
            // value = a / b
            if (_a.IsConstant)
                return _b.Resolve(_a.Value / value);
            if (_b.IsConstant)
                return _a.Resolve(_b.Value * value);
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"({_a})/({_b})";
    }
}
