﻿using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A factory for power planes.
    /// </summary>
    [Drawable("POW", "A power plane.", "General", "vdd vcc vss vee")]
    public class PowerFactory : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private static readonly CustomLabelAnchorPoints _anchors = new(new LabelAnchorPoint(new(0, -1.5), new(0, -1)));

            private const string _anchor = "anchor";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "power";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("a", "The pin.", this, new(), new(0, 1)), "x", "p", "a");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                if (Variants.Contains(_anchor))
                    drawing.Polyline(new Vector2[] { new(-4, 4), new(), new(4, 4) }, new("anchor"));
                else
                {
                    drawing.RequiredCSS.Add(".plane { stroke-width: 1pt; }");
                    drawing.Line(new Vector2(-5, 0), new Vector2(5, 0), new("plane"));
                }
                _anchors.Draw(drawing, this);
            }
        }
    }
}