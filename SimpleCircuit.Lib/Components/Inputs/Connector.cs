using SimpleCircuit.Components.Pins;
using System.Linq;

namespace SimpleCircuit.Components.Inputs
{
    [SimpleKey("CONN", "A connector or fastener.", Category = "Inputs")]
    public class Connector : ScaledOrientedDrawable, ILabeled
    {
        private const double _invsq2 = 0.70710678118;
        private const double _khalf = 0.2652164898395;
        private const double _k = 0.552284749831;

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
            drawing.OpenBezier(new Vector2[]
            {
                new(_invsq2, -_invsq2),
                new(_invsq2 - _khalf, -_invsq2 - _khalf), new(_khalf, -1), new(0, -1),
                new(-_k, -1), new(-1, -_k), new(-1, 0),
                new(-1, _k), new(-_k, 1), new(0, 1),
                new(_khalf, 1), new(_invsq2 - _khalf, _invsq2 + _khalf), new(_invsq2, _invsq2)
            }.Select(v => v * 4));
        }
    }
}
