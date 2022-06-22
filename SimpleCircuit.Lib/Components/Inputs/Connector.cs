using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// A connector.
    /// </summary>
    [Drawable("CONN", "A connector or fastener.", "Inputs")]
    public class Connector : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("Adds a label next to the connector.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "connector";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(-4, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(2, 0), new(1, 0)), "p", "pos", "a");
                DrawingVariants = Variant.Do(DrawConnector);
            }
            private void DrawConnector(SvgDrawing drawing)
            {
                drawing.ExtendPin(Pins["n"]);
                drawing.ExtendPin(Pins["p"], 4);
                drawing.Circle(new(), 1.5);
                drawing.Arc(new(), Math.PI / 4, -Math.PI / 4, 4, null, 3);
            }
        }
    }
}