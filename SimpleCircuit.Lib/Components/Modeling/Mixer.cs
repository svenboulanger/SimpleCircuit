﻿using System;

namespace SimpleCircuit.Components.Modeling
{
    [Drawable("MIX", "A mixer", "Modeling")]
    public class Mixer : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable
        {
            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

                double s = Size * 0.5;
                if (!Variants.Contains(Square))
                    s /= Math.Sqrt(2.0);
                drawing.Line(new(-s, -s), new(s, s));
                drawing.Line(new(-s, s), new(s, -s));
            }
        }
    }
}