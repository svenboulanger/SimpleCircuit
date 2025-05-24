using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components.Styles;
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

        /// <summary>
        /// Creates a new <see cref="BoundsBuilder"/>.
        /// </summary>
        /// <param name="formatter">The text formatter.</param>
        /// <param name="style">The style.</param>
        /// <param name="diagnostics">The diagnostiscs handler.</param>
        public BoundsBuilder(ITextFormatter formatter, IStyle style, IDiagnosticHandler diagnostics)
            : base(formatter, style, diagnostics)
        {
            _tf.Push(Transform.Identity);
        }

        /// <inheritdoc />
        public override IGraphicsBuilder BeginGroup(string id = null, IEnumerable<string> classes = null, bool atStart = false) => this;

        /// <inheritdoc />
        public override IGraphicsBuilder Circle(Vector2 center, double radius, IStyle options)
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
        public override IGraphicsBuilder Ellipse(Vector2 center, double rx, double ry, IStyle options)
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
        public override IGraphicsBuilder Line(Vector2 start, Vector2 end, IStyle options = null)
        {
            start = CurrentTransform.Apply(start);
            end = CurrentTransform.Apply(end);
            Expand(start, end);
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Path(Action<IPathBuilder> pathBuild, IStyle options)
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
        public override IGraphicsBuilder Polygon(IEnumerable<Vector2> points, IStyle options)
        {
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                Expand(tpt);
            }
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Polyline(IEnumerable<Vector2> points, IStyle options = null)
        {
            foreach (var pt in points)
            {
                var tpt = CurrentTransform.Apply(pt);
                Expand(tpt);
            }
            return this;
        }

        /// <inheritdoc />
        public override IGraphicsBuilder Text(Span span, Vector2 location, TextOrientation orientation)
        {
            if (span is null)
                return this;

            location = CurrentTransform.Apply(location);
            var bounds = span.Bounds.Bounds;
            switch (orientation.Type)
            {
                case TextOrientationTypes.Normal:
                    {
                        Expand(location + bounds);
                    }
                    break;

                case TextOrientationTypes.VertialUp:
                    {
                    }
                    break;

                case TextOrientationTypes.VerticalDown:
                    {
                    }
                    break;

                case TextOrientationTypes.Transformed:
                    {
                        var expand = CurrentTransform.ApplyDirection(orientation.Orientation);
                        Expand(expand * bounds.Right + expand.Perpendicular * bounds.Top);
                        Expand(expand * bounds.Left + expand.Perpendicular * bounds.Top);
                        Expand(expand * bounds.Right + expand.Perpendicular * bounds.Bottom);
                        Expand(expand * bounds.Left + expand.Perpendicular * bounds.Bottom);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
            
            return this;
        }
    }
}
