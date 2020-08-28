using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Fixes the x-coordinate of a point.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class FixedXConstraint : IConstraint
    {
        private readonly ITranslatingItem _a;
        private readonly IRotatingItem _ra;
        private readonly PinDescription _pin;
        private Vector2 _offset;
        private readonly double _x;
        private VariableContribution _da, _daAngle;
        private Element<double> _rhs;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedXConstraint"/> class.
        /// </summary>
        /// <param name="a">The translating item.</param>
        /// <param name="x">The x-coordinate.</param>
        public FixedXConstraint(ITranslatingItem a, double x)
            : this(a, null, x)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedXConstraint"/> class.
        /// </summary>
        /// <param name="a">The translating item.</param>
        /// <param name="pin">The pin description.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <exception cref="ArgumentNullException">a</exception>
        public FixedXConstraint(ITranslatingItem a, PinDescription pin, double x)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _ra = a as IRotatingItem;
            _pin = pin;
            _x = x;
        }

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
                _daAngle.Add(con.Dfxda);
                _rhs.Add(con.Dfxda * _ra.Angle.Value - con.Frx);
            }
            else
                _rhs.Subtract(_offset.Y);
            _rhs.Add(_x);
        }

        /// <inheritdoc/>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _offset = _pin?.Relative ?? new Vector2();
            _da = _a.X.GetDerivative(solver, row);
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
            return $"Fixes X-coordinate of {(_pin != null ? $"pin '{_pin}' of " : "")}'{_a}' to {_x:G3}";
        }
    }
}
