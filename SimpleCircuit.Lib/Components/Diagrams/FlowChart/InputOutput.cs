using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    [Drawable("FIO", "A Flowchart Input/Output.", "Flowchart", "parallelogram")]
    public class InputOutput : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : DiagramBlockInstance, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint());
            private double _width = 30.0, _height = 15.0;

            /// <summary>
            /// Variant for manual input.
            /// </summary>
            public const string Manual = "manual";

            /// <inheritdoc />
            public override string Type => "process";

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

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;

                if (Variants.Contains(Manual))
                {
                    double c = b - a * 0.25;
                    drawing.Path(builder =>
                    {
                        builder.MoveTo(-a, -c)
                            .LineTo(a, -b)
                            .LineTo(a, b)
                            .LineTo(-a, b)
                            .Close();
                    });
                    _anchors[0] = new LabelAnchorPoint(new(0, 0.5 * (b - c)), new());
                }
                else
                {
                    double c = a - Height * 0.25;
                    drawing.Path(builder =>
                    {
                        builder.MoveTo(-c, -b)
                            .LineTo(a, -b)
                            .LineTo(c, b)
                            .LineTo(-a, b)
                            .Close();
                    });
                    _anchors[0] = new LabelAnchorPoint(new(), new());
                }
                _anchors.Draw(drawing, this);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;

                static Vector2 Interp(Vector2 a, Vector2 b, double ka)
                {
                    double k = ka / (Math.PI * 0.5);
                    return (1 - k) * a + k * b;
                }

                if (Variants.Contains(Manual))
                {
                    double c = b - a * 0.25;
                    foreach (var pin in pins)
                    {
                        double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                        if (angle < -Math.PI * 0.75)
                            pin.Offset = Interp(new(-a, b), new(-a, -c), angle + Math.PI * 1.25);
                        else if (angle < -Math.PI * 0.25)
                            pin.Offset = Interp(new(-a, -c), new(a, -b), angle + Math.PI * 0.75);
                        else if (angle < Math.PI * 0.25)
                            pin.Offset = Interp(new(a, -b), new(a, b), angle + Math.PI * 0.25);
                        else if (angle < Math.PI * 0.75)
                            pin.Offset = Interp(new(a, b), new(-a, b), angle - Math.PI * 0.25);
                        else
                            pin.Offset = Interp(new(-a, b), new(-a, -c), angle - Math.PI * 0.75);
                    }
                }
                else
                {
                    double c = a - Height * 0.25;
                    foreach (var pin in pins)
                    {
                        double angle = Math.Atan2(pin.Orientation.Y, pin.Orientation.X);
                        if (angle < -Math.PI * 0.75)
                            pin.Offset = Interp(new(-a, b), new(-c, -b), angle + Math.PI * 1.25);
                        else if (angle < -Math.PI * 0.5)
                            pin.Offset = Interp(new(-c, -b), new(0, -b), angle + Math.PI * 0.75);
                        else if (angle < -Math.PI * 0.25)
                            pin.Offset = Interp(new(0, -b), new(a, -b), angle + Math.PI * 0.5);
                        else if (angle < Math.PI * 0.25)
                            pin.Offset = Interp(new(a, -b), new(c, b), angle + Math.PI * 0.25);
                        else if (angle < Math.PI * 0.5)
                            pin.Offset = Interp(new(c, b), new(0, b), angle - Math.PI * 0.25);
                        else if (angle < Math.PI * 0.75)
                            pin.Offset = Interp(new(0, b), new(-a, b), angle - Math.PI * 0.5);
                        else
                            pin.Offset = Interp(new(-a, b), new(-c, -b), angle - Math.PI * 0.75);
                    }
                }
            }
        }
    }
}
