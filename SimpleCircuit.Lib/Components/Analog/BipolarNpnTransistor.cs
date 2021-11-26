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
        /// <inheritdoc />
        [Description("The label next to the transistor.")]
        public string Label { get; set; }

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

            if (options?.PackagedTransistors ?? false)
                AddVariant("packaged");
            DrawingVariants = Variant.Map("packaged", Draw);
            PinUpdate = Variant.Map("packaged", UpdatePins);
        }

        private void Draw(SvgDrawing drawing, bool packaged)
        {
            drawing.Segments(new Vector2[]
            {
                new(-6, 0), new(-8, 0),
                new(6, 0), new(8, 0),
                new(0, packaged ? 8 : 6), new(0, 4)
            }, new("wire"));
            drawing.Line(new(-3, 4), new(-6, 0), new("emitter") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });
            drawing.Line(new(3, 4), new(6, 0), new("collector"));
            drawing.Line(new(-6, 4), new(6, 4), new("base"));
            if (packaged)
                drawing.Circle(new(), 8.0);
            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));

        }
        private void UpdatePins(bool packaged)
        {
            ((FixedOrientedPin)Pins[1]).Offset = new(0, packaged ? 8 : 6);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"NPN {Name}";
    }
}
