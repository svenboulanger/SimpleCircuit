﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Digital
{
    /// <summary>
    /// An invertor.
    /// </summary>
    [Drawable("INV", "An invertor.", "Digital")]
    [Drawable("NOT", "An invertor.", "Digital")]
    public class Invertor : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, IStandardizedDrawable, IBoxDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "invertor";

            /// <inheritdoc />
            public Standards Supported { get; } = Standards.American | Standards.European;

            [Description("The margin for labels to the edge.")]
            [Alias("lm")]
            public double LabelMargin { get; set; } = 1.0;

            Vector2 IBoxDrawable.TopLeft => new(-5, -5);
            Vector2 IBoxDrawable.BottomRight => new(5, 5);

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("input", "The input pin.", this, new(-6, 0), new(-1, 0)), "in", "input");
                Pins.Add(new FixedOrientedPin("positivepower", "The positive power pin.", this, new(0, -3), new(0, -1)), "vpos", "vp");
                Pins.Add(new FixedOrientedPin("negativepower", "The negative power pin.", this, new(0, 3), new(0, 1)), "vneg", "vn");
                Pins.Add(new FixedOrientedPin("output", "The output pin.", this, new(9, 0), new(1, 0)), "out", "output");
                _anchors = new(
                    new LabelAnchorPoint(new(0, -4), new(1, -1), Appearance),
                    new LabelAnchorPoint(new(0, 4), new(1, 1), Appearance));
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
                        if (Variants.Contains(Options.European))
                        {
                            SetPinOffset(0, new(-5, 0));
                            SetPinOffset(1, new(0, -5));
                            SetPinOffset(2, new(0, 5));
                            SetPinOffset(3, new(8, 0));
                        }
                        else
                        {
                            SetPinOffset(0, new(-6, 0));
                            SetPinOffset(1, new(0, -3));
                            SetPinOffset(2, new(0, 3));
                            SetPinOffset(3, new(9, 0));
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                switch (Variants.Select(Options.European, Options.American))
                {
                    case 0: DrawInverterIEC(builder); break;
                    case 1:
                    default: DrawInverter(builder); break;
                }
            }
            private void DrawInverter(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, 2, "in", "out");
                builder.Polygon([
                    new(-6, 6),
                    new(6, 0),
                    new(-6, -6)
                ], Appearance);
                builder.Circle(new(7.5, 0), 1.5, Appearance);

                _anchors.Draw(builder, this);
            }

            private void DrawInverterIEC(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, Appearance, 2, "in", "out");

                builder.Rectangle(-5, -5, 10, 10, Appearance);
                builder.Circle(new(6.5, 0), 1.5, Appearance);
                builder.Text("1", new Vector2(), new Vector2(), Appearance);

                new OffsetAnchorPoints<IBoxDrawable>(BoxLabelAnchorPoints.Default, 1).Draw(builder, this);
            }
        }
    }
}