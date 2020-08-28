using SimpleCircuit.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Contributions
{
    /// <summary>
    /// A contributor for an orientation, which is basically the in-product with a vector.
    /// </summary>
    /// <seealso cref="IContributor" />
    public class ProjectionContributor : IContributor
    {
        private readonly IContributor _sx, _sy, _a;
        private readonly double _angle;
        private readonly Vector2 _normal;

        /// <inheritdoc/>
        public UnknownTypes Type => UnknownTypes.Scalar;

        /// <inheritdoc/>
        public double Value
        {
            get
            {
                var a = _a.Value;
                var value = 0.0;
                if (!_normal.X.IsZero())
                {
                    var dx = _normal.X * Math.Cos(a);
                    if (_sx.IsFixed)
                        dx *= _sx.Value;
                    value += dx;
                }
                if (!_normal.Y.IsZero())
                {
                    var dy = _normal.Y * Math.Sin(a);
                    if (_sy.IsFixed)
                        dy *= _sy.Value;
                    value += dy;
                }
                return value;
            }
        }

        /// <inheritdoc/>
        public bool IsFixed
        {
            get
            {
                // No orientation
                if (!_a.IsFixed)
                    return false;
                if (!_sx.IsFixed)
                {
                    var c = Math.Cos(_a.Value);
                    if (!(c * _normal.X).IsZero())
                        return false;
                }
                if (!_sy.IsFixed)
                {
                    var s = Math.Sin(_a.Value);
                    if (!(s * _normal.Y).IsZero())
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionContributor" /> class.
        /// </summary>
        /// <param name="sx">The x-scale contributor.</param>
        /// <param name="sy">The y-scale contributor.</param>
        /// <param name="a">The angle contributor.</param>
        /// <param name="angle">The angle offset.</param>
        /// <param name="normal">The normal with which the in-product is calculated.</param>
        /// <exception cref="ArgumentNullException">Thrown if any contributor is <c>null</c>.</exception>
        public ProjectionContributor(IContributor sx, IContributor sy, IContributor a, double angle, Vector2 normal)
        {
            _sx = sx ?? throw new ArgumentNullException(nameof(sx));
            _sy = sy ?? throw new ArgumentNullException(nameof(sy));
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _angle = angle;
            if (normal.X.IsZero() && normal.Y.IsZero())
                throw new ArgumentException("Normal is (0, 0).");
            _normal = normal / Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
        }

        /// <inheritdoc/>
        public IContribution CreateContribution(ISparseSolver<double> solver, int row, UnknownSolverMap map)
        {
            IContribution sx, sy;
            if (Math.Cos(_angle).IsZero())
                sx = new ConstantContribution(solver, row, 1.0, UnknownTypes.ScaleX);
            else
                sx = _sx.CreateContribution(solver, row, map);
            if (Math.Sin(_angle).IsZero())
                sy = new ConstantContribution(solver, row, 1.0, UnknownTypes.ScaleY);
            else
                sy = _sy.CreateContribution(solver, row, map);
            var a = _a.CreateContribution(solver, row, map);
            return new ProjectionContribution(sx, sy, a, _angle, _normal);
        }

        /// <inheritdoc/>
        public bool Fix(double value)
        {
            double px = Math.Cos(_angle), py = Math.Sin(_angle);
            double dx1 = px * _normal.X, dx2 = py * _normal.Y;
            double dy1 = px * _normal.Y, dy2 = -py * _normal.X;

            // Assume that we need to determine the angle
            if (!_a.IsFixed)
            {
                if (!_sx.IsFixed)
                {
                    if (!dx1.IsZero() || !dy1.IsZero())
                        return false;
                }
                else
                {
                    dx1 *= _sx.Value;
                    dy1 *= _sx.Value;
                }
                if (!_sy.IsFixed)
                {
                    if (!dx2.IsZero() || !dy2.IsZero())
                        return false;
                }
                else
                {
                    dx2 *= _sy.Value;
                    dy2 *= _sy.Value;
                }

                // We can only fix the orientation if the in-product is +1 or -1.
                if (value > 0)
                {
                    if (!(value - 1).IsZero())
                        return false;
                    return _a.Fix(Math.Atan2(dy1 + dy2, dx1 + dx2));
                }
                else
                {
                    if (!(value + 1).IsZero())
                        return false;
                    return _a.Fix(Utility.Wrap(Math.Atan2(-dy1 - dy2, -dx1 - dx2)));
                }
            }

            // We have an angle, but no scale along the X-axis?
            double ax = Math.Cos(_a.Value), ay = Math.Sin(_a.Value);
            double coefSx = dx1 * ax + dy1 * ay;
            double coefSy = -dx2 * ay + dy2 * ax;
            if (!_sx.IsFixed && !coefSx.IsZero())
            {
                if (!_sy.IsFixed)
                {
                    if (!coefSy.IsZero())
                        return false;
                }
                else
                    coefSy *= _sy.Value;
                return _sx.Fix((value - coefSy) / coefSx);
            }
            if (!_sy.IsFixed && !coefSy.IsZero())
            {
                if (!_sx.IsFixed)
                {
                    if (!coefSx.IsZero())
                        return false;
                }
                else
                    coefSx *= _sx.Value;
                return _sy.Fix((value - coefSx) / coefSy);
            }
            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _sx.Reset();
            _sy.Reset();
            _a.Reset();
        }

        /// <inheritdoc/>
        public IEnumerable<int> GetUnknowns(UnknownSolverMap map)
        {
            IEnumerable<int> result = Enumerable.Empty<int>();
            double x = _normal.X, y = _normal.Y;
            if (_a.IsFixed)
            {
                x *= Math.Cos(_a.Value);
                y *= Math.Sin(_a.Value);
            }
            else
                result = result.Union(_a.GetUnknowns(map));
            
            if (!x.IsZero())
                result = result.Union(_sx.GetUnknowns(map));
            if (!y.IsZero())
                result = result.Union(_sy.GetUnknowns(map));
            return result.Distinct();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"N({_a})*{_normal}";
    }
}
