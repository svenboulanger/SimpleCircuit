using System;
using System.Text;

namespace SimpleCircuit.Drawing
{

    /// <summary>
    /// A class for building an SVG path.
    /// </summary>
    public class PathBuilder
    {
        private readonly StringBuilder _sb = new();
        private bool _isFirst = true;
        private readonly ExpandableBounds _bounds;
        private Vector2 _p1, _n1, _h1, _p2, _n2, _h2; // The local coordinate handles and points
        private Vector2 _last; // The last global coordinate
        private char _impliedAction = '\0';

        /// <summary>
        /// Gets the starting point of the last drawn segment.
        /// </summary>
        public Vector2 Start => _p1;

        /// <summary>
        /// Gets the end point of the last drawn segment.
        /// </summary>
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
        /// Gets the bounds of the path that was built.
        /// </summary>
        public Bounds Bounds => _bounds.Bounds;

        /// <summary>
        /// Gets or sets the transform for the path builder.
        /// </summary>
        public Transform Transform { get; } = Transform.Identity;

        /// <summary>
        /// Creates a new path builder.
        /// </summary>
        /// <param name="transform">The transform.</param>
        public PathBuilder(Transform transform)
        {
            _bounds = new();
            Transform = transform;
        }

        /// <summary>
        /// Initializes the path to the correct origin.
        /// </summary>
        private void InitializePath()
        {
            if (_isFirst && !Transform.Offset.IsZero())
            {
                Append($"M{Convert(Transform.Offset)}");
                _last = Transform.Offset;
            }
        }

        /// <summary>
        /// Moves the current point using absolute coordinates.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder MoveTo(Vector2 location)
        {
            // Local coordinate space
            _p1 = _h1 = _p2;
            _p2 = _h2 = location;
            _n1 = _n2 = new();

            // Draw in global coordinate space
            location = Transform.Apply(location);
            Append($"{Action('M', 'L')}{Convert(location)}");
            _last = location;
            _bounds.Expand(location);
            return this;
        }

        /// <summary>
        /// Moves the current point using absolute coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder MoveTo(double x, double y)
            => MoveTo(new(x, y));

        /// <summary>
        /// Moves the current point using relative coordinates.
        /// </summary>
        /// <param name="delta">The step.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Move(Vector2 delta)
        {
            InitializePath();
            _p1 = _h1 = _p2;
            _p2 += delta;
            _h2 = _p2;

            _n1 = new();
            _n2 = new();

            // Draw in global coordinate space
            delta = Transform.ApplyDirection(delta);
            Append($"{Action('m', 'l')}{Convert(delta)}");
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <summary>
        /// Moves the current point using relative coordinates.
        /// </summary>
        /// <param name="dx">The step along the x-axis.</param>
        /// <param name="dy">The step along the y-axis.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Move(double dx, double dy)
            => Move(new(dx, dy));

        private void AppendLine(Vector2 delta)
        {
            bool horiz = delta.Y.IsZero();
            bool vert = delta.X.IsZero();
            if (horiz && vert)
                return;
            else if (horiz)
                Append($"{Action('h')}{Convert(delta.X)}");
            else if (vert)
                Append($"{Action('v')}{Convert(delta.Y)}");
            else
                Append($"{Action('l')}{Convert(delta)}");
        }

        /// <summary>
        /// Draws a line using absolute coordinates.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder LineTo(Vector2 location)
        {
            InitializePath();

            // Local coordinate space
            _p1 = _p2;
            _h1 = _p1;
            _p2 = location;
            _h2 = location;

            Vector2 delta = _p2 - _p1;
            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = delta / delta.Length;
            _n2 = _n1;

            // Global coordinate space
            delta = Transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <summary>
        /// Draws a line using absolute coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder LineTo(double x, double y)
            => LineTo(new(x, y));

        /// <summary>
        /// Draws a line using relative coordinates.
        /// </summary>
        /// <param name="delta">The step.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Line(Vector2 delta)
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
            delta = Transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <summary>
        /// Draws a line using relative coordinates.
        /// </summary>
        /// <param name="dx">The step along the x-axis.</param>
        /// <param name="dy">The step along the y-axis.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Line(double dx, double dy)
            => Line(new(dx, dy));

        /// <summary>
        /// Draws a horizontal line using absolute coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder HorizontalTo(double x)
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
            delta = Transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <summary>
        /// Draws a horizontal line using relative coordinates.
        /// </summary>
        /// <param name="dx">The step.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Horizontal(double dx)
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
            delta = Transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(_last);
            return this;
        }

        /// <summary>
        /// Draws a vertical line using absolute coordinates.
        /// </summary>
        /// <param name="y">The x-coordinate.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder VerticalTo(double y)
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
            delta = Transform.ApplyDirection(delta);
            AppendLine(delta);
            _last += delta;
            _bounds.Expand(delta);
            return this;
        }

