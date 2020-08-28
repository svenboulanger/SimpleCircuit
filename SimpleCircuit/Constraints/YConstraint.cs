using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Constraints the position of two elements along the vertical axis.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class YConstraint : IConstraint
    {
        private readonly ITranslatingItem _a, _b;
        private readonly IRotatingItem _ra, _rb;
        private readonly PinDescription _pinA, _pinB;
        private Vector2 _relativeA, _relativeB;
        private VariableContribution _da, _daAngle;
        private VariableContribution _db, _dbAngle;
        private Element<double> _rhs;
        private readonly double _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="YConstraint"/> class.
        /// </summary>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <param name="pinA">The pin description for item a.</param>
        /// <param name="pinB">The pin description for item b.</param>
        public YConstraint(ITranslatingItem a, ITranslatingItem b, PinDescription pinA, PinDescription pinB)
            : this(a, b, pinA, pinB, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YConstraint"/> class.
        /// </summary>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <param name="offset">The offset.</param>
        public YConstraint(ITranslatingItem a, ITranslatingItem b, double offset)
            : this(a, b, null, null, offset)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YConstraint"/> class.
        /// </summary>
        /// <param name="a">The first item.</param>
        /// <param name="b">The second item.</param>
        /// <param name="pinA">The pin description for item a.</param>
        /// <param name="pinB">The pin description for item b.</param>
        /// <param name="offset">The offset.</param>
        public YConstraint(ITranslatingItem a, ITranslatingItem b, PinDescription pinA, PinDescription pinB, double offset)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _ra = a as IRotatingItem;
            _pinA = pinA;

            _b = b ?? throw new ArgumentNullException(nameof(b));
            _rb = b as IRotatingItem;
            _pinB = pinB;

            _offset = offset;
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
                var con = new OffsetContributions(_ra.Angle.Value, _relativeA);
                _daAngle.Add(con.Dfyda);
                _rhs.Add(con.Dfyda * _ra.Angle.Value - con.Fry);
            }
            else
                _rhs.Subtract(_relativeA.Y);

            _db.Subtract(1.0);
            if (_dbAngle != null)
            {
                var con = new OffsetContributions(_rb.Angle.Value, _relativeB);
                _dbAngle.Subtract(con.Dfyda);
                _rhs.Subtract(con.Dfyda * _rb.Angle.Value - con.Fry);
            }
            else
                _rhs.Add(_relativeB.Y);

            _rhs.Add(_offset);
        }

        /// <inheritdoc/>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _relativeA = _pinA?.Relative ?? new Vector2();
            _relativeB = _pinB?.Relative ?? new Vector2();
            _da = _a.Y.GetDerivative(solver, row);
            _db = _b.Y.GetDerivative(solver, row);
            _rhs = solver.GetElement(row);

            // Can A rotate?
            if (_ra != null && (!_relativeA.X.Equals(0.0) || _relativeA.Y.Equals(0.0)))
                _daAngle = _ra.Angle.GetDerivative(solver, row);
            else
                _daAngle = null;

            // Can B rotate?
            if (_rb != null && (!_relativeB.X.Equals(0.0) || _relativeB.Y.Equals(0.0)))
                _dbAngle = _rb.Angle.GetDerivative(solver, row);
            else
                _dbAngle = null;
        }

        public override string ToString()
        {
            return $"Fixes Y of {(_pinA != null ? $"pin '{_pinA}' of " : "")}'{_a}' to {(_pinB != null ? $"pin '{_pinB}' of " : "")}'{_b}' with offset {_offset:G3}";
        }
    }
}
