using System;
using System.Diagnostics.SymbolStore;
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
        private readonly Transform _transform;
        private Vector2 _p1, _n1, _h1, _p2, _n2, _h2;
        private Func<Vector2, Vector2> _relativeModifier = null, _absoluteModifier = null;
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
        /// Creates a new path builder.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="transform">The transform.</param>
        public PathBuilder(ExpandableBounds bounds, Transform transform)
        {
            _bounds = bounds;
            _transform = transform;
        }

        /// <summary>
        /// Applies a modifier to absolute coordinates.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder WithAbsoluteModifier(Func<Vector2, Vector2> modifier)
        {
            _absoluteModifier = modifier;
            return this;
        }

        /// <summary>
        /// Applies a modifier to relative coordinates.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder WithRelativeModifier(Func<Vector2, Vector2> modifier)
        {
            _relativeModifier = modifier;
            return this;
        }

        /// <summary>
        /// Applies a modifier to relative and absolute coordinates.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder WithModifier(Func<Vector2, Vector2> modifier)
        {
            _relativeModifier = modifier;
            _absoluteModifier = modifier;
            return this;
        }

        /// <summary>
        /// Moves the current point using absolute coordinates.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder MoveTo(Vector2 location)
        {
            location = _absoluteModifier?.Invoke(location) ?? location;
            location = _transform.Apply(location);

            // Store the current segment information
            _p1 = _p2;
            _h1 = _p1;
            _p2 = location;
            _h2 = location;
            _n1 = new();
            _n2 = new();

            _bounds.Expand(location);
            Append($"{Action('M', 'L')}{Convert(location)}");
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
            if (_isFirst)
            {
                delta = _absoluteModifier?.Invoke(delta) ?? delta;
                delta = _transform.Apply(delta);
            }
            else
            {
                delta = _relativeModifier?.Invoke(delta) ?? delta;
                delta = _transform.ApplyDirection(delta);
            }

            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p1;

            _n1 = new();
            _n2 = new();

            _bounds.Expand(_p2);
            Append($"{Action('m', 'l')}{Convert(delta)}");
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
            location = _absoluteModifier?.Invoke(location) ?? location;
            location = _transform.Apply(location);

            _p1 = _p2;
            _h1 = _p1;
            _p2 = location;
            _h2 = location;

            _n1 = _p2 - _p1;
            _n1 /= _n1.Length;
            _n2 = _n1;

            AppendLine(location - _p1);
            _bounds.Expand(location);
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
            delta = _relativeModifier?.Invoke(delta) ?? delta;
            delta = _transform.ApplyDirection(delta);

            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p1;

            _n1 = delta / delta.Length;
            _n2 = _n1;

            AppendLine(delta);
            _bounds.Expand(_p2);
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
            Vector2 vo = _absoluteModifier?.Invoke(new()) ?? new();
            vo = _transform.Apply(vo);
            Vector2 vnx;
            if (x.IsZero())
            {
                vnx = _relativeModifier?.Invoke(new(1, 0)) ?? new(1, 0);
                vnx = _transform.ApplyDirection(vnx);
                vnx /= vnx.Length;
            }
            else
            {
                Vector2 vx = _absoluteModifier?.Invoke(new(x, 0)) ?? new(x, 0);
                vx = _transform.Apply(vx);
                vnx = vx - vo;
                x = vnx.Length;
                vnx /= x;
            }

            double k = x - (_p2 - vo).Dot(vnx);
            var delta = k * vnx;

            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            _n1 = delta / delta.Length;
            _n2 = _n1;

            _bounds.Expand(_p2);
            AppendLine(delta);
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
            delta = _relativeModifier?.Invoke(delta) ?? delta;
            delta = _transform.ApplyDirection(delta);

            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            _n1 = delta / delta.Length;
            _n2 = delta;

            _bounds.Expand(_p2);
            AppendLine(delta);
            return this;
        }

        /// <summary>
        /// Draws a vertical line using absolute coordinates.
        /// </summary>
        /// <param name="y">The x-coordinate.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder VerticalTo(double y)
        {
            Vector2 vo = _absoluteModifier?.Invoke(new()) ?? new();
            vo = _transform.Apply(vo);
            Vector2 vny;
            if (y.IsZero())
            {
                vny = _relativeModifier?.Invoke(new(0, 1)) ?? new(0, 1);
                vny = _transform.ApplyDirection(vny);
                vny /= vny.Length;
            }
            else
            {
                Vector2 vy = _absoluteModifier?.Invoke(new(0, y)) ?? new(0, y);
                vy = _transform.Apply(vy);
                vny = vy - vo;
                y = vny.Length;
                vny /= y;
            }

            double k = y - (_p2 - vo).Dot(vny);
            var delta = k * vny;

            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            _n1 = delta / delta.Length;
            _n2 = _n1;

            _bounds.Expand(_p2);
            AppendLine(delta);
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
            delta = _relativeModifier?.Invoke(delta) ?? delta;
            delta = _transform.ApplyDirection(delta);

            _p1 = _p2;
            _h1 = _p1;
            _p2 += delta;
            _h2 = _p2;

            _n1 = delta / delta.Length;
            _n2 = _n1;

            _bounds.Expand(_p2);
            AppendLine(delta);
            return this;
        }

        private void CalculateBezierNormals()
        {
            _n1 = _h1 - _p1;
            if (_n1.X.IsZero() && _n1.Y.IsZero())
                _n1 = _h2 - _p1;
            _n1 /= _n1.Length;
            _n2 = _p2 - _h1;
            if (_n2.X.IsZero() && _n2.Y.IsZero())
                _n2 = _p2 - _h1;
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
            h1 = _absoluteModifier?.Invoke(h1) ?? h1;
            h2 = _absoluteModifier?.Invoke(h2) ?? h2;
            end = _absoluteModifier?.Invoke(end) ?? end;
            h1 = _transform.Apply(h1);
            h2 = _transform.Apply(h2);
            end = _transform.Apply(end);

            _p1 = _p2;
            _h1 = h1;
            _p2 = end;
            _h2 = h2;
            CalculateBezierNormals();

            _bounds.Expand(new[] { h1, h2, end });
            Append($"{Action('C')}{Convert(h1)} {Convert(h2)} {Convert(end)}");
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
            dh1 = _relativeModifier?.Invoke(dh1) ?? dh1;
            dh2 = _relativeModifier?.Invoke(dh2) ?? dh2;
            dend = _relativeModifier?.Invoke(dend) ?? dend;
            dh1 = _transform.ApplyDirection(dh1);
            dh2 = _transform.ApplyDirection(dh2);
            dend = _transform.ApplyDirection(dend);

            _p1 = _p2;
            _h1 = _p1 + dh1;
            _h2 = _p1 + dh2;
            _p2 = _p1 + dend;
            CalculateBezierNormals();

            _bounds.Expand(new[] { _h1, _h2, _p2 });
            Append($"{Action('c')}{Convert(dh1)} {Convert(dh2)} {Convert(dend)}");
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
            h = _absoluteModifier?.Invoke(h) ?? h;
            end = _absoluteModifier?.Invoke(end) ?? end;
            h = _transform.Apply(h);
            end = _transform.Apply(end);
            var h1 = 2 * _p2 - _h2;

            _p1 = _p2;
            _h1 = h1;
            _p2 = end;
            _h2 = h;
            CalculateBezierNormals();

            _bounds.Expand(new[] { _h1, _h2, _p2 });
            Append($"{Action('S')}{Convert(_h2)} {Convert(_p2)}");
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
            dh = _relativeModifier?.Invoke(dh) ?? dh;
            dend = _relativeModifier?.Invoke(dend) ?? dend;
            dh = _transform.ApplyDirection(dh);
            dend = _transform.ApplyDirection(dend);
            var h1 = 2 * _p2 - _h2;

            _p1 = _p2;
            _h1 = h1;
            _p2 = _p1 + dend;
            _h2 = _p1 + dh;
            CalculateBezierNormals();

            _bounds.Expand(new[] { _h1, _h2, _p2});
            Append($"{Action('s')}{Convert(dh)} {Convert(dend)}");
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
            h = _absoluteModifier?.Invoke(h) ?? h;
            end = _absoluteModifier?.Invoke(end) ?? end;
            h = _transform.Apply(h);
            end = _transform.Apply(end);

            _p1 = _p2;
            _h1 = h;
            _h2 = h;
            _p2 = end;
            CalculateBezierNormals();

            _bounds.Expand(new[] { _h2, _p2 });
            Append($"{Action('Q')}{Convert(_h2)} {Convert(_p2)}");
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
            dh = _relativeModifier?.Invoke(dh) ?? dh;
            dend = _relativeModifier?.Invoke(dend) ?? dend;
            dh = _transform.ApplyDirection(dh);
            dend = _transform.ApplyDirection(dend);

            _p1 = _p2;
            _h1 = _p1 + dh;
            _p2 = _p1 + dend;
            _h2 = _h1;
            CalculateBezierNormals();

            _bounds.Expand(new[] { _h1, _p2 });
            Append($"{Action('q')}{Convert(dh)} {Convert(dend)}");
            return this;
        }

        /// <summary>
        /// Draws a smooth quadrature bezier curve using absolute coordinates.
        /// </summary>
        /// <param name="end">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder SmoothQuadTo(Vector2 end)
        {
            end = _absoluteModifier?.Invoke(end) ?? end;
            var h = 2 * _p2 - _h2;
            end = _transform.Apply(end);

            _p1 = _p2;
            _h1 = h;
            _p2 = end;
            _h2 = h;
            CalculateBezierNormals();

            _bounds.Expand(new[] { h, end });
            Append($"{Action('T')}{Convert(end)}");
            return this;
        }

        /// <summary>
        /// Draws a smooth quadrature bezier curve using relative coordinates.
        /// </summary>
        /// <param name="dend">The end point.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder SmoothQuad(Vector2 dend)
        {
            dend = _relativeModifier?.Invoke(dend) ?? dend;
            var h = 2 * _p2 - _h2;
            dend = _transform.ApplyDirection(dend);

            _p1 = _p2;
            _h1 = h;
            _p2 = _p1 + dend;
            _h2 = h;

            _bounds.Expand(new[] { _h2, _p2 });
            Append($"{Action('t')}{Convert(dend)}");
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
        /// Calculates the average orientation given the new orientation.
        /// </summary>
        /// <param name="a">The first argument.</param>
        /// <param name="b">The second argument.</param>
        /// <returns>The average orientation.</returns>
        public static Vector2 Bisector(Vector2 a, Vector2 b)
        {
            var newOrientation = a * b.Length + b * a.Length;
            if (newOrientation.X.IsZero() && newOrientation.Y.IsZero())
                return new Vector2(a.Y, -a.X) / a.Length;
            return newOrientation / newOrientation.Length;
        }

        /// <summary>
        /// Converts the path builder to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
            => _sb.ToString();
    }
}