        /// <summary>
        /// Draws a horizontal line using relative coordinates.
        /// </summary>
        /// <param name="dx">The step.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Vertical(double dy)
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
            delta = Transform.ApplyDirection(delta);
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

        /// <summary>
        /// Draws a bezier curve using absolute coordinates.
        /// </summary>
        /// <param name="h1">The first handle.</param>
        /// <param name="h2">The second handle.</param>
        /// <param name="end">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder CurveTo(Vector2 h1, Vector2 h2, Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = h1;
            _p2 = end;
            _h2 = h2;
            CalculateBezierNormals();

            // Draw in global coordinate space
            h1 = Transform.Apply(h1);
            h2 = Transform.Apply(h2);
            end = Transform.Apply(end);
            Append($"{Action('C')}{Convert(h1)} {Convert(h2)} {Convert(end)}");
            _last = end;
            _bounds.Expand(new[] { h1, h2, end });
            return this;
        }

        /// <summary>
        /// Draws a bezier curve using relative coordinates.
        /// </summary>
        /// <param name="dh1">The first handle.</param>
        /// <param name="dh2">The second handle.</param>
        /// <param name="dend">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Curve(Vector2 dh1, Vector2 dh2, Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = _p1 + dh1;
            _h2 = _p1 + dh2;
            _p2 = _p1 + dend;
            CalculateBezierNormals();

