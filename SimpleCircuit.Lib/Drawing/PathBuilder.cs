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
        private readonly Transform _transform;
        private Vector2 _current, _lastHandle;
        private Func<Vector2, Vector2> _relativeModifier = null, _absoluteModifier = null;

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
            _lastHandle = _current = _transform.Apply(location);

            _bounds.Expand(_current);
            Append($"M{Convert(_current)}");
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
            delta = _relativeModifier?.Invoke(delta) ?? delta;
            delta = _transform.ApplyDirection(delta);
            _current += delta;
            _lastHandle = _current;

            _bounds.Expand(_current);
            Append($"m{Convert(delta)}");
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

        /// <summary>
        /// Draws a line using absolute coordinates.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>The path builder.</returns>
        public PathBuilder LineTo(Vector2 location)
        {
            location = _absoluteModifier?.Invoke(location) ?? location;
            _lastHandle = _current = _transform.Apply(location);

            _bounds.Expand(_current);
            Append($"L{Convert(_current)}");
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
            _current += delta;
            _lastHandle = _current;

            _bounds.Expand(_current);
            Append($"l{Convert(delta)}");
            return this;
        }

        /// <summary>
        /// Draws a line using relative coordinates.
        /// </summary>
        /// <param name="dx">The step along the x-axis.</param>
        /// <param name="dy">The step along the y-axis.</param>
        /// <returns></returns>
        public PathBuilder Line(double dx, double dy)
            => Line(new(dx, dy));

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
            _lastHandle = _transform.Apply(h2);
            _current = _transform.Apply(end);

            _bounds.Expand(new[] { h1, _lastHandle, _current });
            Append($"C{Convert(h1)} {Convert(h2)} {Convert(end)}");
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
            _lastHandle = _current + dh2;
            _current += dend;

            _bounds.Expand(new[] { _current + dh1, _lastHandle, _current });
            Append($"c{Convert(dh1)} {Convert(dh2)} {Convert(dend)}");
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
            var h1 = 2 * _current - _lastHandle;
            _lastHandle = _transform.Apply(h);
            _current = _transform.Apply(end);

            _bounds.Expand(new[] { h1, _lastHandle, _current });
            Append($"S{Convert(h.X)} {Convert(h.Y)} {Convert(end.X)} {Convert(end.Y)}");
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
            var h1 = 2 * _current - _lastHandle;
            dh = _transform.ApplyDirection(dh);
            dend = _transform.ApplyDirection(dend);
            _lastHandle = _current + dh;
            _current += dend;

            _bounds.Expand(new[] { h1, _lastHandle, _current });
            Append($"s{Convert(dh.X)} {Convert(dh.Y)} {Convert(dend.X)} {Convert(dend.Y)}");
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
            _lastHandle = _transform.Apply(h);
            _current = _transform.Apply(end);

            _bounds.Expand(new[] { _lastHandle, _current });
            Append($"Q{Convert(_lastHandle)} {Convert(_current)}");
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
            _lastHandle = _current + dh;
            _current += dend;

            _bounds.Expand(new[] { _lastHandle, _current });
            Append($"q{Convert(_lastHandle)} {Convert(_current)}");
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
            _lastHandle = 2 * _current - _lastHandle;
            _current = _transform.Apply(end);

            _bounds.Expand(new[] { _lastHandle, _current });
            Append($"T{Convert(_current)}");
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
            _lastHandle = 2 * _current - _lastHandle;
            dend = _transform.ApplyDirection(dend);
            _current += dend;

            _bounds.Expand(new[] { _lastHandle, _current });
            Append($"t{Convert(dend)}");
            return this;
        }

        /// <summary>
        /// Closes the path.
        /// </summary>
        /// <returns>The path builder.</returns>
        public PathBuilder Close()
        {
            Append("Z");
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
        /// Converts the path builder to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
            => _sb.ToString();
    }
}
