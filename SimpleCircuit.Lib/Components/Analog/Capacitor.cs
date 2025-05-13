using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    [Drawable("C", "A capacitor.", "Analog", "electrolytic programmable sensor")]
    public class Capacitor : DrawableFactory
    {
        private const string _curved = "curved";
        private const string _signs = "signs";
        private const string _electrolytic = "electrolytic";
        private const string _programmable = "programmable";
        private const string _sensor = "sensor";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors;

            /// <inheritdoc />
            public override string Type => "capacitor";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("pos", "The positive pin", this, new(-1.5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("neg", "the negative pin", this, new(1.5, 0), new(1, 0)), "n", "neg", "b");
                _anchors = new(
                    new LabelAnchorPoint(new(0, -5.5), new(0, -1), Appearance),
                    new LabelAnchorPoint(new(0, 5.5), new(0, 1), Appearance));
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
                        if (Variants.Contains(_electrolytic))
                        {
                            SetPinOffset(Pins["a"], new(-2.25, 0));
                            SetPinOffset(Pins["b"], new(2.25, 0));
                        }
                        else
                        {
                            SetPinOffset(Pins["a"], new(-1.5, 0));
                            SetPinOffset(Pins["b"], new(1.5, 0));
                        }

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
                builder.ExtendPins(Pins, Appearance, 3.5);
                switch (Variants.Select(_curved, _electrolytic))
                {
                    case 0:
                        // Plates
                        builder.Line(new(-1.5, -4), new(-1.5, 4), Appearance.AsStrokeWidth(1.0));
                        builder.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), Appearance.AsStroke());
                        if (Variants.Contains(_signs))
                            builder.Signs(new Vector2(-4, 3), new Vector2(5, 3), Appearance, vertical: true);
                        break;

                    case 1:
                        // Assymetric plates
                        builder.Rectangle(-2.25, -4, 1.5, 8, Appearance);
                        builder.Rectangle(0.75, -4, 1.5, 8, Appearance);
                        if (Variants.Contains(_signs))
                            builder.Signs(new(-5, 3), new(5, 3), Appearance, vertical: true);
                        break;

                    default:
                        // Plates
                        var plateAppearance = Appearance.AsStrokeWidth(1.0);
                        builder.Line(new(-1.5, -4), new(-1.5, 4), plateAppearance);
                        builder.Line(new(1.5, -4), new(1.5, 4), plateAppearance);
                        if (Variants.Contains(_signs))
                            builder.Signs(new(-4, 3), new(4, 3), Appearance, vertical: true);
                        break;
                }

                _anchors[0] = new LabelAnchorPoint(new(0, -5.5), new(0, -1), Appearance);
                _anchors[1] = new LabelAnchorPoint(new(0, 5.5), new(0, 1), Appearance);
                switch (Variants.Select(_programmable, _sensor))
                {
                    case 0:
                        builder.Arrow(new(-4, 4), new(6, -5), Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1), Appearance);
                        break;

                    case 1:
                        builder.Polyline([
                            new(-6, 6),
                            new(-4, 6),
                            new(4, -6)
                        ], Appearance);
                        _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1), Appearance);
                        break;
                }
                _anchors.Draw(builder, Labels);
            }
        }
    }
}