            // Draw in global coordinate space
            Vector2 h1 = Transform.Apply(_h1);
            Vector2 h2 = Transform.Apply(_h2);
            Vector2 end = Transform.Apply(_p2);
            Append($"{Action('c')}{Convert(h1 - _last)} {Convert(h2 - _last)} {Convert(end - _last)}");
            _last = end;
            _bounds.Expand(new[] { h1, h2, end });
            return this;
        }

        /// <summary>
        /// Draws a smooth bezier curve using absolute coordinates.
        /// </summary>
        /// <param name="h">The handle.</param>
        /// <param name="end">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder SmoothTo(Vector2 h, Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = end;
            _h2 = h;
            CalculateBezierNormals();

            // Draw in global coordinate space
            Vector2 h1 = Transform.Apply(_h1);
            Vector2 h2 = Transform.Apply(_h2);
            end = Transform.Apply(end);
            Append($"{Action('S')}{Convert(h2)} {Convert(end)}");
            _last = end;
            _bounds.Expand(new[] { h1, h2, end });
            return this;
        }

        /// <summary>
        /// Draws a smooth bezier curve using relative coordinates.
        /// </summary>
        /// <param name="dh">The handle.</param>
        /// <param name="dend">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Smooth(Vector2 dh, Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = _p1 + dend;
            _h2 = _p1 + dh;
            CalculateBezierNormals();

            // Draw in global coordinate space
            Vector2 h1 = Transform.Apply(_h1);
            Vector2 h2 = Transform.Apply(_h2);
            Vector2 end = Transform.Apply(_p2);
            Append($"{Action('s')}{Convert(h2 - _last)} {Convert(end - _last)}");
            _last = end;
            _bounds.Expand(new[] { h1, h2, end });
            return this;
        }

        /// <summary>
        /// Draws a quadrature bezier curve using absolute coordinates.
        /// </summary>
        /// <param name="h">The handle.</param>
        /// <param name="end">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder QuadCurveTo(Vector2 h, Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = h;
            _h2 = h;
            _p2 = end;
            CalculateBezierNormals();

            // Draw in global coordinate space
            h = Transform.Apply(h);
            end = Transform.Apply(end);
            Append($"{Action('Q')}{Convert(h)} {Convert(end)}");
            _last = end;
            _bounds.Expand(new[] { h, end });
            return this;
        }

        /// <summary>
        /// Draws a quadrature bezier curve using relative coordinates.
        /// </summary>
        /// <param name="dh">The handle.</param>
        /// <param name="dend">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder QuadCurve(Vector2 dh, Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = _p1 + dh;
            _p2 = _p1 + dend;
            _h2 = _h1;
            CalculateBezierNormals();

            // Draw in global coordinate space
            Vector2 h = Transform.Apply(_h1);
            Vector2 end = Transform.Apply(_p2);
            Append($"{Action('q')}{Convert(h)} {Convert(end)}");
            _last = end;
            _bounds.Expand(new[] { h, end });
            return this;
        }

        /// <summary>
        /// Draws a smooth quadrature bezier curve using absolute coordinates.
        /// </summary>
        /// <param name="end">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder SmoothQuadTo(Vector2 end)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = end;
            _h2 = _h1;
            CalculateBezierNormals();

            // Draw in global coordinate space
            Vector2 h = Transform.Apply(_h1);
            end = Transform.Apply(end);
            Append($"{Action('T')}{Convert(end)}");
            _last = end;
            _bounds.Expand(new[] { h, end });
            return this;
        }

        /// <summary>
        /// Draws a smooth quadrature bezier curve using relative coordinates.
        /// </summary>
        /// <param name="dend">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder SmoothQuad(Vector2 dend)
        {
            InitializePath();

            _p1 = _p2;
            _h1 = 2 * _p2 - _h2;
            _p2 = _p1 + dend;
            _h2 = _h1;
            CalculateBezierNormals();

            // Draw in global coordinate space
            Vector2 h = Transform.Apply(_h1);
            Vector2 end = Transform.Apply(_p2);
            Append($"{Action('t')}{Convert(end - _last)}");
            _last = end;
            _bounds.Expand(new[] { h, end });
            return this;
        }

        /// <summary>
        /// Draws an arc.
        /// </summary>
        /// <param name="rx">The radius in X-direction.</param>
        /// <param name="ry">The radius in Y-direction.</param>
        /// <param name="angle">The angle of the X-axis.</param>
        /// <param name="largeArc">The large-arc argument. If <c>true</c>, the arc that is greater than 180deg is chosen.</param>
        /// <param name="sweepFlag">The sweep direction. If <c>true</c>, the sweep is through increasing angles.</param>
        /// <param name="end">The end point of the arc.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder ArcTo(double rx, double ry, double angle, bool largeArc, bool sweepFlag, Vector2 end)
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

            Vector2 p1 = irot * (_p2 - end) * 0.5;
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
            var hl = 4.0 / 3.0 * Math.Tan(Math.Abs(da) * 0.25);
            double pid2 = Math.Sign(da) * Math.PI * 0.5;
            double currentA = theta1;
            Transform toOriginal = new(0.5 * (_p2 + end), rot);
            Vector2 lastPoint = cp + Vector2.Normal(theta1).Scale(rx, ry);
            Vector2 lastTangent = Vector2.Normal(theta1 + pid2).Scale(rx, ry) * hl;
            for (int i = 1; i <= segments; i++)
            {
                currentA += da;
                Vector2 nextPoint = cp + Vector2.Normal(currentA).Scale(rx, ry);
                Vector2 nextTangent = Vector2.Normal(currentA + pid2).Scale(rx, ry) * hl;
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

        /// <summary>
        /// Draws an arc.
        /// </summary>
        /// <param name="rx">The radius in X-direction.</param>
        /// <param name="ry">The radius in Y-direction.</param>
        /// <param name="angle">The angle of the X-axis.</param>
        /// <param name="largeArc">The large-arc argument. If <c>true</c>, the arc that is greater than 180deg is chosen.</param>
        /// <param name="sweepFlag">The sweep direction. If <c>true</c>, the sweep is through increasing angles.</param>
        /// <param name="dend">The end point of the arc relative to the current point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Arc(double rx, double ry, double angle, bool largeArc, bool sweepFlag, Vector2 dend)
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

            Matrix2 rot = Matrix2.Rotate(angle);
            Matrix2 irot = rot.Transposed; // Possible because transformation is orthonormal

            Vector2 p1 = irot * -dend * 0.5;
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
            var hl = 4.0 / 3.0 * Math.Tan(Math.Abs(da) * 0.25);
            double pid2 = Math.Sign(da) * Math.PI * 0.5;
            double currentA = theta1;
            Transform toOriginal = new(_p2 + dend * 0.5, rot);
            Vector2 lastPoint = cp + Vector2.Normal(theta1).Scale(rx, ry);
            Vector2 lastTangent = Vector2.Normal(theta1 + pid2).Scale(rx, ry) * hl;
            for (int i = 1; i <= segments; i++)
            {
                currentA += da;
                Vector2 nextPoint = cp + Vector2.Normal(currentA).Scale(rx, ry);
                Vector2 nextTangent = Vector2.Normal(currentA + pid2).Scale(rx, ry) * hl;
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

        /// <summary>
        /// Closes the path.
        /// </summary>
        /// <returns>The path builder.</returns>
        public PathBuilder Close()
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
        /// Converts a double to a rounded value for our svg-document.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        private static string Convert(double value)
        {
            string result = Math.Round(value, 2).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            int length = result.Length - 1;
            while (result[length] == '0')
                length--;
            if (result[length] == '.')
                return result[..length];
            return result[..(length + 1)];
        }

        /// <summary>
        /// Converts a vector to a string for our svg document.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>The formatted value.</returns>
        private static string Convert(Vector2 v)
        => $"{Convert(v.X)} {Convert(v.Y)}";

        /// <summary>
        /// Gets the optional action.
        /// </summary>
        /// <param name="c">The action identifier.</param>
        /// <returns>The action result.</returns>
        private string Action(char c, char nextImplied = '\0')
        {
            var current = _impliedAction;
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
