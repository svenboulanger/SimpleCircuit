using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.FlowChart
{
    [Drawable("FT", "A Flowchart Terminator.", "Flowchart", "pill")]
    public class Terminator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <inheritdoc />
        private class Instance(string name) : DiagramBlockInstance(name)
        {
            private static readonly CustomLabelAnchorPoints _anchors = new(new LabelAnchorPoint(new(), new()));
            private double _width = 30.0, _height = 20.0;

            /// <inheritdoc />
            public override string Type => "terminator";

            /// <summary>
            /// Gets or sets the width of the terminator.
            /// </summary>
            [Description("The width of the block.")]
            [Alias("w")]
            public double Width
            {
                get => _width;
                set
                {
                    if (value < _height)
                        _width = _height;
                    else
                        _width = value;
                }
            }

            /// <summary>
            /// Gets or sets the height of the terminator.
            /// </summary>
            [Description("The height of the block.")]
            [Alias("h")]
            public double Height
            {
                get => _height;
                set
                {
                    if (value > _width)
                        _height = _width;
                    else
                        _height = value;
                }
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                double a = Width * 0.5;
                double b = Height * 0.5;

                builder.Path(builder => builder.MoveTo(new(-a + b, -b))
                    .LineTo(new(a - b, -b))
                    .ArcTo(b, b, 0.0, false, true, new(a - b, b))
                    .LineTo(new(-a + b, b))
                    .ArcTo(b, b, 0.0, false, true, new(-a + b, -b)).Close());
                _anchors.Draw(builder, this);
            }

            /// <inheritdoc />
            protected override void UpdatePins(IReadOnlyList<LooselyOrientedPin> pins)
            {
                double a = _width * 0.5;
                double b = _height * 0.5;
                foreach (var pin in pins)
                {
                    if ((pin.Orientation - new Vector2(0, -1)).IsZero())
                        pin.Offset = new(0, -b);
                    else if ((pin.Orientation - new Vector2(0, 1)).IsZero())
                        pin.Offset = new(0, b);
                    else if (pin.Orientation.X > 0)
                        pin.Offset = new Vector2(a - b, 0) + pin.Orientation * b;
                    else
                        pin.Offset = new Vector2(b - a, 0) + pin.Orientation * b;
                }
            }
        }
    }
}
