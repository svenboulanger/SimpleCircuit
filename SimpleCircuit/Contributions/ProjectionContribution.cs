using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A contribution for calculating the orientation of a pin.
    /// </summary>
    /// <seealso cref="IContribution" />
    public class ProjectionContribution : IContribution
    {
        private readonly double _px, _py;
        private readonly IContribution _sx, _sy, _a;
        private double _dfda, _dfdsx, _dfdsy;
        private readonly Element<double> _rhs;
        private readonly Vector2 _normal;

        /// <inheritdoc/>
        public double Value { get; private set; }

        /// <inheritdoc/>
        public int Row => _a.Row;

        /// <inheritdoc/>
        public HashSet<int> Unknowns
        {
            get
            {
                var combined = new HashSet<int>();
                combined.UnionWith(_sx.Unknowns ?? Enumerable.Empty<int>());
                combined.UnionWith(_sy.Unknowns ?? Enumerable.Empty<int>());
                combined.UnionWith(_a.Unknowns ?? Enumerable.Empty<int>());
                return combined;
            }
        }

        /// <inheritdoc/>
        public ISparseSolver<double> Solver => _a.Solver;

        public ProjectionContribution(IContribution sx, IContribution sy, IContribution a, double angle, Vector2 normal)
        {
            _sx = sx ?? throw new ArgumentNullException(nameof(sx));
            _sy = sy ?? throw new ArgumentNullException(nameof(sy));
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _px = Math.Cos(angle);
            _py = Math.Sin(angle);
            _normal = normal;
            _rhs = a.Solver.GetElement(a.Row);
        }

        /// <inheritdoc/>
        public void Add(double derivative, Element<double> rhs)
        {
            if (rhs == null)
            {
                rhs = _rhs;
                rhs.Subtract(derivative * Value);
            }
            _a.Add(derivative * _dfda, rhs);
            _sx.Add(derivative * _dfdsx, rhs);
            _sy.Add(derivative * _dfdsy, rhs);
        }

        /// <inheritdoc/>
        public void Update(IVector<double> solution)
        {
            _sx.Update(solution);
            _sy.Update(solution);
            _a.Update(solution);

            var ca = Math.Cos(_a.Value);
            var sa = Math.Sin(_a.Value);
            Value = (_sx.Value * _px * ca - _sy.Value * _py * sa) * _normal.X +
                (_sx.Value * _px * sa + _sy.Value * _py * ca) * _normal.Y;
            _dfda = -(_sx.Value * _px * sa + _sy.Value * _py * ca) * _normal.X +
                (_sx.Value * _px * ca - _sy.Value * _py * sa) * _normal.Y;
            _dfdsx = _px * ca * _normal.X + _px * sa * _normal.Y;
            _dfdsy = -_py * sa * _normal.X + _py * ca * _normal.Y;
        }
    }
}
