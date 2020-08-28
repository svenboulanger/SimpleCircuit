using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributions.Contributors
{
    /// <summary>
    /// A contributor that represents an offset to a parent object that can rotate, scale and move.
    /// </summary>
    /// <seealso cref="IContributor" />
    public class OffsetXContributor : IContributor
    {
        private readonly IContributor _x, _sx, _sy, _a;
        private readonly Vector2 _relative;

        /// <inheritdoc/>
        public UnknownTypes Type => UnknownTypes.X;

        /// <inheritdoc/>
        public double Value => _x.Value + _sx.Value * _relative.X * Math.Cos(_a.Value) - _sy.Value * _relative.Y * Math.Sin(_a.Value);

        /// <inheritdoc/>
        public bool IsFixed
        {
            get
            {
                if (_x.IsFixed)
                {
                    // There is no offset, so orientation and scaling has no influence
                    if (_relative.X.Equals(0.0) && _relative.Y.Equals(0.0))
                        return true;

                    // If the orientation and scaling is fixed, then we can 
                    if (_a.IsFixed)
                    {
                        var a = _a.Value;
                        var dx = _relative.X * Math.Cos(a);
                        var dy = _relative.Y * Math.Sin(a);
                        if ((_sx.IsFixed || dx.IsZero()) &&
                            (_sy.IsFixed || dy.IsZero()))
                            return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetXContributor" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="sx">The sx.</param>
        /// <param name="sy">The sy.</param>
        /// <param name="a">a.</param>
        /// <param name="relative">The relative.</param>
        public OffsetXContributor(IContributor x, IContributor sx, IContributor sy, IContributor a, Vector2 relative)
        {
            _x = x ?? throw new ArgumentNullException(nameof(x));
            _sx = sx ?? throw new ArgumentNullException(nameof(sx));
            _sy = sy ?? throw new ArgumentNullException(nameof(sy));
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _relative = relative;
        }

        /// <inheritdoc/>
        public IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
        {
            // Let's try to save some unnecessary unknown calculations
            IContribution sx, sy, a;
            if (!_sx.IsFixed && _relative.X.IsZero())
                sx = new ConstantContribution(solver, row, 1.0, UnknownTypes.ScaleX);
            else
                sx = _sx.CreateContribution(solver, row, map);
            if (!_sy.IsFixed && _relative.Y.IsZero())
                sy = new ConstantContribution(solver, row, 1.0, UnknownTypes.ScaleY);
            else
                sy = _sy.CreateContribution(solver, row, map);
            if (_relative.X.IsZero() && _relative.Y.IsZero())
                a = new ConstantContribution(solver, row, 0.0, UnknownTypes.Angle);
            else
                a = _a.CreateContribution(solver, row, map);
            return new OffsetXContribution(_x.CreateContribution(solver, row, map), sx, sy, a, _relative);
        }

        /// <inheritdoc/>
        public bool Fix(double value)
        {
            // No offset?
            if (_relative.X.Equals(0.0) && _relative.Y.Equals(0.0))
                return _x.Fix(value);

            // Orientations always lead to two possible answers (or none). So we can't use it to fix anything...
            if (!_a.IsFixed)
                return false;
            var a = _a.Value;
            var dx = _relative.X * Math.Cos(a);
            var dy = _relative.Y * Math.Sin(a);

            // Can we fix X?
            if (!_x.IsFixed)
            {
                if (!_sx.IsFixed)
                {
                    if (!dx.IsZero())
                        return false;
                }
                else
                    dx *= _sx.Value;
                if (_sy.IsFixed)
                {
                    if (!dy.IsZero())
                        return false;
                }
                else
                    dy *= _sy.Value;
                return _x.Fix(value - dx + dy);
            }

            // We can only figure out the X-scale if there is something to work with
            if (!_sx.IsFixed && !dx.IsZero())
            {
                // Only if the Y-scale is fixed
                if (!_sy.IsFixed)
                {
                    if (!dy.IsZero())
                        return false;
                }
                else
                    dy *= _sy.Value;
                return _sx.Fix((value - _x.Value + dy) / dx);
            }
            if (!_sy.IsFixed && !dy.IsZero())
            {
                // Only if the X-scale is fixed
                if (!_sx.IsFixed)
                {
                    if (!dx.IsZero())
                        return false;
                }
                else
                    dx *= _sx.Value;
                return _sy.Fix(-(value - _x.Value - dx) / dy);
            }
            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _x.Reset();
            _sx.Reset();
            _sy.Reset();
            _a.Reset();
        }

        /// <inheritdoc/>
        public IEnumerable<int> GetUnknowns(UnknownSolverMap map)
        {
            var result = _x.GetUnknowns(map);
            double c = 1.0, s = 1.0;
            if (_a.IsFixed)
            {
                c = Math.Cos(_a.Value);
                s = Math.Sin(_a.Value);
            }
            else if (!_relative.X.IsZero() || !_relative.Y.IsZero())
                result = result.Union(_a.GetUnknowns(map));
            if (!(c * _relative.X).IsZero())
                result = result.Union(_sx.GetUnknowns(map));
            if (!(s * _relative.Y).IsZero())
                result = result.Union(_sy.GetUnknowns(map));
            return result.Distinct();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"OffsetX({_x}, {_a})";
    }
}
