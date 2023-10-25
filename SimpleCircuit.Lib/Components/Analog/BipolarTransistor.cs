using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A bipolar transistor.
    /// </summary>
    [Drawable("QN", "An NPN bipolar transistor.", "Analog", "packaged")]
    [Drawable("NPN", "An NPN bipolar transistor.", "Analog", "packaged")]
    [Drawable("QP", "A PNP bipolar transistor.", "Analog", "packaged")]
    [Drawable("PNP", "A PNP bipolar transistor.", "Analog", "packaged")]
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
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -3), new(0, -1)));

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
                _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                if (Variants.Contains(_packaged))
                {
                    drawing.Circle(new(), 8.0);
                    _anchors[0] = new LabelAnchorPoint(new(0, -9), new(0, -1));
                }
                _anchors.Draw(drawing, this);
            }
        }
        private class Pnp : ScaledOrientedDrawable, ILabeled
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -3), new(0, -1)));

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
                _anchors[0] = new LabelAnchorPoint(new(0, -3), new(0, -1));
                if (Variants.Contains(_packaged))
                {
                    drawing.Circle(new(), 8.0);
                    _anchors[0] = new LabelAnchorPoint(new(0, -9), new(0, -1));
                }
                _anchors.Draw(drawing, this);
            }
        }
    }
}
