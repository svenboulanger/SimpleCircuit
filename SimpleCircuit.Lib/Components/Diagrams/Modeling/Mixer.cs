﻿using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("MIX", "A mixer", "Modeling", "x")]
    public class Mixer : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name)
        {
            /// <inheritdoc />
            public override string Type => "mixer";

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                base.Draw(drawing);

                double s = Size * 0.5;
                if (!Variants.Contains(Square))
                    s /= Math.Sqrt(2.0);
                drawing.Line(new(-s, -s), new(s, s));
                drawing.Line(new(-s, s), new(s, -s));

                DrawLabels(drawing);
            }
        }
    }
}
