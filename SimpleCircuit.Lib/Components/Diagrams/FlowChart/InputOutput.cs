using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    [Drawable("FIO", "A Flowchart Input/Output.", "Flowchart", "parallelogram")]
    public class InputOutput : DrawableFactory
    {
        private const double _edgeSkew = 0.35;

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : DiagramBlockInstance(name)
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint());
            private double _width, _height = 15.0;

            /// <summary>
            /// Variant for manual input.
            /// </summary>
            public const string Manual = "manual";

            /// <inheritdoc />
            public override string Type => "io";

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block. If 0, the width is calculated from the content. The default is 0.")]
            [Alias("w")]
            public double Width { get; set; }

            /// <summary>
            /// Gets or sets the minimum width.
            /// </summary>
            [Description("The minimum width of the block. Only used when determining the width from contents.")]
            public double MinWidth { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block. If 0, the height is calculated from the content. The default is 0.")]
            [Alias("h")]
            public double Height { get; set; }

            /// <summary>
            /// Gets or sets the minimum height.
            /// </summary>
            [Description("The minimum height of the block. Only used when determining the height from contents.")]
            public double MinHeight { get; set; } = 10.0;

            [Description("The radius of the sharp corners.")]
            [Alias("rs")]
            public double CornerRadiusSharp { get; set; }

            [Description("The radius of the blunt corners.")]
            [Alias("rb")]
            public double CornerRadiusBlunt { get; set; }

            [Description("Shorthand for setting the radius of both sharp and blunt corners.")]
            [Alias("r")]
            public double Radius
            {
                get => Math.Min(CornerRadiusSharp, CornerRadiusBlunt);
                set
                {
                    CornerRadiusBlunt = value;
                    CornerRadiusSharp = value;
                }
            }

            /// <summary>
            /// Gets or sets the margin for the content when sizing.
            /// </summary>
            [Description("The margin used when sizing the block using the contents. The default is 1 on all sides.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                // When determining the size, let's update the size based on the label bounds
                switch (context.Mode)
                {
                    case PreparationMode.Sizes:
                        if (Width.IsZero() || Height.IsZero())
                        {
                            // Figure out the bounds of the contents
                            var bounds = new ExpandableBounds();
                            foreach (var label in Labels)
                                bounds.Expand(label.Formatted.Bounds.Bounds);
                            var b = bounds.Bounds.Expand(Margin);

                            if (Variants.Contains(Manual))
                            {
                                _width = Width.IsZero() ? Math.Max(MinWidth, b.Width) : Width;
                                _height = Height.IsZero() ? Math.Max(MinHeight, b.Height * 1.25) : Height;
                            }
                            else
                            {
                                _height = Height.IsZero() ? Math.Max(MinHeight, b.Height) : Height;
                                _width = Width.IsZero() ? Math.Max(MinWidth, b.Width + _height * _edgeSkew * 2) : Width;
                            }
                        }
                        else
                        {
                            _width = Width;
                            _height = Height;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.Modify(Style);

                if (Variants.Contains(Manual))
                    Trapezoid(builder, style);
                else
                {
                    var edge = new Vector2(_height * _edgeSkew, -_height);
                    builder.Parallellogram(0.0, 0.0, _width, edge, style, CornerRadiusSharp, CornerRadiusBlunt);
                    _anchors[0] = new LabelAnchorPoint(new(), new());
                }
                _anchors.Draw(builder, this, style);
            }

            private void Trapezoid(IGraphicsBuilder drawing, IStyle style)
            {
                var pcorner = new Vector2(-_width * 0.5, -_height * 0.5);
                var edge = new Vector2(_width, _height * 0.25);
                CommonGraphical.RoundedCorner(pcorner, new(0, _height * 0.5), edge * 0.5, CornerRadiusSharp,
                    out var ps1, out var ps2, out bool cornerSharp);
                CommonGraphical.RoundedCorner(pcorner + edge, -edge * 0.5, new Vector2(0, _height - edge.Y) * 0.5, CornerRadiusBlunt,
                    out var pb1, out var pb2, out bool cornerBlunt);
                double straightRadius = Math.Min(CornerRadiusBlunt, CornerRadiusSharp);
                CommonGraphical.RoundedCorner(new(_width * 0.5, _height * 0.5), new(0, -(_height - edge.Y) * 0.5), new(-_width * 0.5, 0), straightRadius,
                    out var pc1, out var pc2, out bool cornerStraightRight);
                CommonGraphical.RoundedCorner(new(-_width * 0.5, _height * 0.5), new(_width * 0.5, 0), new(0, -_height * 0.5), straightRadius,
                    out var pd1, out var pd2, out bool cornerStraightLeft);

                // Draw
                drawing.Path(b =>
                {
                    b.MoveTo(ps1);
                    if (cornerSharp)
                        b.ArcTo(CornerRadiusSharp, CornerRadiusSharp, 0.0, false, true, ps2);
                    b.LineTo(pb1);
                    if (cornerBlunt)
                        b.ArcTo(CornerRadiusBlunt, CornerRadiusBlunt, 0.0, false, true, pb2);
                    b.LineTo(pc1);
                    if (cornerStraightRight)
                        b.ArcTo(straightRadius, straightRadius, 0.0, false, true, pc2);
                    b.LineTo(pd1);
                    if (cornerStraightLeft)
                        b.ArcTo(straightRadius, straightRadius, 0.0, false, true, pd2);
                    b.Close();
                }, style);
                _anchors[0] = new LabelAnchorPoint(new(0, edge.Y / 2), new());
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                static Vector2 Interp(Vector2 a, Vector2 b, double ka)
                {
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }

                if (Variants.Contains(Manual))
                {
                    // Calculate the points for the trapezoid
                    var pcorner = new Vector2(-_width * 0.5, -_height * 0.5);
                    var edge = new Vector2(_width, _height * 0.25);
                    double straightRadius = Math.Min(CornerRadiusSharp, CornerRadiusBlunt);
                    foreach (var pin in pins)
                    {
                        if (pin.Orientation.X > 0.707 && pin.Orientation.Y > 0.707)
                        {
                            // Bottom right straight corner
                            CommonGraphical.InterpRoundedCorner(new(_width * 0.5, _height * 0.5), new(0, -(_height - edge.Y) * 0.5), new(-_width * 0.5, 0), straightRadius, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X > 0.707 && pin.Orientation.Y < -0.707)
                        {
                            // Top right blunt corner
                            CommonGraphical.InterpRoundedCorner(pcorner + edge, -edge * 0.5, new Vector2(0, _height - edge.Y) * 0.5, CornerRadiusBlunt, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X < -0.707 && pin.Orientation.Y > 0.707)
                        {
                            // Bottom left straight corner
                            CommonGraphical.InterpRoundedCorner(new(-_width * 0.5, _height * 0.5), new(_width * 0.5, 0), new(0, -_height * 0.5), straightRadius, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X < -0.707 && pin.Orientation.Y < -0.707)
                        {
                            // Top left sharp corner
                            CommonGraphical.InterpRoundedCorner(pcorner, new(0, _height * 0.5), edge * 0.5, CornerRadiusSharp, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else
                        {
                            double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                            CommonGraphical.RoundedCorner(pcorner, new(0, _height * 0.5), edge * 0.5, CornerRadiusSharp,
                                out var ps1, out var ps2, out bool cornerSharp);
                            CommonGraphical.RoundedCorner(pcorner + edge, -edge * 0.5, new Vector2(0, _height - edge.Y) * 0.5, CornerRadiusBlunt,
                                out var pb1, out var pb2, out bool cornerBlunt);
                            CommonGraphical.RoundedCorner(new(_width * 0.5, _height * 0.5), new(0, -(_height - edge.Y) * 0.5), new(-_width * 0.5, 0), straightRadius,
                                out var pc1, out var pc2, out bool cornerStraightRight);
                            CommonGraphical.RoundedCorner(new(-_width * 0.5, _height * 0.5), new(_width * 0.5, 0), new(0, -_height * 0.5), straightRadius,
                                out var pd1, out var pd2, out bool cornerStraightLeft);
                            if (angle < -Math.PI * 0.75)
                                pin.Offset = Interp(new(-_width * 0.5, edge.Y * 0.5), pb1, 2 * (angle + Math.PI));
                            else if (angle < -Math.PI * 0.25)
                                pin.Offset = Interp(ps2, pb1, angle + Math.PI * 0.75);
                            else if (angle < 0.0)
                                pin.Offset = Interp(pb2, new(_width * 0.5, edge.Y * 0.5), 2 * (angle + Math.PI * 0.25));
                            else if (angle < Math.PI * 0.25)
                                pin.Offset = Interp(new(_width * 0.5, edge.Y * 0.5), pc1, 2 * angle);
                            else if (angle < Math.PI * 0.75)
                                pin.Offset = Interp(pc2, pd1, angle - Math.PI * 0.25);
                            else
                                pin.Offset = Interp(ps2, new(-_width * 0.5, edge.Y * 0.5), 2 * (angle - Math.PI * 0.75));
                        }
                    }
                }
                else
                {
                    // Calculate the points for the parallellogram
                    var edge = new Vector2(_height * _edgeSkew, -_height);
                    var pcorner = new Vector2(-_width * 0.5, -edge.Y * 0.5);
                    var horiz = new Vector2((_width - edge.X) * 0.5, 0);
                    foreach (var pin in pins)
                    {
                        if (pin.Orientation.X > 0.707 && pin.Orientation.Y > 0.707)
                        {
                            // Bottom right blunt corner
                            CommonGraphical.InterpRoundedCorner(-pcorner - edge, edge * 0.5, -horiz, CornerRadiusBlunt, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X > 0.707 && pin.Orientation.Y < -0.707)
                        {
                            // Top right sharp corner
                            CommonGraphical.InterpRoundedCorner(-pcorner, -horiz, -edge * 0.5, CornerRadiusSharp, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X < -0.707 && pin.Orientation.Y > 0.707)
                        {
                            // Bottom left sharp corner
                            CommonGraphical.InterpRoundedCorner(pcorner, horiz, edge * 0.5, CornerRadiusSharp, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X < -0.707 && pin.Orientation.Y < -0.707)
                        {
                            // Top left blunt corner
                            CommonGraphical.InterpRoundedCorner(pcorner + edge, -edge * 0.5, horiz, CornerRadiusBlunt, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else
                        {
                            double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                            CommonGraphical.RoundedCorner(pcorner, horiz, edge * 0.5, CornerRadiusSharp,
                                out var ps1, out var ps2, out bool cornerSharp);
                            CommonGraphical.RoundedCorner(pcorner + edge, -edge * 0.5, horiz, CornerRadiusBlunt,
                                out var pb1, out var pb2, out bool cornerBlunt);
                            if (angle < -Math.PI * 0.75)
                                pin.Offset = Interp(ps2, pb1, angle + Math.PI * 1.25);
                            else if (angle < -Math.PI * 0.5)
                                pin.Offset = Interp(pb2, new(0, -_height * 0.5), 2 * (angle + Math.PI * 0.75));
                            else if (angle < -Math.PI * 0.25)
                                pin.Offset = Interp(new(0, -_height * 0.5), -ps1, 2 * (angle + Math.PI * 0.5));
                            else if (angle < Math.PI * 0.25)
                                pin.Offset = Interp(-ps2, -pb1, angle + Math.PI * 0.25);
                            else if (angle < Math.PI * 0.5)
                                pin.Offset = Interp(-pb2, new(0, _height * 0.5), 2 * (angle - Math.PI * 0.25));
                            else if (angle < Math.PI * 0.75)
                                pin.Offset = Interp(new(0, _height * 0.5), ps1, 2 * (angle - Math.PI * 0.5));
                            else
                                pin.Offset = Interp(ps2, pb1, angle - Math.PI * 0.75);
                        }
                    }
                }
            }
        }
    }
}
