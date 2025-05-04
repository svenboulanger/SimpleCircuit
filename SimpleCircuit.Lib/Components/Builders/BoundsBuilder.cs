using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components.Appearance;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Builders
{
    /// <summary>
    /// An <see cref="IGraphicsBuilder"/> for finding bounds.
    /// </summary>
    public class BoundsBuilder : BaseGraphicsBuilder
    {
        private readonly Stack<Transform> _tf = new();
        private readonly ITextFormatter _formatter;

        /// <summary>
        /// Creates a new <see cref="BoundsBuilder"/>.
        /// </summary>
        /// <param name="formatter">The text formatter.</param>
        /// <param name="diagnostics">The diagnostiscs handler.</param>
        public BoundsBuilder(ITextFormatter formatter, IDiagnosticHandler diagnostics = null)
            : base(diagnostics)
        {
            _tf.Push(Transform.Identity);
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        /// <inheritdoc />
        public override IGraphicsBuilder BeginGroup(string id = null, IEnumerable<string> classes = null, bool atStart = false) => this;

        /// <inheritdoc />
        public override IGraphicsBuilder Circle(Vector2 center, double radius, IAppearanceOptions options)
        {
            radius = CurrentTransform.ApplyDirection(new(radius, 0)).Length;
            center = CurrentTransform.Apply(center);
            double m = options.LineThickness * 0.5;
            Expand(
                center - new Vector2(radius + m, radius + m),
                center + new Vector2(radius + m, radius + m));
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, IAppearanceOptions options)
        {
            double kx = rx * 0.552284749831;
            double ky = ry * 0.552284749831;
            BeginTransform(new Transform(center, Matrix2.Identity));
            Path(b => b.MoveTo(new(-rx, 0))
                .CurveTo(new(-rx, -ky), new(-kx, -ry), new(0, -ry))
                .CurveTo(new(kx, -ry), new(rx, -ky), new(rx, 0))
                .CurveTo(new(rx, ky), new(kx, ry), new(0, ry))
                .CurveTo(new(-kx, ry), new(-rx, ky), new(-rx, 0)).Close(),
                options);
            EndTransform();
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder EndGroup() => this;

        /// <inheritdoc />
        public override IGraphicsBuilder Line(Vector2 start, Vector2 end, IAppearanceOptions options = null)
        {
            start = CurrentTransform.Apply(start);
            end = CurrentTransform.Apply(end);
            Expand(start, end);
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, IAppearanceOptions options)
        {
            if (pathBuild is null)
                return this;
            var bounds = new ExpandableBounds();
            var builder = new BoundsPathBuilder(CurrentTransform, bounds);
            pathBuild(builder);
            Expand(bounds.Bounds);
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Polygon(IEnumerable<Vector2> points, IAppearanceOptions options)
        {
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                Expand(tpt);
            }
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Polyline(IEnumerable<Vector2> points, IAppearanceOptions options = null)
        {
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                Expand(tpt);
            }
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Text(Span span, Vector2 location, Vector2 expand)
        {
            location = CurrentTransform.Apply(location);
            expand = CurrentTransform.ApplyDirection(expand);

            // Compute the location based on the location and expansion
            var bounds = span.Bounds.Bounds;
            double y = location.Y, x = location.X;
            if (expand.Y.IsZero())
                y = y - bounds.Height * 0.5 - bounds.Top;
            else if (expand.Y < 0)
                y -= bounds.Bottom;
            else
                y -= bounds.Top;
            if (expand.X.IsZero())
                x = x - bounds.Width * 0.5 - bounds.Left;
            else if (expand.X < 0)
                x -= bounds.Right;
            else
                x -= bounds.Left;

            // Return the offset bounds
            Expand(bounds + new Vector2(x, y));
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Text(string value, Vector2 location, Vector2 expand, IAppearanceOptions appearance)
        {
            if (string.IsNullOrWhiteSpace(value))
                return this;

            var span = _formatter.Format(value, appearance);
            return Text(span, location, expand);
        }
    }
}
