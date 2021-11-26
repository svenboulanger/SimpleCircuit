using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A wire segment that has a bit more information.
    /// </summary>
    [SimpleKey("SEG", "A wire segment.", Category = "Wires")]
    public class Segment : ScaledOrientedDrawable
    {
        /// <summary>
        /// Creates a new wire segment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        public Segment(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("input", "The input.", this, new(-4, 0), new(-1, 0)), "i", "a", "in", "input");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(4, 0), new(1, 0)), "o", "b", "out", "output");

            DrawingVariants = Variant.All(
                Variant.Do(DrawWire),
                Variant.If("underground").Do(DrawUnderground),
                Variant.If("air").Do(DrawAir),
                Variant.If("tube").Do(DrawTube),
                Variant.If("inwall").Do(DrawInWall),
                Variant.If("onwall").Do(DrawOnWall)
                );
        }

        private void DrawWire(SvgDrawing drawing) =>
            drawing.Line(new(-4, 0), new(4, 0), new("wire"));
        private static void DrawUnderground(SvgDrawing drawing)
        {
            drawing.Segments(new Vector2[]
            {
                new(-4, -5), new(4, -5),
                new(-2.5, -3.5), new(2.5, -3.5),
                new(-1, -2), new(1, -2)
            });
        }
        private static void DrawAir(SvgDrawing drawing) => drawing.Circle(new(), 2);
        private static void DrawTube(SvgDrawing drawing) => drawing.Circle(new(0, -3.5), 1.5);
        private static void DrawInWall(SvgDrawing drawing)
        {
            drawing.Polyline(new Vector2[] { new(-3, -2), new(-3, -5), new(3, -5), new(3, -2) });
            drawing.Line(new(0, -2), new(0, -5));
        }
        private static void DrawOnWall(SvgDrawing drawing)
        {
            drawing.Polyline(new Vector2[] { new(-3, 5), new(-3, 2), new(3, 2), new(3, 5) });
            drawing.Line(new(0, 5), new(0, 2));
        }

        /// <inheritdoc />
        public override string ToString() => $"Segment {Name}";
    }
}
