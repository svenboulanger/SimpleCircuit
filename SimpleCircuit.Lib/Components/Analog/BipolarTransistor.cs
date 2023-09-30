using SimpleCircuit.Circuits.Contexts;
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
            /// <inheritdoc />
            public override string Type => "npn";

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <summary>
            /// Creates a new <see cref="Npn"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Npn(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("emitter", "The emitter.", this, new(-6, 0), new(-1, 0)), "e", "emitter");
                Pins.Add(new FixedOrientedPin("base", "The base.", this, new(0, 4), new(0, 1)), "b", "base");
                Pins.Add(new FixedOrientedPin("collector", "The collector.", this, new(6, 0), new(1, 0)), "c", "collector");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // Transistor
                drawing.Arrow(new(-3, 4), new(-6, 0), new("emitter"));
                drawing.Line(new(3, 4), new(6, 0), new("collector"));
                drawing.Line(new(-6, 4), new(6, 4), new("base"));

                // Package
                if (Variants.Contains(_packaged))
                {
                    drawing.Circle(new(), 8.0);
                    Labels.SetDefaultPin(-1, location: new(0, -9), expand: new(0, -1));
                }
                else
                    Labels.SetDefaultPin(-1, location: new(0, -3), expand: new(0, -1));
                Labels.Draw(drawing);
            }
        }
        private class Pnp : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);

                // Transistor
                drawing.Arrow(new(6, 0), new(3, 4), new("emitter"));
                drawing.Line(new(-3, 4), new(-6, 0), new("collector"));
                drawing.Line(new(-6, 4), new(6, 4), new("base"));

                // Package
                if (Variants.Contains(_packaged))
                {
                    drawing.Circle(new(), 8.0);
                    Labels.SetDefaultPin(-1, location: new(0, -9), expand: new(0, -1));
                }
                else
                    Labels.SetDefaultPin(-1, location: new(0, -3), expand: new(0, -1));
                Labels.Draw(drawing);
            }
        }
    }
}
