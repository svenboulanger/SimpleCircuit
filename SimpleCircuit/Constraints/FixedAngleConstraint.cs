using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;

namespace SimpleCircuit.Constraints
{
    /// <summary>
    /// Fixes the angle of a rotating component.
    /// </summary>
    /// <seealso cref="IConstraint" />
    public class FixedAngleConstraint : IConstraint
    {
        private readonly IRotatingItem _a;
        private readonly PinDescription _pinA;
        private readonly double _angle;
        private double _offset;
        private VariableContribution _da;
        private Element<double> _rhs;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedAngleConstraint"/> class.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="angle">The angle.</param>
        /// <exception cref="ArgumentNullException">a</exception>
        public FixedAngleConstraint(IRotatingItem a, PinDescription pinA, double angle = 0.0)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _pinA = pinA;
            _angle = Angles.Wrap(angle);
        }

        public void Setup(ISparseSolver<double> solver, int row, VariableMap variables)
        {
            var _da = new 
        }

        /// <inheritdoc/>
        public void Apply()
        {
            // f = a - a_o = 0
            // df/da = 1
            _da.Add(1.0);
            _rhs?.Add(_offset);
        }

        /// <inheritdoc/>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _offset = Angles.Wrap(_angle - (_pinA?.Angle ?? 0.0));
            _da = _a.Angle.GetDerivative(solver, row);
            _rhs = !_offset.Equals(0.0) ? solver.GetElement(row) : null;
        }

        public override string ToString()
        {
            return $"Fixes orientation of {(_pinA != null ? $"pin '{_pinA}' of " : "")}'{_a}' to angle {_angle * 180 / Math.PI:G3}";
        }
    }
}
