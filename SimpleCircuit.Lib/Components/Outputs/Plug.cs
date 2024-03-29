﻿using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A wall plug.
    /// </summary>
    [Drawable("WP", "A wall plug.", "Outputs", "earth child proof sealed")]
    public class Plug : DrawableFactory
    {
        private const string _earth = "earth";
        private const string _sealed = "sealed";
        private const string _child = "child";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(6, -1), new(1, -1)));

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "plug";

            [Description("The multiplicity of the wall plug.")]
            [Alias("m")]
            public int Multiple { get; set; } = 1;

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(), new(-1, 0)), "in", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(), new(1, 0)), "out", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPin(Pins["a"]);
                drawing.Arc(new(4, 0), Math.PI / 2, -Math.PI / 2, 4, null, 1);

                if (Variants.Contains(_earth))
                    DrawProtectiveConnection(drawing);
                if (Variants.Contains(_sealed))
                    DrawSealed(drawing);
                if (Variants.Contains(_child))
                    DrawChildProtection(drawing);

                if (Multiple > 1)
                {
                    drawing.Line(new(2.6, -1.4), new(-0.2, -4.2));
                    drawing.Text(Multiple.ToString(), new(-0.6, -4.6), new(-1, -1));
                }

                _anchors.Draw(drawing, this);
            }
            private void DrawProtectiveConnection(SvgDrawing drawing)
            {
                drawing.Line(new(0, 4), new(0, -4), new("earth"));
            }
            private void DrawChildProtection(SvgDrawing drawing)
            {
                drawing.Path(b => b.MoveTo(4, -6).LineTo(4, -4).MoveTo(4, 4).LineTo(4, 6), new("child"));
            }
            private void DrawSealed(SvgDrawing drawing)
            {
                drawing.Text("h", new(0.5, 3), new(-1, 1));
            }
        }
    }
}