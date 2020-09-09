using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A derivative for an angled offset to a variable along the X-direction.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class OffsetYContribution : IContribution
    {
        private readonly Vector2 _relative;
        private readonly IContribution _y, _sx, _sy, _a;
        private double _dfda, _dfdsx, _dfdsy;
        private Element<double> _rhs;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _y.Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _y.Solver;

        /// <inheritdoc/>
        public IEnumerable<int> Unknowns =>
            _y.Unknowns
                .Union(_sx.Unknowns)
                .Union(_sy.Unknowns)
                .Union(_a.Unknowns);

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetXContribution"/> class.
        /// </summary>
        /// <param name="y">The contribution for the y-coordinate.</param>
        /// <param name="sx">The contribution for scaling along the x-axis.</param>
        /// <param name="sy">The contribution for scaling along the y-axis.</param>
        /// <param name="a">The contribution for the angle.</param>
        /// <param name="relative">The relative position.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="y"/>, <paramref name="sx"/>, <paramref name="sy"/> or <paramref name="a"/> is <c>null</c>.</exception>
        public OffsetYContribution(IContribution y, IContribution sx, IContribution sy, IContribution a, Vector2 relative)
        {
            _y = y ?? throw new ArgumentNullException(nameof(y));
            _sx = sx ?? throw new ArgumentNullException(nameof(sx));
            _sy = sy ?? throw new ArgumentNullException(nameof(sy));
            _a = a ?? throw new ArgumentNullException(nameof(a));
            if (_y.Solver != _a.Solver || _y.Row != _a.Row)
                throw new ArgumentException("Cannot combine contributions from different solvers or rows.");
            _relative = relative;
            if (a != null && (!relative.X.Equals(0.0) || !relative.Y.Equals(0.0)) ||
                a == null && !relative.X.Equals(0.0))
                _rhs = a.Solver.GetElement(a.Row);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _y.Update(solution);
            _sx.Update(solution);
            _sy.Update(solution);
            _a.Update(solution);
            var c = Math.Cos(_a.Value);
            var s = Math.Sin(_a.Value);
            Value = _y.Value + _sx.Value * _relative.X * s + _sy.Value * _relative.Y * c;
            _dfda = _sx.Value * _relative.X * c - _sy.Value * _relative.Y * s;
            _dfdsx = _relative.X * s;
            _dfdsy = _relative.Y * c;
        }

        /// <inheritdoc/>
        public void Add(double factor, Element<double> rhs)
        {
            if (rhs == null && _rhs != null)
            {
                rhs = _rhs;
                rhs.Subtract(factor * Value);
            }

            _y.Add(factor, rhs);
            _sx.Add(factor * _dfdsx, rhs);
            _sy.Add(factor * _dfdsy, rhs);
            _a.Add(factor * _dfda, rhs);
        }
    }
}
