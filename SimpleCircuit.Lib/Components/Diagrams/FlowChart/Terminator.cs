using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
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
        private class Instance : DiagramBlockInstance
        {
            private readonly CustomLabelAnchorPoints _anchors;
            private double _width = 0, _height = 0;

            /// <inheritdoc />
            public override string Type => "terminator";

            /// <summary>
            /// Gets or sets the width of the terminator.
            /// </summary>
            [Description("The width of the block. If 0, the width is calculated from content. The default is 0.")]
            [Alias("w")]
            public double Width { get; set; }

            /// <summary>
            /// Gets or sets the minimum width.
            /// </summary>
            [Description("The minimum width of the block. Only used when determining the width from contents.")]
            public double MinWidth { get; set; } = 0.0;

            /// <summary>
            /// Gets or sets the height of the terminator.
            /// </summary>
            [Description("The height of the block. If 0, the height is calculated from content. The default is 0.")]
            [Alias("h")]
            public double Height { get; set; }

            /// <summary>
            /// Gets or sets the minimum height.
            /// </summary>
            [Description("The minimum height of the block. Only used when determining the height from contents.")]
            public double MinHeight { get; set; } = 10.0;

            /// <inheritdoc />
            [Description("The margin of the label to the edge. Only used when sizing based on content.")]
            public double LabelMargin { get; set; } = 1.0;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                _anchors = new(new LabelAnchorPoint(new(), new(), Appearance));
            }

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
                            var b = bounds.Bounds.Expand(LabelMargin);
                            if (Height.IsZero())
                                _height = Math.Max(MinHeight, b.Height);
                            if (Width.IsZero())
                                _width = Math.Max(MinWidth, b.Width + _height);
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
                double a = _width * 0.5;
                double b = _height * 0.5;

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
