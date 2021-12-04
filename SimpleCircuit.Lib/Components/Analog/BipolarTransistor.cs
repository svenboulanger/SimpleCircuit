using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A bipolar transistor.
    /// </summary>
    [Drawable(new[] { "QN", "NPN" }, "An NPN bipolar transistor.", new[] { "Analog" })]
    [Drawable(new[] { "QP", "PNP" }, "A PNP bipolar transistor.", new[] { "Analog" })]
    public class BipolarTransistor : DrawableFactory
    {
        /// <inheritdoc />
        public override IDrawable Create(string key, string name, Options options)
        {
            switch (key)
            {
                case "QN":
                case "NPN":
                    return new Npn(name, options);
                case "QP":
                case "PNP":
                    return new Pnp(name, options);
                default:
                    throw new ArgumentException($"Invalid key '{key}' for bipolar transistor.");
            }
        }

        private class Npn : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }
            public Npn(string name, Options options)
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
        }
        private class Pnp : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }
            public Pnp(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(-8, 0), new(-1, 0)), "c", "collector");
                Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 6), new(0, 1)), "b", "base");
                Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(8, 0), new(1, 0)), "e", "emitter");

                if (options?.PackagedTransistors ?? false)
                    AddVariant("packaged");
                DrawingVariants = Variant.Map("packaged", Draw);
                PinUpdate = Variant.Map("packaged", UpdatePins);
            }

            private void Draw(SvgDrawing drawing, bool packaged)
            {
                // Connection wires
                drawing.Segments(new Vector2[]
                {
                new(-6, 0), new(-8, 0),
                new(6, 0), new(8, 0),
                new(0, packaged ? 8 : 6), new(0, 4)
                }, new("wire"));

                // Emitter
                drawing.Line(new(6, 0), new(3, 4), new("emitter") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });

                // Collector
                drawing.Line(new(-3, 4), new(-6, 0), new("collector"));

                // Base
                drawing.Line(new(-6, 4), new(6, 4), new("base"));

                // Packaged transistor (circle around the transistor)
                if (packaged)
                    drawing.Circle(new(), 8.0);

                // The label
                if (!string.IsNullOrEmpty(Label))
                    drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));
            }
            private void UpdatePins(bool packaged)
            {
                ((FixedOrientedPin)Pins[1]).Offset = new(0, packaged ? 8 : 6);
            }
        }
    }
}
