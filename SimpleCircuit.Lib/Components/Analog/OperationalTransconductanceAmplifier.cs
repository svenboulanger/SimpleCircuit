﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// An Operational Transconductance Amplifier (OTA).
    /// </summary>
    [Drawable("TA", "A transconductance amplifier.", "Analog", "programmable")]
    [Drawable("OTA", "A transconductance amplifier.", "Analog", "programmable")]
    public class OperationalTransconductanceAmplifier : DrawableFactory
    {
        private const string _differentialInput = "diffin";
        private const string _swapInput = "swapin";
        private const string _differentialOutput = "diffout";
        private const string _swapOutput = "swapout";
        private const string _programmable = "programmable";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            var result = new Instance(name);

            // We will just add a differential input as the default
            result.Variants.Add(_differentialInput);
            return result;
        }

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "ota";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative input.", this, new(-5, -4), new(-1, 0)), "n", "inn", "neg");
                Pins.Add(new FixedOrientedPin("positive", "The positive input.", this, new(-5, 4), new(-1, 0)), "p", "inp", "pos");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power.", this, new(0, -7), new(0, -1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power.", this, new(0, 7), new(0, 1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, new(5, 0), new(1, 0)), "no", "outn");
                Pins.Add(new FixedOrientedPin("positiveoutput", "The output.", this, new(5, 0), new(1, 0)), "o", "out", "outp", "output");
                _anchors = new(
                    new LabelAnchorPoint(new(2, 8), new(1, 1)),
                    new LabelAnchorPoint(new(), new(), Vector2.UX, TextOrientationType.Transformed),
                    new LabelAnchorPoint(new(2, -8), new(1, -1)));
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
                        if (Variants.Contains(_differentialInput))
                        {
                            if (Variants.Contains(_swapInput))
                            {
                                SetPinOffset(0, new(-5, 4));
                                SetPinOffset(1, new(-5, -4));
                            }
                            else
                            {
                                SetPinOffset(0, new(-5, -4));
                                SetPinOffset(1, new(-5, 4));
                            }
                        }
                        else
                        {
                            SetPinOffset(0, new(-5, 0));
                            SetPinOffset(1, new(-5, 0));
                        }

                        if (Variants.Contains(_differentialOutput))
                        {
                            if (Variants.Contains(_swapOutput))
                            {
                                SetPinOffset(4, new(5, -4));
                                SetPinOffset(5, new(5, 4));
                            }
                            else
                            {
                                SetPinOffset(4, new(5, 4));
                                SetPinOffset(5, new(5, -4));
                            }
                        }
                        else
                        {
                            SetPinOffset(4, new(5, 0));
                            SetPinOffset(5, new(5, 0));
                        }


                        // Update anchors
                        _anchors[0] = new LabelAnchorPoint(new(2, 8), new(1, 1));
                        _anchors[2] = new LabelAnchorPoint(new(2, -8), new(1, -1));
                        if (Variants.Contains(_programmable))
                            _anchors[2] = new LabelAnchorPoint(new(2, -13), new(1, -1));
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                // The triangle
                builder.Polygon([
                    new(-5, -9),
                    new(5, -5),
                    new(5, 5),
                    new(-5, 9)
                ], style);

                // Pins and signs of the input
                if (Variants.Contains(_differentialInput))
                {
                    builder.ExtendPins(Pins, style, 2, "inn", "inp");
                    if (Variants.Contains(_swapInput))
                        builder.Signs(new(-2, -4), new(-2, 4), style);
                    else
                        builder.Signs(new(-2, 4), new(-2, -4), style);
                }
                else
                    builder.ExtendPin(Pins["n"], style);

                // Pins and signs of the output
                if (Variants.Contains(_differentialOutput))
                {
                    builder.ExtendPins(Pins, style, 3, "outp", "outn");
                    if (Variants.Contains(_swapOutput))
                        builder.Signs(new(6, 7), new(6, -7), style);
                    else
                        builder.Signs(new(6, -7), new(6, 7), style);
                }
                else
                    builder.ExtendPin(Pins["o"], style);

                // Programmable
                if (Variants.Contains(_programmable))
                    builder.Arrow(new(-7, 10), new(6, -12), style);

                // Labels
                _anchors.Draw(builder, this, style);
            }
        }
    }
}