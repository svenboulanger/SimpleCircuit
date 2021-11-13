using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A wire that can have a variable length.
    /// </summary>
    public class Wire : OrientedDrawable
    {
        [Description("Makes the wire a bus.")]
        public bool Bus { get; set; }

        /// <inheritdoc />
        public override int Order => -1;

        /// <summary>
        /// Creates a new wire.
        /// </summary>
        /// <param name="name">The name of the wire.</param>
        /// <param name="minimum">The minimum wire length.</param>
        public Wire(string name, double minimum, Options options)
            : base(name)
        {
            if (double.IsNaN(minimum))
                minimum = options.MinimumWireLength;
            Pins.Add(new FixedOrientedPin("start", "The starting point of the wire.", this, new(0, 0), new(-1, 0)), "start");
            Pins.Add(new MinimumOffsetPin("end", "The end point of the wire.", this, new(1, 0), minimum), "end");
        }

        /// <summary>
        /// Fixes the wire to the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public void Fix(double length)
        {
            var p = (MinimumOffsetPin)Pins[1];
            p.Fix = true;
            p.MinimumOffset = length;
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            if (Bus)
            {
                drawing.Polyline(new Vector2[] {
                    Pins[0].Location,
                    (Pins[0].Location + Pins[1].Location) / 2,
                    Pins[1].Location
                }, new("wire") { MiddleMarker = Drawing.PathOptions.MarkerTypes.Slash, Id = Name });
            }
            else
            {
                drawing.Line(Pins[0].Location, Pins[1].Location, new("wire") { Id = Name });
            }
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            Draw(drawing);
        }
    }
}
