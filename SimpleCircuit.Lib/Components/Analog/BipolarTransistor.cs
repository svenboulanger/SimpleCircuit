using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

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

        private class Npn : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(1);

            /// <inheritdoc />
            public override string Type => "npn";

            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                // Package background
                if (Variants.Contains(_packaged))
                {
                    builder.Circle(new(), 8.0, style);
                    _anchors[0] = new LabelAnchorPoint(new(0, -8 - style.LineThickness * 0.5 - LabelMargin), new(0, -1));
                }
                else
                    _anchors[0] = new LabelAnchorPoint(new(0, -2 - style.LineThickness * 0.5 - LabelMargin), new(0, -1));

                // Transistor
                builder.ExtendPins(Pins, style);
                builder.Arrow(new(-3, 4), new(-6, 0), style);
                builder.Line(new(3, 4), new(6, 0), style);
                builder.Line(new(-6, 4), new(6, 4), style);
                _anchors.Draw(builder, this, style);
            }
        }
        private class Pnp : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(1);

            /// <inheritdoc />
            public override string Type => "pnp";

            [Description("The margin for labels.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

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
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                // Draw background package
                if (Variants.Contains(_packaged))
                {
                    builder.Circle(new(), 8.0, style);
                    _anchors[0] = new LabelAnchorPoint(new(0, -8 - style.LineThickness * 0.5 - LabelMargin), new(0, -1));
                }
                else
                    _anchors[0] = new LabelAnchorPoint(new(0, -2 - style.LineThickness * 0.5 - LabelMargin), new(0, -1));

                // Transistor
                builder.ExtendPins(Pins, style);
                builder.Arrow(new(6, 0), new(3, 4), style);
                builder.Line(new(-3, 4), new(-6, 0), style);
                builder.Line(new(-6, 4), new(6, 4), style);
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
