using SimpleCircuit.Components;
using SimpleCircuit.Items;
using SimpleCircuit.Variables;
using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Constraints
{
    public class ExtendedPinConstraint : IConstraint
    {
        private readonly ITranslatingItem _a, _b;
        private readonly IRotatingItem _ra, _rb;
        private readonly PinDescription _pinA, _pinB;
        private Vector2 _relativeA, _relativeB;
        private double _angleOffset;
        private VariableContribution _fxdxa, _fydya, _fxdaa, _fydaa;
        private VariableContribution _fxdxb, _fydyb, _fxdab, _fydab;
        private VariableContribution _fxdl, _fydl;
        private Element<double> _rhsX, _rhsY;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public IVariable Length { get; private set; }

        /// <inheritdoc/>
        public int Rows => 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedPinConstraint"/> class.
        /// </summary>
        /// <param name="a">The first item also owns the pin.</param>
        /// <param name="pinA">The pin that is extended.</param>
        /// <param name="b">The second item.</param>
        /// <param name="pinB">The second pin.</param>
        public ExtendedPinConstraint(ITranslatingItem a, PinDescription pinA, ITranslatingItem b, PinDescription pinB)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _ra = a as IRotatingItem;
            _pinA = pinA ?? throw new ArgumentNullException(nameof(pinA));

            _b = b ?? throw new ArgumentNullException(nameof(b));
            _rb = b as IRotatingItem;
            _pinB = pinB;
        }

        /// <inheritdoc/>
        public void GetVariables(VariableMap variables)
        {
            Length = variables.CreateVariable(null, $"(pin {_pinA} of {_a} to {(_pinB != null ? $"pin {_pinB} of " : "")}{_b}).Length", VariableType.Magnitude);
        }

        /// <inheritdoc/>
        public void Apply()
        {
            // Apply to fx and fy
            _fxdxa.Add(1);
            _fydya.Add(1);
            if (_fxdaa != null)
            {
                var c = Math.Cos(_ra.Angle.Value + _angleOffset);
                var s = Math.Sin(_ra.Angle.Value + _angleOffset);
                var con = new OffsetContributions(_ra.Angle.Value, _relativeA);
                var dfxdaa = con.Dfxda - Length.Value * s;
                var dfydaa = con.Dfyda + Length.Value * c;
                _fxdaa.Add(dfxdaa);
                _fydaa.Add(dfydaa);
                _fxdl.Add(c);
                _fydl.Add(s);
                _rhsX.Add(dfxdaa * _ra.Angle.Value - con.Frx);
                _rhsY.Add(dfydaa * _ra.Angle.Value - con.Fry);
            }
            else
            {
                var c = Math.Cos(_angleOffset);
                var s = Math.Sin(_angleOffset);
                _fxdl.Add(c);
                _fydl.Add(s);
                _rhsX.Subtract(_relativeA.X);
                _rhsY.Subtract(_relativeA.Y);
            }

            _fxdxb.Subtract(1);
            _fydyb.Subtract(1);
            if (_fxdab != null)
            {
                var con = new OffsetContributions(_rb.Angle.Value, _relativeB);
                _fxdab.Subtract(con.Dfxda);
                _fydab.Subtract(con.Dfyda);
                _rhsX.Subtract(con.Dfxda * _rb.Angle.Value - con.Frx);
                _rhsY.Subtract(con.Dfyda * _rb.Angle.Value - con.Fry);
            }
            else
            {
                _rhsX.Add(_relativeB.X);
                _rhsY.Add(_relativeB.Y);
            }
        }

        /// <inheritdoc/>
        public void Setup(ISparseSolver<double> solver, int row)
        {
            _relativeA = _pinA.Relative;
            _relativeB = _pinB.Relative;
            _angleOffset = _pinA.Angle;

            // fx
            int row1 = row;
            _fxdxa = _a.X.GetDerivative(solver, row);
            _fxdxb = _b.X.GetDerivative(solver, row);
            _fxdl = Length.GetDerivative(solver, row);
            _rhsX = solver.GetElement(row);

            // fy
            var row2 = ++row;
            _fydya = _a.Y.GetDerivative(solver, row);
            _fydyb = _b.Y.GetDerivative(solver, row);
            _fydl = Length.GetDerivative(solver, row);
            _rhsY = solver.GetElement(row);

            if (_ra != null)
            {
                _fxdaa = _ra.Angle.GetDerivative(solver, row1);
                _fydaa = _ra.Angle.GetDerivative(solver, row2);
            }
            else
                _fxdaa = _fydaa = null;
            if (_rb != null && (!_relativeB.X.Equals(0.0) || !_relativeB.Y.Equals(0.0)))
            {
                _fxdab = _rb.Angle.GetDerivative(solver, row1);
                _fydab = _rb.Angle.GetDerivative(solver, row2);
            }
            else
                _fxdab = _fydab = null;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Extends {(_pinA != null ? $"pin '{_pinA}' of " : "")}'{_a}' to {(_pinB != null ? $"pin '{_pinB}' of " : "")}'{_b}'";
        }
    }
}
