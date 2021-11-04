using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A PNP transistor.
    /// </summary>
    [SimpleKey("QP", "A PNP-type bipolar junction transistor.", Category = "Analog")]
    [SimpleKey("PNP", "A PNP-type bipolar junction transistor.", Category = "Analog")]
    public class BipolarPnpTransistor : ScaledOrientedDrawable, ILabeled
    {
        private bool _packaged;

        /// <inheritdoc />
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets whether the symbol represents a packaged transistor.
        /// </summary>
        public bool Packaged
        {
            get => _packaged;
            set
            {
                _packaged = value;
                if (_packaged)
                    ((FixedOrientedPin)Pins[1]).Offset = new(0, 8);
                else
                    ((FixedOrientedPin)Pins[1]).Offset = new(0, 6);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarPnpTransistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BipolarPnpTransistor(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(-8, 0), new(-1, 0)), "c", "collector");
            Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 6), new(0, 1)), "b", "base");
            Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(8, 0), new(1, 0)), "e", "emitter");
            Packaged = GlobalOptions.PackagedTransistors;
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new[]
            {
                new Vector2(0, _packaged ? 8.0 : 6.0), new Vector2(0, 4),
                new Vector2(-6, 4), new Vector2(6, 4)
            });
            drawing.Polyline(new[] { new Vector2(-3, 4), new Vector2(-6, 0), new Vector2(-8, 0) });
            drawing.Polyline(new[] { new Vector2(3, 4), new Vector2(6, 0), new Vector2(8, 0) });
            drawing.Polygon(new[] { new Vector2(3, 4), new Vector2(3.7, 1.4), new Vector2(5.3, 2.6) });
            if (Packaged)
                drawing.Circle(new(), 8);
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
