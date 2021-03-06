﻿namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An NPN transistor.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    /// <seealso cref="ILabeled" />
    [SimpleKey("QN", "NPN-type BJT", Category = "Analog"), SimpleKey("NPN", "NPN-type BJT", Category = "Analog")]
    public class BipolarNpnTransistor : TransformingComponent, ILabeled
    {
        /// <inheritdoc />
        public string Label { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarNpnTransistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BipolarNpnTransistor(string name)
            : base(name)
        {
            Pins.Add(new[] { "e", "emitter" }, "The emitter.", new Vector2(-8, 0), new Vector2(-1, 0));
            Pins.Add(new[] { "b", "base" }, "The base.", new Vector2(0, 6), new Vector2(0, 1));
            Pins.Add(new[] { "c", "collectr" }, "The collector.", new Vector2(8, 0), new Vector2(1, 0));
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(0, 6), new Vector2(0, 4),
                new Vector2(-6, 4), new Vector2(6, 4) 
            });
            drawing.Polyline(new[] { new Vector2(-3, 4), new Vector2(-6, 0), new Vector2(-8, 0) });
            drawing.Polyline(new[] { new Vector2(3, 4), new Vector2(6, 0), new Vector2(8, 0) });
            drawing.Polygon(new[] { new Vector2(-6, 0), new Vector2(-3.7, 1.4), new Vector2(-5.3, 2.6) });
            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"NPN {Name}";
    }
}
