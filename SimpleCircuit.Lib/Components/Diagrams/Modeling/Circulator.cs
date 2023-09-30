using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("CIRC", "A circulator", "Modeling")]
    public class Circulator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ModelingDrawable, ILabeled
        {
            protected override double Size => 12;

            /// <inheritdoc />
            public override string Type => "circulator";

            /// <inheritdoc />
            public Labels Labels { get; } = new Labels();

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
                drawing.Arc(new(), -Math.PI * 0.8, Math.PI * 0.8, Size * 0.25, intermediatePoints: 4);
                double x = Math.Cos(Math.PI * 0.8) * Size * 0.25;
                double y = Math.Sin(Math.PI * 0.8) * Size * 0.25;
                double s = Size * 0.1;
                drawing.Polyline(new Vector2[]
                {
                    new(x + s, y + s * 1.8), new(x, y), new(x + s * 1.8, y)
                });

                Labels.BoxedLabel(Variants, new(-Size * 0.5, -Size * 0.5), new(Size * 0.5, Size * 0.5), 1, -1, 1);
                Labels.Draw(drawing);
            }

        }
    }
}
