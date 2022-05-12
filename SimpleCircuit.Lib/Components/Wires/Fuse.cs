using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// A fuse.
    /// </summary>
    [Drawable("FUSE", "A fuse.", "Wires")]
    public class Fuse : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the fuse.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "fuse";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-6, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "b", "n", "neg");

                if (options?.ElectricalInstallation ?? false)
                    AddVariant("ei");

                DrawingVariants = Variant.FirstOf(
                    Variant.If("ei").Do(
                        Variant.If("auto").DoElse(
                            DrawAutoFuse,
                            DrawIEC
                        )
                    ),
                    Variant.If("ansi").Do(
                        Variant.If("alt").DoElse(
                            Variant.Do(DrawANSIalt),
                            Variant.Do(DrawANSI)
                        )
                    ),
                    Variant.If("auto").Do(DrawAutoFuse),
                    Variant.Do(DrawIEC));
            }

            private void DrawIEC(SvgDrawing drawing)
            {
                drawing.Polygon(new Vector2[]
                {
                new(-6, -3), new(6, -3),
                new(6, 3), new(-6, 3)
                });
                drawing.Path(b => b.MoveTo(-3.5, -3).Line(0, 6).MoveTo(3.5, -3).Line(0, 6));
            }
            private void DrawANSI(SvgDrawing drawing)
            {
                drawing.Polygon(new Vector2[]
                {
                new(-6, -3), new(6, -3),
                new(6, 3), new(-6, 3)
                });
                drawing.Line(new(-6, 0), new(6, 0));
            }
            private void DrawANSIalt(SvgDrawing drawing)
            {
                drawing.OpenBezier(new Vector2[]
                {
                new(-6, 0),
                new(-6, -1.65685424949), new(-4.65685424949, -3), new(-3, -3),
                new(-1.34314575051, -3), new(0, -1.65685424949), new(),
                new(0, 1.65685424949), new(1.34314575051, 3), new(3, 3),
                new(4.65685424949, 3), new(6, 1.65685424949), new(6, 0)
                });
            }
            private void DrawAutoFuse(SvgDrawing drawing)
            {
                drawing.Line(new(-6, 0), new(-4, 0), new("wire"));
                drawing.Line(new(-4, 0), new(4, -4));
                drawing.Polygon(new Vector2[]
                {
                new(4, -4), new(3.25, -5.5),
                new(1.25, -4.5), new(2, -3)
                }, new("dot"));
            }
        }
    }
}