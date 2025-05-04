using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// A connector.
    /// </summary>
    [Drawable("CONN", "A connector or fastener.", "Inputs", "male female")]
    public class Connector : DrawableFactory
    {
        private const string _male = "male";
        private const string _female = "female";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            /// <inheritdoc />
            public override string Type => "connector";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(-4, 0), new(-1, 0)), "n", "neg", "b");
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(2, 0), new(1, 0)), "p", "pos", "a");
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
                        switch (Variants.Select(Options.American))
                        {
                            case 0:
                                switch (Variants.Select(_male, _female))
                                {
                                    case 0:
                                    case 1:
                                        SetPinOffset(0, new());
                                        SetPinOffset(1, new());
                                        break;

                                    default:
                                        SetPinOffset(0, new(-2, 0));
                                        SetPinOffset(1, new(2, 0));
                                        break;
                                }
                                break;

                            default:
                                SetPinOffset(0, new(-4, 0));
                                SetPinOffset(1, new(1.5, 0));
                                break;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                switch (Variants.Select(Options.American))
                {
                    case 0:
                        switch (Variants.Select(_male, _female))
                        {
                            case 0:
                                builder.ExtendPin(Pins["n"], Appearance, 5);
                                builder.Polyline([
                                    new(-4, 4),
                                    new(),
                                    new(-4, -4)
                                ]);
                                if (Pins["p"].Connections > 0)
                                {
                                    _anchors[0] = new LabelAnchorPoint(new(1, 1), new(1, 1), Appearance);
                                    _anchors[1] = new LabelAnchorPoint(new(1, -1), new(1, -1), Appearance);
                                }
                                else
                                {
                                    _anchors[0] = _anchors[1] = new LabelAnchorPoint(new(1, 0), new(1, 0), Appearance);
                                }
                                break;

                            case 1:
                                builder.ExtendPin(Pins["n"], Appearance, 5);
                                builder.Polyline([
                                    new(4, 4),
                                    new(),
                                    new(4, -4)
                                ]);
                                if (Pins["p"].Connections > 0)
                                {
                                    _anchors[0] = new LabelAnchorPoint(new(5, 1), new(1, 1), Appearance);
                                    _anchors[1] = new LabelAnchorPoint(new(5, -1), new(1, -1), Appearance);
                                }
                                else
                                {
                                    _anchors[0] = _anchors[1] = new LabelAnchorPoint(new(5, 0), new(1, 0), Appearance);
                                }
                                break;

                            default:
                                builder.ExtendPins(Pins, Appearance,  5);
                                builder.Polyline([
                                    new(-6, 4),
                                    new(-2, 0),
                                    new(-6, -4)
                                ]);
                                builder.Polyline([
                                    new(-2, 4),
                                    new(2, 0),
                                    new(-2, -4)
                                ]);
                                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance);
                                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1), Appearance);
                                break;
                        }
                        break;

                    default:
                        builder.ExtendPin(Pins["n"], Appearance);
                        builder.ExtendPin(Pins["p"], Appearance, 4);
                        builder.Circle(new(), 1.5, Appearance);
                        double s = Math.Sqrt(2) * 2;
                        builder.Path(b => b.MoveTo(new(s, -s)).ArcTo(4, 4, 0, true, false, new(s, s)));
                        _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1), Appearance);
                        _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1), Appearance);
                        break;
                }

                _anchors.Draw(builder, this);
            }
        }
    }
}