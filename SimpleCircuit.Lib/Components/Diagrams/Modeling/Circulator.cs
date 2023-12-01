using System;

namespace SimpleCircuit.Components.Diagrams.Modeling
{
    [Drawable("CIRC", "A circulator.", "Modeling", "rotate")]
    public class Circulator : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        private class Instance(string name) : ModelingDrawable(name, 12.0)
        {
            /// <inheritdoc />
            public override string Type => "circulator";

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
                DrawLabels(drawing);
            }
        }
    }
}
