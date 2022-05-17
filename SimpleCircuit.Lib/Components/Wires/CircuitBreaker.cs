using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Wires
{
    [Drawable("CB", "A circuit breaker.", "Wires")]
    public class CircuitBreaker : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the circuit breaker.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "circuitbreaker";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");

                DrawingVariants = Variant.FirstOf(
                    Variant.If("arei").Then(DrawAutoFuse),
                    Variant.Do(DrawRegular));

                if (options?.AREI ?? false)
                    AddVariant("arei");
            }

            private void DrawRegular(SvgDrawing drawing)
            {
                drawing.OpenBezier(new Vector2[]
                {
                    new(-4, -2),
                    new(-2, -4.5),
                    new(2, -4.5),
                    new(4, -2)
                });

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 3), new(0, 1));
            }

            private void DrawAutoFuse(SvgDrawing drawing)
            {
                drawing.Line(new(-6, 0), new(-4, 0), new("wire"));
                drawing.Line(new(-4, 0), new(4, -4));
                drawing.Polygon(new Vector2[]
                {
                    new(4, -4), new(3.25, -5.5),
                    new(1.25, -4.5), new(2, -3)
                }, new("dot"));

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 3), new(0, 1));
            }
        }
    }
}
