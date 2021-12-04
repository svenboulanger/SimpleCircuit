using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A battery.
    /// </summary>
    [Drawable("BAT", "A battery.", "Sources")]
    public class Battery : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private int _cells = 1;
            private double Length => _cells * 4 + 8;
            [Description("The label next to the battery.")]
            public string Label { get; set; }
            [Description("The number of cells.")]
            public int Cells
            {
                get => _cells;
                set
                {
                    _cells = value;
                    if (_cells < 1)
                        _cells = 1;
                }
            }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin", this, new(-8, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin", this, new(8, 0), new(1, 0)), "p", "pos", "a");
                DrawingVariants = Variant.Do(DrawBattery);
                PinUpdate = Variant.Do(UpdatePins);
            }
            private void DrawBattery(SvgDrawing drawing)
            {
                // Wires
                double offset = Length / 2, cellOffset = _cells * 2 - 1; ;
                drawing.Segments(new Vector2[]
                {
                new(-offset, 0), new(-cellOffset, 0),
                new(cellOffset, 0), new(offset, 0)
                }, new("wire"));

                // The cells
                double x = -_cells * 2 + 1;
                for (int i = 0; i < _cells; i++)
                {
                    drawing.Line(new(x, -2), new(x, 2), new("neg"));
                    x += 2.0;
                    drawing.Line(new(x, -6), new(x, 6), new("pos"));
                    x += 2.0;
                }

                // Add a little plus and minus next to the terminals!
                drawing.Segments(new Vector2[]
                {
                new(offset - 2, 2), new(offset - 2, 4),
                new(offset - 1, 3), new(offset - 3, 3)
                }, new("plus"));
                drawing.Line(new(-offset + 2, 2), new(-offset + 2, 4), new("minus"));

                // Depending on the orientation, let's anchor the text differently
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -8), new Vector2(0, -1));
            }
            private void UpdatePins()
            {
                double offset = Length / 2;
                ((FixedOrientedPin)Pins[0]).Offset = new(-offset, 0);
                ((FixedOrientedPin)Pins[1]).Offset = new(offset, 0);
            }
        }
    }
}