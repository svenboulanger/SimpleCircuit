using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
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
        private class Instance(string name) : DiagramBlockInstance(name), ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint());
            private double _width = 30.0, _height = 15.0;

            /// <summary>
            /// Variant for manual input.
            /// </summary>
            public const string Manual = "manual";

            /// <inheritdoc />
            public override string Type => "io";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            [Description("The width of the block.")]
            [Alias("w")]
            public double Width
            {
                get => _width;
                set
                {
                    if (value < _height * 0.5)
                        _width = _height * 0.5;
                    else
                        _width = value;
                }
            }

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height
            {
                get => _height;
                set
                {
                    if (value > _width * 2)
                        _height = _width * 2;
                    else
                        _height = value;
                }
            }

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

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                // drawing.Parallellogram(0, 0, Width, new(Height * 0.5, -Height), CornerRadius, CornerRadius);
                if (Variants.Contains(Manual))
                    Trapezoid(drawing);
                else
                {
                    var edge = new Vector2(Height * _edgeSkew, -Height);
                    drawing.Parallellogram(0.0, 0.0, Width, edge, CornerRadiusSharp, CornerRadiusBlunt);
                    _anchors[0] = new LabelAnchorPoint(new(), new());
                }
                _anchors.Draw(drawing, this);
            }

            private void Trapezoid(SvgDrawing drawing)
            {
                var pcorner = new Vector2(-Width * 0.5, -Height * 0.5);
                var edge = new Vector2(Width, Height * 0.25);
                CommonGraphical.RoundedCorner(pcorner, new(0, Height * 0.5), edge * 0.5, CornerRadiusSharp,
                    out var ps1, out var ps2, out bool cornerSharp);
                CommonGraphical.RoundedCorner(pcorner + edge, -edge * 0.5, new Vector2(0, Height - edge.Y) * 0.5, CornerRadiusBlunt,
                    out var pb1, out var pb2, out bool cornerBlunt);
                double straightRadius = Math.Min(CornerRadiusBlunt, CornerRadiusSharp);
                CommonGraphical.RoundedCorner(new(Width * 0.5, Height * 0.5), new(0, -(Height - edge.Y) * 0.5), new(-Width * 0.5, 0), straightRadius,
                    out var pc1, out var pc2, out bool cornerStraightRight);
                CommonGraphical.RoundedCorner(new(-Width * 0.5, Height * 0.5), new(Width * 0.5, 0), new(0, -Height * 0.5), straightRadius,
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
                });
                _anchors[0] = new LabelAnchorPoint(new(0, (Height - edge.Y) * 0.5), new());
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
                    var pcorner = new Vector2(-Width * 0.5, -Height * 0.5);
                    var edge = new Vector2(Width, Height * 0.25);
                    double straightRadius = Math.Min(CornerRadiusSharp, CornerRadiusBlunt);
                    foreach (var pin in pins)
                    {
                        if (pin.Orientation.X > 0.707 && pin.Orientation.Y > 0.707)
                        {
                            // Bottom right straight corner
                            CommonGraphical.InterpRoundedCorner(new(Width * 0.5, Height * 0.5), new(0, -(Height - edge.Y) * 0.5), new(-Width * 0.5, 0), straightRadius, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X > 0.707 && pin.Orientation.Y < -0.707)
                        {
                            // Top right blunt corner
                            CommonGraphical.InterpRoundedCorner(pcorner + edge, -edge * 0.5, new Vector2(0, Height - edge.Y) * 0.5, CornerRadiusBlunt, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X < -0.707 && pin.Orientation.Y > 0.707)
                        {
                            // Bottom left straight corner
                            CommonGraphical.InterpRoundedCorner(new(-Width * 0.5, Height * 0.5), new(Width * 0.5, 0), new(0, -Height * 0.5), straightRadius, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else if (pin.Orientation.X < -0.707 && pin.Orientation.Y < -0.707)
                        {
                            // Top left sharp corner
                            CommonGraphical.InterpRoundedCorner(pcorner, new(0, Height * 0.5), edge * 0.5, CornerRadiusSharp, 0.5, out var p, out _);
                            pin.Offset = p;
                        }
                        else
                        {
                            double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                            CommonGraphical.RoundedCorner(pcorner, new(0, Height * 0.5), edge * 0.5, CornerRadiusSharp,
                                out var ps1, out var ps2, out bool cornerSharp);
                            CommonGraphical.RoundedCorner(pcorner + edge, -edge * 0.5, new Vector2(0, Height - edge.Y) * 0.5, CornerRadiusBlunt,
                                out var pb1, out var pb2, out bool cornerBlunt);
                            CommonGraphical.RoundedCorner(new(Width * 0.5, Height * 0.5), new(0, -(Height - edge.Y) * 0.5), new(-Width * 0.5, 0), straightRadius,
                                out var pc1, out var pc2, out bool cornerStraightRight);
                            CommonGraphical.RoundedCorner(new(-Width * 0.5, Height * 0.5), new(Width * 0.5, 0), new(0, -Height * 0.5), straightRadius,
                                out var pd1, out var pd2, out bool cornerStraightLeft);
                            if (angle < -Math.PI * 0.75)
                                pin.Offset = Interp(new(-Width * 0.5, edge.Y * 0.5), pb1, 2 * (angle + Math.PI));
                            else if (angle < -Math.PI * 0.25)
                                pin.Offset = Interp(ps2, pb1, angle + Math.PI * 0.75);
                            else if (angle < 0.0)
                                pin.Offset = Interp(pb2, new(Width * 0.5, edge.Y * 0.5), 2 * (angle + Math.PI * 0.25));
                            else if (angle < Math.PI * 0.25)
                                pin.Offset = Interp(new(Width * 0.5, edge.Y * 0.5), pc1, 2 * angle);
                            else if (angle < Math.PI * 0.75)
                                pin.Offset = Interp(pc2, pd1, angle - Math.PI * 0.25);
                            else
                                pin.Offset = Interp(ps2, new(-Width * 0.5, edge.Y * 0.5), 2 * (angle - Math.PI * 0.75));
                        }
                    }
                }
                else
                {
                    // Calculate the points for the parallellogram
                    var edge = new Vector2(Height * _edgeSkew, -Height);
                    var pcorner = new Vector2(-Width * 0.5, -edge.Y * 0.5);
                    var horiz = new Vector2((Width - edge.X) * 0.5, 0);
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
                                pin.Offset = Interp(pb2, new(0, -Height * 0.5), 2 * (angle + Math.PI * 0.75));
                            else if (angle < -Math.PI * 0.25)
                                pin.Offset = Interp(new(0, -Height * 0.5), -ps1, 2 * (angle + Math.PI * 0.5));
                            else if (angle < Math.PI * 0.25)
                                pin.Offset = Interp(-ps2, -pb1, angle + Math.PI * 0.25);
                            else if (angle < Math.PI * 0.5)
                                pin.Offset = Interp(-pb2, new(0, Height * 0.5), 2 * (angle - Math.PI * 0.25));
                            else if (angle < Math.PI * 0.75)
                                pin.Offset = Interp(new(0, Height * 0.5), ps1, 2 * (angle - Math.PI * 0.5));
                            else
                                pin.Offset = Interp(ps2, pb1, angle - Math.PI * 0.75);
                        }
                    }
                }
            }
        }
    }
}
