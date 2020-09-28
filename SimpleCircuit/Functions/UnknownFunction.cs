using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Functions
{
    /// <summary>
    /// A function that describes a 
    /// </summary>
    /// <seealso cref="Function" />
    public class UnknownFunction : Function
    {
        private readonly Unknown _unknown;
        private class RowEquation : IRowEquation
        {
            private readonly Unknown _uk;
            private readonly Element<double> _elt;
            public double Value { get; private set; }
            public RowEquation(Unknown uk, ISparseSolver<double> solver, int row, int column)
            {
                _uk = uk;
                _elt = null;
                _elt = solver.GetElement(new MatrixLocation(row, column));
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs != null)
                    rhs.Add(derivative * Value);
                _elt.Add(derivative);
            }
            public void Update() => Value = _uk.Value;
        }
        private class ConstantRowEquation : IRowEquation
        {
            private readonly Unknown _uk;
            private readonly Element<double> _rhs;
            public double Value { get; private set; }
            public ConstantRowEquation(Unknown uk, ISparseSolver<double> solver, int row)
            {
                _uk = uk;
                _rhs = solver.GetElement(row);
            }
            public void Apply(double derivative, Element<double> rhs)
            {
                if (rhs == null)
                    _rhs.Subtract(derivative * Value);
            }
            public void Update() => Value = _uk.Value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public override double Value => _unknown.Value;

        /// <summary>
        /// Gets a flag that shows whether or not the function is a constant value.
        /// </summary>
        /// <value>
        ///   <c>true</c> the function is constant; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstant => _unknown.IsFixed;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFunction"/> class.
        /// </summary>
        /// <param name="unknown">The unknown.</param>
        /// <exception cref="ArgumentNullException">unknown</exception>
        public UnknownFunction(Unknown unknown)
        {
            _unknown = unknown ?? throw new ArgumentNullException(nameof(unknown));
        }

        /// <summary>
        /// Sets up the function for the specified solver.
        /// </summary>
        /// <param name="coefficient">The coefficient.</param>
        /// <param name="equations">The produced equations for each unknown.</param>
        public override void Differentiate(Function coefficient, Dictionary<Unknown, Function> equations)
        {
            if (_unknown.IsFixed)
                return;

            if (coefficient == null)
                coefficient = 1.0;

            // Add the coefficient to the specified equation
            if (equations.TryGetValue(_unknown, out var eq))
                equations[_unknown] = eq + coefficient;
            else
                equations.Add(_unknown, coefficient);
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
            if (!_unknown.IsFixed)
                return new RowEquation(_unknown, solver, row, mapper.Map(_unknown));
            return new ConstantRowEquation(_unknown, solver, row);
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
            if (!_unknown.IsFixed)
            {
                _unknown.Value = value;
                _unknown.IsFixed = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => _unknown.IsFixed ? $"{_unknown}={Value}" : _unknown.ToString();
    }
}
