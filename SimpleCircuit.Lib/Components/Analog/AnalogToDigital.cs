using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An analog-to-digital converter.
    /// </summary>
    [Drawable("ADC", "An analog-to-digital converter.", "Digital")]
    public class AnalogToDigital : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, IBoxDrawable
        {
            private const string _differentialInput = "diffin";
            private const string _swapInput = "swapin";
            private const string _differentialOutput = "diffout";
            private const string _swapOutput = "swapout";

            Vector2 IBoxDrawable.TopLeft => new(-Width * 0.5, -Height * 0.5);
            Vector2 IBoxDrawable.BottomRight => new(Width * 0.5, Height * 0.5);

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            [Description("The width of the ADC.")]
            [Alias("w")]
            public double Width { get; set; } = 18;

            [Description("The height of the ADC.")]
            [Alias("h")]
            public double Height { get; set; } = 12;

            /// <inheritdoc />
            public override string Type => "adc";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, new(-9, 0), new(-1, 0)), "input", "in", "pi", "inp");
                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, new(-9, 0), new(-1, 0)), "inn", "ni");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(9, 0), new(1, 0)), "outn", "no");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, new(9, 0), new(1, 0)), "output", "out", "po", "outp");
            }

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        double x = -Width / 2;
                        double y = Height / 4;
                        if (Variants.Contains(_differentialInput))
                        {
                            if (Variants.Contains(_swapInput))
                            {
                                SetPinOffset(0, new(x, y));
                                SetPinOffset(1, new(x, -y));
                            }
                            else
                            {
                                SetPinOffset(0, new(x, -y));
                                SetPinOffset(1, new(x, y));
                            }
                        }
                        else
                        {
                            SetPinOffset(0, new(x, 0));
                            SetPinOffset(1, new(x, 0));
                        }

                        if (Variants.Contains(_differentialOutput))
                        {
                            x = Width / 2 - Height / 4;
                            if (Variants.Contains(_swapOutput))
                            {
                                SetPinOffset(2, new(x, -y));
                                SetPinOffset(3, new(x, y));
                            }
                            else
                            {
                                SetPinOffset(2, new(x, y));
                                SetPinOffset(3, new(x, -y));
                            }
                        }
                        else
                        {
                            x = Width / 2;
                            SetPinOffset(2, new(x, 0));
                            SetPinOffset(3, new(x, 0));
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var options = Appearance.CreatePathOptions();
                var signOptions = Appearance.CreateMarkerOptions();
                if (Variants.Contains(_differentialInput))
                {
                    builder.ExtendPins(Pins, Appearance, this, 2, "inp", "inn");
                    double x = -Width / 2 + 3;
                    double y = Height / 4;
                    if (Variants.Contains(_swapInput))
                        builder.Signs(new(x, y), new(x, -y), signOptions);
                    else
                        builder.Signs(new(x, -y), new(x, y), signOptions);
                }
                else
                    builder.ExtendPin(Pins["in"], Appearance, this);

                if (Variants.Contains(_differentialOutput))
                {
                    builder.ExtendPins(Pins, Appearance, this, 4, "outp", "outn");
                    double x = Width / 2 - Height / 4 + 2;
                    double y = Height / 4 + 2.0;
                    if (Variants.Contains(_swapOutput))
                        builder.Signs(new(x, y), new(x, -y), signOptions);
                    else
                        builder.Signs(new(x, -y), new(x, y), signOptions);
                }
                else
                    builder.ExtendPin(Pins["out"], Appearance, this);

                builder.Polygon(
                [
                    new(-Width / 2, Height / 2), new(Width / 2 - Height / 2, Height / 2),
                    new(Width / 2, 0), new(Width / 2 - Height / 2, -Height / 2),
                    new(-Width / 2, -Height / 2)
                ], options);

                BoxLabelAnchorPoints.Default.Draw(builder, this);
            }
        }
    }
}