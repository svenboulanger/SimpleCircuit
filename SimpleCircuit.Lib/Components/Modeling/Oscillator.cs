﻿using System;

namespace SimpleCircuit.Components.Modeling
{
    [Drawable("OSC", "An oscillator", "Modeling")]
    public class Oscillator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable
        {
            protected override double Size => 10;

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
                drawing.AC(new(), Size * 0.25);
            }
        }
    }
}