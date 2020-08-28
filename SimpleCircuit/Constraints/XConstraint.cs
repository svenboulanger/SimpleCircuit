using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Constrains the x-coordinates of two translating items to each other.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class XConstraint : IConstraint
    {
        private readonly VariableContribution _dx1, _dx2;
        private readonly Element<double> _rhs;
        private readonly double _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="XConstraint" /> class.
        /// </summary>
        /// <param name="dx1">The first variable's derivative.</param>
        /// <param name="dx2">The second variable's derivative.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dx1"/> or <paramref name="dx2"/> is <c>null</c>.</exception>
        public XConstraint(VariableContribution dx1, VariableContribution dx2, double offset = 0.0)
        {
            _dx1 = dx1 ?? throw new ArgumentNullException(nameof(dx1));
            _dx2 = dx2 ?? throw new ArgumentNullException(nameof(dx2));
            _offset = offset;
        }

        /// <inheritdoc/>
        public void Apply()
        {
            // f(x1, x2) = x1 - x2 - offset = 0
            // df = dx1 - dx2
            _dx1.Add(1.0);
            _dx2.Add(-1.0);

            _da.Add(1.0);
            if (_daAngle != null)
            {
                var con = new OffsetContributions(_ra.Angle.Value, _relativeA);
                _daAngle.Add(con.Dfxda);
                _rhs.Add(con.Dfxda * _ra.Angle.Value - con.Frx);
            }
            else
                _rhs.Subtract(_relativeA.X);

            _db.Subtract(1.0);
            if (_dbAngle != null)
            {
                var con = new OffsetContributions(_rb.Angle.Value, _relativeB);
                _dbAngle.Subtract(con.Dfxda);
                _rhs.Subtract(con.Dfxda * _rb.Angle.Value - con.Frx);
            }
            else
                _rhs.Add(_relativeB.X);

            _rhs.Add(_offset);
        }

        /// <inheritdoc/>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _relativeA = _pinA?.Relative ?? new Vector2();
            _relativeB = _pinB?.Relative ?? new Vector2();
            _da = _a.X.GetDerivative(solver, row);
            _db = _b.X.GetDerivative(solver, row);
            _rhs = solver.GetElement(row);

            // Can A rotate?
            if (_ra != null && (!_relativeA.X.Equals(0.0) || !_relativeA.Y.Equals(0.0)))
                _daAngle = _ra.Angle.GetDerivative(solver, row);
            else
                _daAngle = null;

            // Can B rotate?
            if (_rb != null && (!_relativeB.X.Equals(0.0) || !_relativeB.Y.Equals(0.0)))
                _dbAngle = _rb.Angle.GetDerivative(solver, row);
            else
                _dbAngle = null;
        }

        public override string ToString()
        {
            return $"Fixes X of {(_pinA != null ? $"pin '{_pinA}' of " : "")}'{_a}' to {(_pinB != null ? $"pin '{_pinB}' of " : "")}'{_b}' with offset {_offset:G3}";
        }
    }
}
