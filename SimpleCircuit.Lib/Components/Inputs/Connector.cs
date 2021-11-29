using SimpleCircuit.Components.Pins;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Inputs
{
    [SimpleKey("CONN", "A connector or fastener.", Category = "Inputs")]
    public class Connector : ScaledOrientedDrawable, ILabeled
    {
        [Description("Adds a label next to the connector.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new connector.
        /// </summary>
        /// <param name="name">The name of the connector.</param>
        /// <param name="options">The options.</param>
        public Connector(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(-6, 0), new(-1, 0)), "n", "neg", "b");
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(6, 0), new(1, 0)), "p", "pos", "a");

            DrawingVariants = Variant.Do(DrawConnector);
        }

        private void DrawConnector(SvgDrawing drawing)
        {
            drawing.Segments(new Vector2[]
            {
                new(-6, 0), new(-4, 0),
                new(2, 0), new(6, 0)
            }, new("wire"));

            drawing.Circle(new(), 1.5);
            drawing.Arc(new(), Math.PI / 4, -Math.PI / 4, 4, null, 3);
        }
    }
}
