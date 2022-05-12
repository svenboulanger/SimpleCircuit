using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A light.
    /// </summary>
    [Drawable("LIGHT", "A light point.", "Outputs")]
    public class Light : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private static readonly double _sqrt2 = Math.Sqrt(2) * 2;

            [Description("The label next to the light.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "light";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");

                if (options?.ElectricalInstallation ?? false)
                    AddVariant("ei");
                DrawingVariants = Variant.All(
                    Variant.Do(DrawLamp),
                    Variant.IfNot("ei").Do(DrawCasing),
                    Variant.If("projector").Do(DrawProjector),
                    Variant.If("direction").Do(Variant.Map("diverging", DrawDirectional)),
                    Variant.If("emergency").Do(DrawEmergency));
                PinUpdate = Variant.Map("ei", UpdatePins);
            }

            private void DrawLamp(SvgDrawing drawing)
            {
                drawing.Path(b => b.MoveTo(-_sqrt2, -_sqrt2).LineTo(_sqrt2, _sqrt2).MoveTo(_sqrt2, -_sqrt2).LineTo(-_sqrt2, _sqrt2));

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -7), new Vector2(0, -1));
            }
            private void DrawCasing(SvgDrawing drawing)
            {
                drawing.Circle(new Vector2(), 4);
            }
            private void DrawProjector(SvgDrawing drawing)
            {
                drawing.Arc(new(), -Math.PI * 0.95, -Math.PI * 0.05, 6, new("projector"), 1);
            }
            private void DrawDirectional(SvgDrawing drawing, bool diverging)
            {
                var options = new PathOptions("direction") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow };
                if (diverging)
                {
                    drawing.Line(new(-2, 6), new(-6, 12), options);
                    drawing.Line(new(2, 6), new(6, 12), options);
                }
                else
                {
                    drawing.Line(new(-2, 6), new(-2, 12), options);
                    drawing.Line(new(2, 6), new(2, 12), options);
                }
            }
            private void DrawEmergency(SvgDrawing drawing)
            {
                drawing.Circle(new(), 1.5, new("dot"));
            }
            private void UpdatePins(bool eic)
            {
                if (eic)
                {
                    ((FixedOrientedPin)Pins[0]).Offset = new();
                    ((FixedOrientedPin)Pins[1]).Offset = new();
                }
                else
                {
                    ((FixedOrientedPin)Pins[0]).Offset = new(-4, 0);
                    ((FixedOrientedPin)Pins[1]).Offset = new(4, 0);
                }
            }
        }
    }
}