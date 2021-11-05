using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An NPN transistor.
    /// </summary>
    [SimpleKey("QN", "An NPN-type bipolar junction transistor.", Category = "Analog")]
    [SimpleKey("NPN", "An NPN-type bipolar junction transistor.", Category = "Analog")]
    public class BipolarNpnTransistor : ScaledOrientedDrawable, ILabeled
    {
        private bool _packaged;

        /// <inheritdoc />
        [Description("The label next to the transistor.")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets whether the symbol represents a packaged transistor.
        /// </summary>
        [Description("Displays a packaged transistor.")]
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
        /// Initializes a new instance of the <see cref="BipolarNpnTransistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public BipolarNpnTransistor(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(-8, 0), new(-1, 0)), "e", "emitter");
            Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 6), new(0, 1)), "b", "base");
            Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(8, 0), new(1, 0)), "c", "collector");
            Packaged = options?.PackagedTransistors ?? false;
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
            drawing.Polygon(new[] { new Vector2(-6, 0), new Vector2(-3.7, 1.4), new Vector2(-5.3, 2.6) });

            if (Packaged)
                drawing.Circle(new(), 8.0);
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
