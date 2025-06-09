using System;
using System.Text;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// A class for building an SVG path.
    /// </summary>
    /// <remarks>
    /// Creates a new path builder.
    /// </remarks>
    /// <param name="transform">The transform.</param>
    public class SvgPathBuilder(Transform transform, ExpandableBounds bounds) : IPathBuilder
    {
        private readonly StringBuilder _sb = new();
        private bool _isFirst = true;
        private readonly ExpandableBounds _bounds = bounds;
        private readonly Transform _transform = transform;
        private Vector2 _p1, _n1, _h1, _p2, _n2, _h2; // The local coordinate handles and points
        private Vector2 _last; // The last global coordinate
        private char _impliedAction = '\0';

        /// <inheritdoc />
        public Vector2 Start => _p1;

        /// <inheritdoc />
        public Vector2 End => _p2;

        /// <summary>
        /// Gets the normal of the path at the starting point of the last drawn segment.
        /// </summary>
        public Vector2 StartNormal => _n1;

        /// <summary>
        /// Gets the normal of the path at the ending point of the last drawn segment.
        /// </summary>
        public Vector2 EndNormal => _n2;

        /// <summary>
        /// Initializes the path to the correct origin.
        /// </summary>
        private void InitializePath()
        {
            if (_isFirst && !_transform.Offset.IsZero())
            {
                Append($"M{_transform.Offset.ToSVG()}");
                _last = _transform.Offset;
            }
        }

        /// <inheritdoc />
        public IPathBuilder MoveTo(Vector2 location)
        {
            // Local coordinate space
            _p1 = _h1 = _p2;
            _p2 = _h2 = location;
            _n1 = _n2 = new();

            // Draw in global coordinate space
            location = _transform.Apply(location);
            Append($"{Action('M', 'L')}{location.ToSVG()}");
            _last = location;
            _bounds.Expand(location);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Move(Vector2 delta)
        {
            InitializePath();
            _p1 = _h1 = _p2;
            _p2 += delta;
            _h2 = _p2;

            _n1 = new();
            _n2 = new();

            // Draw in global coordinate space
            delta = _transform.ApplyDirection(delta);
            Append($"{Action('m', 'l')}{delta.ToSVG()}");
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        private void AppendLine(Vector2 delta)
        {
            bool horiz = delta.Y.IsZero();
            bool vert = delta.X.IsZero();
            if (horiz && vert)
                return;
            else if (horiz)
                Append($"{Action('h')}{delta.X.ToSVG()}");
            else if (vert)
                Append($"{Action('v')}{delta.Y.ToSVG()}");
            else
                Append($"{Action('l')}{delta.ToSVG()}");
        }

        /// <inheritdoc />
        public IPathBuilder LineTo(Vector2 location)
        {
            InitializePath();

            // Local coordinate space
            _p1 = _p2;
            _h1 = _p1;
            _p2 = location;
            _h2 = location;

            var delta = _p2 - _p1;
            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = delta / delta.Length;
            _n2 = _n1;

            // Global coordinate space
            delta = _transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Line(Vector2 delta)
        {
            InitializePath();

            // Local coordinate space
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p1;

            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = delta / delta.Length;
            _n2 = _n1;

            // Draw in global coordinate space
            delta = _transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder HorizontalTo(double x)
        {
            InitializePath();

            Vector2 delta = new(x - _p2.X, 0);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = new(Math.Sign(delta.X), 0);
            _n2 = _n1;

            // Draw in global coordinate space
            delta = _transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Horizontal(double dx)
        {
            InitializePath();

            Vector2 delta = new(dx, 0);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = new(Math.Sign(delta.X), 0);
            _n2 = delta;

            // Draw in global coordinate space
            delta = _transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder VerticalTo(double y)
        {
            InitializePath();

            Vector2 delta = new(0, y - _p2.Y);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = new(0, Math.Sign(delta.Y));
            _n2 = _n1;

            // Draw in global coordinate space
            delta = _transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Vertical(double dy)
        {
            InitializePath();

            Vector2 delta = new(0, dy);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (_n1.IsZero())
                _n1 = new();
            else
                _n1 = new(0, Math.Sign(delta.Y));
            _n2 = _n1;

            // Draw in global coordinate space
            delta = _transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        private void CalculateBezierNormals()
        {
            _n1 = _h1 - _p1;
            if (_n1.X.IsZero() && _n1.Y.IsZero())
                _n1 = _h2 - _p1;

            if (_n1.IsZero())
                _n1 = new();
            else
                _n1 /= _n1.Length;

            _n2 = _p2 - _h1;
            if (_n2.X.IsZero() && _n2.Y.IsZero())
                _n2 = _p2 - _h1;

            if (_n2.IsZero())
                _n2 = new();
            else
                _n2 /= _n2.Length;
        }

        /// <inheritdoc />
        public IPathBuilder CurveTo(Vector2 h1, Vector2 h2, Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = h1;
            _p2 = end;
            _h2 = h2;
            CalculateBezierNormals();

            // Draw in global coordinate space
            h1 = _transform.Apply(h1);
            h2 = _transform.Apply(h2);
            end = _transform.Apply(end);
            Append($"{Action('C')}{h1.ToSVG()} {h2.ToSVG()} {end.ToSVG()}");
            _last = end;
            _bounds.Expand([h1, h2, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Curve(Vector2 dh1, Vector2 dh2, Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = _p1 + dh1;
            _h2 = _p1 + dh2;
            _p2 = _p1 + dend;
            CalculateBezierNormals();

            // Draw in global coordinate space
            var h1 = _transform.Apply(_h1);
            var h2 = _transform.Apply(_h2);
            var end = _transform.Apply(_p2);
            Append($"{Action('c')}{(h1 - _last).ToSVG()} {(h2 - _last).ToSVG()} {(end - _last).ToSVG()}");
            _last = end;
            _bounds.Expand([h1, h2, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder SmoothTo(Vector2 h, Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = end;
            _h2 = h;
            CalculateBezierNormals();

            // Draw in global coordinate space
            var h1 = _transform.Apply(_h1);
            var h2 = _transform.Apply(_h2);
            end = _transform.Apply(end);
            Append($"{Action('S')}{h2.ToSVG()} {end.ToSVG()}");
            _last = end;
            _bounds.Expand([h1, h2, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Smooth(Vector2 dh, Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = _p1 + dend;
            _h2 = _p1 + dh;
            CalculateBezierNormals();

            // Draw in global coordinate space
            var h1 = _transform.Apply(_h1);
            var h2 = _transform.Apply(_h2);
            var end = _transform.Apply(_p2);
            Append($"{Action('s')}{(h2 - _last).ToSVG()} {(end - _last).ToSVG()}");
            _last = end;
            _bounds.Expand([h1, h2, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder QuadCurveTo(Vector2 h, Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = h;
            _h2 = h;
            _p2 = end;
            CalculateBezierNormals();

            // Draw in global coordinate space
            h = _transform.Apply(h);
            end = _transform.Apply(end);
            Append($"{Action('Q')}{h.ToSVG()} {end.ToSVG()}");
            _last = end;
            _bounds.Expand([h, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder QuadCurve(Vector2 dh, Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = _p1 + dh;
            _p2 = _p1 + dend;
            _h2 = _h1;
            CalculateBezierNormals();

            // Draw in global coordinate space
            var h = _transform.Apply(_h1);
            var end = _transform.Apply(_p2);
            Append($"{Action('q')}{h.ToSVG()} {end.ToSVG()}");
            _last = end;
            _bounds.Expand([h, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder SmoothQuadTo(Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = end;
            _h2 = _h1;
            CalculateBezierNormals();

            // Draw in global coordinate space
            var h = _transform.Apply(_h1);
            end = _transform.Apply(end);
            Append($"{Action('T')}{end.ToSVG()}");
            _last = end;
            _bounds.Expand([h, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder SmoothQuad(Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = _p1 + dend;
            _h2 = _h1;
            CalculateBezierNormals();

            // Draw in global coordinate space
            var h = _transform.Apply(_h1);
            var end = _transform.Apply(_p2);
            Append($"{Action('t')}{(end - _last).ToSVG()}");
            _last = end;
            _bounds.Expand([h, end]);
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder ArcTo(double rx, double ry, double angle, bool largeArc, bool sweepFlag, Vector2 end)
        {
            InitializePath();

            if (rx.IsZero() || ry.IsZero())
            {
                // Treat as a straight line
                LineTo(end);
                return this;
            }
            rx = Math.Abs(rx);
            ry = Math.Abs(ry);

            Matrix2 rot, irot;
            angle %= 360.0;
            if (angle.IsZero())
                rot = Matrix2.Identity;
            else if ((angle - 90.0).IsZero())
                rot = new(0, -1, 1, 0);
            else if ((angle + 90.0).IsZero())
                rot = new(0, 1, -1, 0);
            else if ((angle + 180).IsZero() || (angle - 180).IsZero())
                rot = new(-1, 0, 0, -1);
            else
                rot = Matrix2.Rotate(angle / 180.0 * Math.PI);
            irot = rot.Transposed; // Possible because transformation is orthonormal

            var p1 = irot * (_p2 - end) * 0.5;
            Vector2 p12 = new(p1.X * p1.X, p1.Y * p1.Y);
            Vector2 r2 = new(rx * rx, ry * ry);
            double cr = p12.X / r2.X + p12.Y / r2.Y;
            if (cr > 1)
            {
                cr = Math.Sqrt(cr);
                rx *= cr;
                ry *= cr;
                r2 = new(rx * rx, ry * ry);
            }

            double dq = r2.X * p12.Y + r2.Y * p12.X;
            double pq = (r2.X * r2.Y - dq) / dq;
            double sc = Math.Sqrt(Math.Max(0.0, pq));
            if (largeArc == sweepFlag)
                sc = -sc;

            Vector2 cp = new(rx * sc * p1.Y / ry, -ry * sc * p1.X / rx);
            Vector2 v = new((p1.X - cp.X) / rx, (p1.Y - cp.Y) / ry);
            double theta1 = Angle(new(1, 0), v);
            double dtheta = Angle(v, new((-p1.X - cp.X) / rx, (-p1.Y - cp.Y) / ry));
            if (sweepFlag)
            {
                if (dtheta < 0)
                    dtheta += 2 * Math.PI;
            }
            else
            {
                if (dtheta > 0)
                    dtheta -= 2 * Math.PI;
            }

            // Approximate using bezier curves
            int segments = (int)Math.Ceiling(Math.Abs(dtheta) / (Math.PI / 2));
            double da = dtheta / segments;
            double hl = 4.0 / 3.0 * Math.Tan(Math.Abs(da) * 0.25);
            double pid2 = Math.Sign(da) * Math.PI * 0.5;
            double currentA = theta1;
            Transform toOriginal = new(0.5 * (_p2 + end), rot);
            var lastPoint = cp + Vector2.Normal(theta1).Scale(rx, ry);
            var lastTangent = Vector2.Normal(theta1 + pid2).Scale(rx, ry) * hl;
            for (int i = 1; i <= segments; i++)
            {
                currentA += da;
                var nextPoint = cp + Vector2.Normal(currentA).Scale(rx, ry);
                var nextTangent = Vector2.Normal(currentA + pid2).Scale(rx, ry) * hl;
                CurveTo(
                    toOriginal.Apply(lastPoint + lastTangent),
                    toOriginal.Apply(nextPoint - nextTangent),
                    toOriginal.Apply(nextPoint)
                    );
                lastPoint = nextPoint;
                lastTangent = nextTangent;
            }
            return this;
        }

        /// <inheritdoc />
        public IPathBuilder Arc(double rx, double ry, double angle, bool largeArc, bool sweepFlag, Vector2 dend)
        {
            InitializePath();

            if (rx.IsZero() || ry.IsZero())
            {
                // Treat as a straight line
                Line(dend);
                return this;
            }
            rx = Math.Abs(rx);
            ry = Math.Abs(ry);
            angle *= Math.PI / 180.0;

            var rot = Matrix2.Rotate(angle);
            var irot = rot.Transposed; // Possible because transformation is orthonormal

            var p1 = irot * -dend * 0.5;
            Vector2 p12 = new(p1.X * p1.X, p1.Y * p1.Y);
            Vector2 r2 = new(rx * rx, ry * ry);
            double cr = p12.X / r2.X + p12.Y / r2.Y;
            if (cr > 1)
            {
                cr = Math.Sqrt(cr);
                rx *= cr;
                ry *= cr;
                r2 = new(rx * rx, ry * ry);
            }

            double dq = r2.X * p12.Y + r2.Y * p12.X;
            double pq = (r2.X * r2.Y - dq) / dq;
            double sc = Math.Sqrt(Math.Max(0.0, pq));
            if (largeArc == sweepFlag)
                sc = -sc;

            Vector2 cp = new(rx * sc * p1.Y / ry, -ry * sc * p1.X / rx);
            Vector2 v = new((p1.X - cp.X) / rx, (p1.Y - cp.Y) / ry);
            double theta1 = Angle(new(1, 0), v);
            double dtheta = Angle(v, new((-p1.X - cp.X) / rx, (-p1.Y - cp.Y) / ry));
            if (sweepFlag)
            {
                if (dtheta < 0)
                    dtheta += 2 * Math.PI;
            }
            else
            {
                if (dtheta > 0)
                    dtheta -= 2 * Math.PI;
            }

            // Approximate using bezier curves
            int segments = (int)Math.Ceiling(Math.Abs(dtheta) / (Math.PI / 2));
            double da = dtheta / segments;
            double hl = 4.0 / 3.0 * Math.Tan(Math.Abs(da) * 0.25);
            double pid2 = Math.Sign(da) * Math.PI * 0.5;
            double currentA = theta1;
            Transform toOriginal = new(_p2 + dend * 0.5, rot);
            var lastPoint = cp + Vector2.Normal(theta1).Scale(rx, ry);
            var lastTangent = Vector2.Normal(theta1 + pid2).Scale(rx, ry) * hl;
            for (int i = 1; i <= segments; i++)
            {
                currentA += da;
                var nextPoint = cp + Vector2.Normal(currentA).Scale(rx, ry);
                var nextTangent = Vector2.Normal(currentA + pid2).Scale(rx, ry) * hl;
                CurveTo(
                    toOriginal.Apply(lastPoint + lastTangent),
                    toOriginal.Apply(nextPoint - nextTangent),
                    toOriginal.Apply(nextPoint)
                    );
                lastPoint = nextPoint;
                lastTangent = nextTangent;
            }
            return this;
        }

        private static double Angle(Vector2 u, Vector2 v)
        {
            double ac = (u.X * v.X + u.Y * v.Y) / (u.Length * v.Length);
            if (ac > 1)
                ac = 1;
            else if (ac < -1)
                ac = -1;
            ac = Math.Acos(ac);
            if (u.X * v.Y < u.Y * v.X)
                return -ac;
            return ac;
        }

        /// <inheritdoc />
        public IPathBuilder Close()
        {
            Append(Action('Z'));
            return this;
        }

        /// <summary>
        /// Appends a command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        private void Append(string cmd)
        {
            if (_isFirst)
                _isFirst = false;
            else
                _sb.Append(' ');
            _sb.Append(cmd);
        }

        /// <summary>
        /// Gets the optional action.
        /// </summary>
        /// <param name="c">The action identifier.</param>
        /// <returns>The action result.</returns>
        private string Action(char c, char nextImplied = '\0')
        {
            char current = _impliedAction;
            _impliedAction = nextImplied == '\0' ? c : nextImplied;
            if (c == current)
                return "";
            return c.ToString();
        }

        /// <summary>
        /// Converts the path builder to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
            => _sb.ToString();
    }
}
