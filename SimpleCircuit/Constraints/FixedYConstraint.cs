using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Fixes the y-coordinate of a point.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class FixedYConstraint : IConstraint
    {
        private readonly ITranslatingItem _a;
        private readonly IRotatingItem _ra;
        private readonly PinDescription _pin;
        private Vector2 _offset;
        private readonly double _y;
        private VariableContribution _da, _daAngle;
        private Element<double> _rhs;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedYConstraint"/> class.
        /// </summary>
        /// <param name="a">The translating item.</param>
        /// <param name="y">The y-coordinate.</param>
        public FixedYConstraint(ITranslatingItem a, double y)
            : this(a, null, y)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedYConstraint"/> class.
        /// </summary>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="pin">The pin description.</param>
        /// <param name="a">The translating item.</param>
        /// <exception cref="ArgumentNullException">a</exception>
        public FixedYConstraint(ITranslatingItem a, PinDescription pin, double y)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _ra = a as IRotatingItem;
            _pin = pin;
            _y = y;
        }


        public int Rows => 1;
        public void GetVariables(VariableMap variables)
        {
        }

        /// <inheritdoc/>
        public void Apply()
        {
            _da.Add(1.0);
            if (_daAngle != null)
            {
                var con = new OffsetContributions(_ra.Angle.Value, _offset);
                _daAngle.Add(con.Dfyda);
                _rhs.Add(con.Dfyda * _ra.Angle.Value - con.Fry);
            }
            else
                _rhs.Subtract(_offset.Y);
            _rhs.Add(_y);
        }

        /// <inheritdoc/>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _offset = _pin?.Relative ?? new Vector2();
            _da = _a.Y.GetDerivative(solver, row);
            _rhs = solver.GetElement(row);
            if (_ra != null && (!_offset.X.Equals(0.0) || !_offset.Y.Equals(0.0)))
                _daAngle = _ra.Angle.GetDerivative(solver, row);
            else
                _daAngle = null;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Fixes Y-coordinate of {(_pin != null ? $"pin '{_pin}' of " : "")}'{_a}' to {_y:G3}";
        }
    }
}
