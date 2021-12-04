using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A bus wire segment.
    /// </summary>
    [Drawable("BUS", "A bus or wire segment.", "Wires")]
    public class Bus : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The number of crossings. Can be used to indicate a bus.")]
            public int Crossings { get; set; } = 1;
            [Description("The label next to the direction.")]
            public string Label { get; set; }
            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("input", "The input.", this, new(-2, 0), new(-1, 0)), "i", "a", "in", "input");
                Pins.Add(new FixedOrientedPin("output", "The output.", this, new(2, 0), new(1, 0)), "o", "b", "out", "output");

                DrawingVariants = Variant.Map("straight", Draw);
                PinUpdate = Variant.Do(UpdatePins);
            }
            private void Draw(SvgDrawing drawing, bool straight)
            {
                drawing.Line(new(-Crossings - 2, 0), new(Crossings + 2, 0), new("wire"));
                if (Crossings > 0)
                {
                    List<Vector2> points = new(Crossings * 2);
                    for (int i = 0; i < Crossings; i++)
                    {
                        double x = i * 2 - Crossings + 1;
                        points.AddRange(straight ?
                            new Vector2[] { new(x, 3), new(x, -3) } :
                            new Vector2[] { new(x - 1.5, 3), new(x + 1.5, -3) });
                    }
                    drawing.Segments(points);
                }

                // The label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, -4), new(0, -1));
            }
            private void UpdatePins()
            {
                ((FixedOrientedPin)Pins[0]).Offset = new(-(Crossings - 1) - 2, 0);
                ((FixedOrientedPin)Pins[1]).Offset = new(Crossings + 1, 0);
            }
        }
    }
}