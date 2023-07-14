using System;
using System.Text;
using System.Transactions;

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
        public Transform Transform { get; set; } = Transform.Identity;

        /// <summary>
        /// Creates a new path builder.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="transform">The transform.</param>
        public PathBuilder(Transform transform)
        {
            _bounds = new();
            Transform = transform;
        }

        /// <summary>
        /// Adds another transform on top of the current one.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder WithTransform(Transform transform)
        {
            Transform = Transform.Apply(transform);
            return this;
        }

        /// <summary>
        /// Moves the current point using absolute coordinates.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder MoveTo(Vector2 location)
        {
            // Local coordinate space
            _p1 = _p2;
            _h1 = _p1;
            _p2 = location;
            _h2 = location;
            _n1 = new();
            _n2 = new();

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
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p1;

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
            _last = location;
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
            Vector2 delta = new(x - _p2.X, 0);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

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
        /// Draws a horizontal line using relative coordinates.
        /// </summary>
        /// <param name="dx">The step.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder Horizontal(double dx)
        {
            Vector2 delta = new(dx, 0);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = delta / delta.Length;
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
            Vector2 delta = new(0, y - _p2.Y);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (delta.IsZero())
                _n1 = new();
            else
                _n1 = delta / delta.Length;
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
            Vector2 delta = new(0, dy);
            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            if (_n1.IsZero())
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
            return Math.Round(value, 5).ToString("G4", System.Globalization.CultureInfo.InvariantCulture);
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
