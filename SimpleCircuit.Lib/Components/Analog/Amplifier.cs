﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A factory for amplifiers.
    /// </summary>
    [Drawable("A", "A generic amplifier.", "Analog", "schmitt trigger comparator programmable")]
    [Drawable("OA", "An operational amplifier.", "Analog", "schmitt trigger comparator programmable")]
    public class Amplifier : DrawableFactory
    {
        private const string _differentialInput = "diffin";
        private const string _swapInput = "swapin";
        private const string _differentialOutput = "diffout";
        private const string _swapOutput = "swapout";
        private const string _schmitt = "schmitt";
        private const string _comparator = "comparator";
        private const string _programmable = "programmable";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
        {
            switch (key)
            {
                case "OA":
                    var result = new Instance(name);
                    result.Variants.Add(_differentialInput);
                    return result;

                default:
                    return new Instance(name);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        private class Instance : ScaledOrientedDrawable
        {
            private readonly static Vector2
                _supplyPos = new(-2, -5),
                _supplyNeg = new(-2, 5),
                _inputPos = new(-8, -4),
                _inputNeg = new(-8, 4),
                _inputCommon = new(-8, 0),
                _outputPos = new(0, 4),
                _outputNeg = new(0, -4),
                _outputCommon = new(8, 0);
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "amplifier";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name) : base(name)
            {
                _anchors = new(
                    new LabelAnchorPoint(new(2, 5), new(1, 1), Appearance),
                    new LabelAnchorPoint(new(-2.5, 0), new(), Appearance),
                    new LabelAnchorPoint(new(2, -5), new(1, -1), Appearance)
                );
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
                        Pins.Clear();

                        // Add input pins
                        if (Variants.Contains(_differentialInput))
                        {
                            if (Variants.Contains(_swapInput))
                            {
                                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _inputNeg, new(-1, 0)), "i", "in", "inp", "pi", "p");
                                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _inputPos, new(-1, 0)), "inn", "ni", "n");
                            }
                            else
                            {
                                Pins.Add(new FixedOrientedPin("positiveinput", "The (positive) input.", this, _inputPos, new(-1, 0)), "i", "in", "inp", "pi", "p");
                                Pins.Add(new FixedOrientedPin("negativeinput", "The negative input.", this, _inputNeg, new(-1, 0)), "inn", "ni", "n");
                            }
                        }
                        else
                            Pins.Add(new FixedOrientedPin("input", "The input.", this, _inputCommon, new(-1, 0)), "i", "in", "inp", "pi", "p");

                        // Add power supply pins
                        Pins.Add(new FixedOrientedPin("positivepower", "The positive power supply.", this, _supplyPos, new(0, -1)), "vpos", "vp");
                        Pins.Add(new FixedOrientedPin("negativepower", "The negative power supply.", this, _supplyNeg, new(0, 1)), "vneg", "vn");

                        // Add output pins
                        if (Variants.Contains(_differentialOutput))
                        {
                            if (Variants.Contains(_swapOutput))
                            {
                                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _outputPos, new(1, 0)), "outn", "no");
                                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _outputNeg, new(1, 0)), "o", "out", "outp", "po");
                            }
                            else
                            {
                                Pins.Add(new FixedOrientedPin("negativeoutput", "The negative output.", this, _outputNeg, new(1, 0)), "outn", "no");
                                Pins.Add(new FixedOrientedPin("positiveoutput", "The (positive) output.", this, _outputPos, new(1, 0)), "o", "out", "outp", "po");
                            }
                        }
                        else
                            Pins.Add(new FixedOrientedPin("output", "The output.", this, _outputCommon, new(1, 0)), "o", "out", "outp", "po");

                        // Allow dashed/dotted lines
                        Appearance.LineStyle = Variants.Select(Dashed, Dotted) switch
                        {
                            0 => LineStyles.Dashed,
                            1 => LineStyles.Dotted,
                            _ => LineStyles.None
                        };
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                _anchors[0] = new LabelAnchorPoint(new(2, 5), new(1, 1), Appearance);
                _anchors[2] = new LabelAnchorPoint(new(2, -5), new(1, -1), Appearance);

                // The main triangle
                builder.Polygon(
                [
                    new(-8, -8),
                    new(8, 0),
                    new(-8, 8)
                ], Appearance);

                // Differential input?
                var markerAppearance = Appearance.CreateStrokeStyle();
                if (Variants.Contains(_differentialInput))
                {
                    builder.ExtendPins(Pins, Appearance, 2, "inp", "inn");
                    if (Variants.Contains(_swapInput))
                        builder.Signs(new(-5.5, 4), new(-5.5, -4), Appearance);
                    else
                        builder.Signs(new(-5.5, -4), new(-5.5, 4), Appearance);
                }
                else
                    builder.ExtendPin(Pins["in"], Appearance);

                // Differential output?
                if (Variants.Contains(_differentialOutput))
                {
                    builder.ExtendPins(Pins, Appearance, 5, "outp", "outn");
                    if (Variants.Contains(_swapOutput))
                        builder.Signs(new(6, -7), new(6, 7), Appearance);
                    else
                        builder.Signs(new(6, 7), new(6, -7), Appearance);

                    // Give more breathing room to the labels
                    _anchors[0] = new LabelAnchorPoint(new(2, 8.5), new(1, 1), Appearance);
                    _anchors[2] = new LabelAnchorPoint(new(2, -8.5), new(1, -1), Appearance);
                }
                else
                    builder.ExtendPin(Pins["out"], Appearance);

                // Programmable arrow
                if (Variants.Contains(_programmable))
                {
                    builder.Arrow(new(-7, 10), new(4, -8.5), Appearance);
                    _anchors[2] = new LabelAnchorPoint(new(2, -10), new(1, 1), Appearance);
                }

                // Comparator or schmitt trigger
                switch (Variants.Select(_comparator, _schmitt))
                {
                    case 0: // Comparator
                        builder.Path(b => b.MoveTo(new(-5, 2))
                        .LineTo(new(-3, 2))
                        .LineTo(new(-3, -2))
                        .LineTo(new(-1, -2)), Appearance.AsStroke());
                        break;

                    case 1: // Schmitt trigger
                        builder.Path(b =>
                        {
                            b.MoveTo(new(-6, 2))
                                .LineTo(new(-4, 2))
                                .LineTo(new(-4, -2))
                                .LineTo(new(-2, -2));
                            b.MoveTo(new(-4, 2))
                                .LineTo(new(-2, 2))
                                .LineTo(new(-2, -2))
                                .LineTo(new(0, -2));
                        }, Appearance.AsStroke());
                        break;
                }

                _anchors.Draw(builder, Labels);
            }
        }
    }
}
