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
    public class OffsetXContribution : IContribution
    {
        private readonly Vector2 _relative;
        private readonly IContribution _x, _sx, _sy, _a;
        private double _dfda, _dfdsx, _dfdsy;
        private readonly Element<double> _rhs;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _x.Row;

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _x.Solver;

        /// <inheritdoc/>
        public HashSet<int> Unknowns
        {
            get
            {
                var combined = new HashSet<int>();
                combined.UnionWith(_x.Unknowns ?? Enumerable.Empty<int>());
                combined.UnionWith(_sx.Unknowns ?? Enumerable.Empty<int>());
                combined.UnionWith(_sy.Unknowns ?? Enumerable.Empty<int>());
                combined.UnionWith(_a.Unknowns ?? Enumerable.Empty<int>());
                return combined;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetXContribution"/> class.
        /// </summary>
        /// <param name="x">The contribution for the x-coordinate.</param>
        /// <param name="sx">The relative x-scale.</param>
        /// <param name="sy">The relative y-scale.</param>
        /// <param name="a">The contribution for the angle.</param>
        /// <param name="relative">The relative position.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="x"/>, <paramref name="sx"/>, <paramref name="sy"/> or <paramref name="a"/> is <c>null</c>.</exception>
        public OffsetXContribution(IContribution x, IContribution sx, IContribution sy, IContribution a, Vector2 relative)
        {
            _x = x ?? throw new ArgumentNullException(nameof(x));
            _sx = sx ?? throw new ArgumentNullException(nameof(sx));
            _sy = sy ?? throw new ArgumentNullException(nameof(sy));
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _relative = relative;

            // If there is no offset, just skip this part
            if (!_relative.X.IsZero() || !_relative.Y.IsZero())
                _rhs = a.Solver.GetElement(a.Row);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _x.Update(solution);
            _sx.Update(solution);
            _sy.Update(solution);
            _a.Update(solution);
            var c = Math.Cos(_a.Value);
            var s = Math.Sin(_a.Value);
            Value = _x.Value +  _sx.Value * _relative.X * c - _sy.Value * _relative.Y * s;
            _dfda = -(_sx.Value * _relative.X * s + _sy.Value * _relative.Y * c);
            _dfdsx = c * _relative.X;
            _dfdsy = -s * _relative.Y;
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            if (rhs == null && _rhs != null)
            {
                rhs = _rhs;
                rhs.Subtract(derivative * Value);
            }
            _x.Add(derivative, rhs);
            _sx.Add(derivative * _dfdsx, rhs);
            _sy.Add(derivative * _dfdsy, rhs);
            _a.Add(derivative * _dfda, rhs);
        }
    }
}
