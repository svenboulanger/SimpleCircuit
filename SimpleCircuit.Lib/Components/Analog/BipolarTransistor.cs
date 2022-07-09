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
        private const string _packaged = "packaged";

        protected override IDrawable Factory(string key, string name)
        {
            return key switch
            {
                "QN" or "NPN" => new Npn(name),
                "QP" or "PNP" => new Pnp(name),
                _ => throw new ArgumentException($"Invalid key '{key}' for bipolar transistor.")
            };
        }

        private class Npn : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "npn";

            /// <summary>
            /// Creates a new <see cref="Npn"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Npn(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(-8, 0), new(-1, 0)), "e", "emitter");
                Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 6), new(0, 1)), "b", "base");
                Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(8, 0), new(1, 0)), "c", "collector");
            }

            /// <inheritdoc />
            public override void Reset()
            {
                base.Reset();
                SetPinOffset(1, new(0, Variants.Contains(_packaged) ? 8 : 6));
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-6, 0), new(-8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                {
                    if (Variants.Contains(_packaged))
                        drawing.Line(new(0, 4), new(0, 8), new("wire"));
                    else
                        drawing.Line(new(0, 4), new(0, 6), new("wire"));
                }
                if (Pins[2].Connections == 0)
                    drawing.Line(new(6, 0), new(8, 0), new("wire"));

                // Transistor
                drawing.Arrow(new(-3, 4), new(-6, 0), new("emitter"));
                drawing.Line(new(3, 4), new(6, 0), new("collector"));
                drawing.Line(new(-6, 4), new(6, 4), new("base"));

                // Package
                if (Variants.Contains(_packaged))
                    drawing.Circle(new(), 8.0);

                // Label
                drawing.Text(Label, new Vector2(0, -3), new Vector2(0, -1));

            }
        }
        private class Pnp : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label next to the transistor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "pnp";

            /// <summary>
            /// Creates a new <see cref="Pnp"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Pnp(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(-6, 0), new(-1, 0)), "c", "collector");
                Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 4), new(0, 1)), "b", "base");
                Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(6, 0), new(1, 0)), "e", "emitter");
            }

            /// <inheritdoc />
            public override void Reset()
            {
                base.Reset();
                SetPinOffset(1, new(0, Variants.Contains(_packaged) ? 8 : 6));
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                // Wires
                if (Pins[0].Connections == 0)
                    drawing.Line(new(-6, 0), new(-8, 0), new("wire"));
                if (Pins[1].Connections == 0)
                {
                    if (Variants.Contains(_packaged))
                        drawing.Line(new(0, 4), new(0, 8), new("wire"));
                    else
                        drawing.Line(new(0, 4), new(0, 6), new("wire"));
                }
                if (Pins[2].Connections == 0)
                    drawing.Line(new(6, 0), new(8, 0), new("wire"));

                // Transistor
                drawing.Arrow(new(6, 0), new(3, 4), new("emitter"));
                drawing.Line(new(-3, 4), new(-6, 0), new("collector"));
                drawing.Line(new(-6, 4), new(6, 4), new("base"));

                // Package
                if (Variants.Contains(_packaged))
                    drawing.Circle(new(), 8.0);

                // Label
                drawing.Text(Label, new(0, -3), new(0, -1));
            }
        }
    }
}
