using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

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

        /// <inheritdoc />
        protected override IEnumerable<string> GroupClasses
        {
            get
            {
                if (Packaged)
                    yield return "packaged";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarPnpTransistor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options that can be used for the component.</param>
        public BipolarPnpTransistor(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(-8, 0), new(-1, 0)), "c", "collector");
            Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 6), new(0, 1)), "b", "base");
            Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(8, 0), new(1, 0)), "e", "emitter");
            Packaged = options?.PackagedTransistors ?? false;
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            // Connection wires
            drawing.Segments(new Vector2[]
            {
                new(-6, 0), new(-8, 0),
                new(6, 0), new(8, 0),
                new(0, _packaged ? 8 : 6), new(0, 4)
            }, new("wire"));

            // Emitter
            drawing.Line(new(6, 0), new(3, 4), new("emitter") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

            // Collector
            drawing.Line(new(-3, 4), new(-6, 0), new("collector"));

            // Base
            drawing.Line(new(-6, 4), new(6, 4), new("base"));

            // Packaged transistor (circle around the transistor)
            if (Packaged)
                drawing.Circle(new(), 8.0);

            // The label
            if (!string.IsNullOrEmpty(Label))
                drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));
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
