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
        private const string _arei = "arei";
        private const string _direction = "direction";
        private const string _diverging = "diverging";
        private const string _projector = "projector";
        private const string _emergency = "emergency";

        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private static readonly double _sqrt2 = Math.Sqrt(2) * 4;

            [Description("The label next to the light.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "light";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");

                DrawingVariants = Variant.All(
                    Variant.Do(DrawLamp),
                    Variant.IfNot(_arei).Then(DrawCasing),
                    Variant.If(_projector).Then(DrawProjector),
                    Variant.If(_direction).Then(Variant.Map(_diverging, DrawDirectional)),
                    Variant.If(_emergency).Then(DrawEmergency));
                PinUpdate = Variant.Map(_arei, UpdatePins);

                if (options?.AREI ?? false)
                    AddVariant("arei");
            }

            private void DrawLamp(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Cross(new(), _sqrt2);

                // Label
                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new Vector2(0, -5), new Vector2(0, -1));
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
                var options = new PathOptions("direction") { EndMarker = PathOptions.MarkerTypes.Arrow };
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
            private void UpdatePins(bool arei)
            {
                if (arei)
                {
                    SetPinOffset(0, new());
                    SetPinOffset(1, new());
                }
                else
                {
                    SetPinOffset(0, new(-4, 0));
                    SetPinOffset(1, new(4, 0));
                }
            }
        }
    }
}