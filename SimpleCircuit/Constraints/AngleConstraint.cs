using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// A constraint between two angled items.
    /// </summary>
    public class AngleConstraint : IConstraint
    {
        private readonly IRotatingItem _a, _b;
        private readonly PinDescription _pinA, _pinB;
        private readonly double _angle;
        private double _offset;
        private VariableContribution _da, _db;
        private Element<double> _rhs;

        /// <summary>
        /// Initializes a new instance of the <see cref="AngleConstraint"/> class.
        /// </summary>
        /// <param name="a">The first item that needs to be angled <paramref name="angle"/> relative to <paramref name="b"/>.</param>
        /// <param name="b">The second item.</param>
        /// <param name="angle">The fixed angle.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="a"/> or <paramref name="b"/> is <c>null</c>.</exception>
        public AngleConstraint(IRotatingItem a, IRotatingItem b, double angle)
            : this(a, b, null, null, angle)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AngleConstraint"/> class.
        /// </summary>
        /// <param name="a">The first item that needs to be angled <paramref name="angle"/> relative to <paramref name="b"/>.</param>
        /// <param name="b">The second item.</param>
        /// <param name="pinA">The pin of the first item.</param>
        /// <param name="pinB">The pin of the second item.</param>
        /// <param name="angle">The fixed angle.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="a"/> or <paramref name="b"/> is <c>null</c>.</exception>
        public AngleConstraint(IRotatingItem a, IRotatingItem b, PinDescription pinA, PinDescription pinB, double angle = 0.0)
        {
            _angle = angle;
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            _pinA = pinA;
            _pinB = pinB;
        }

        public int Rows => 1;
        public void GetVariables(VariableMap variables)
        {
        }

        /// <summary>
        /// Sets up the elements necessary for the constraint.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="vector">The vector.</param>
        /// <param name="row">The current row.</param>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            // (a1 + a_p1) - (a2 + a_p2) = _angle
            // a1 - a2 = _angle + a_p2 - a_p1
            var offsetA = _pinA?.Angle ?? 0.0;
            var offsetB = _pinB?.Angle ?? 0.0;
            _offset = Angles.Wrap(_angle + offsetB - offsetA);
            _da = _a.Angle.GetDerivative(solver, row);
            _db = _b.Angle.GetDerivative(solver, row);
            _rhs = !_offset.Equals(0.0) ? solver.GetElement(row) : null;
        }

        /// <summary>
        /// Applies the constraint.
        /// </summary>
        public void Apply()
        {
            // f = a_ang - b_ang - _offset = 0
            _da.Add(1.0);
            _db.Add(-1.0);
            _rhs?.Add(_offset);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Fixes orientation of {(_pinA != null ? $"pin '{_pinA}' of " : "")}'{_a}' to {(_pinB != null ? $"pin '{_pinB}' of " : "")}'{_b}' with offset {_angle * 180 / Math.PI:G3}";
        }
    }
}
