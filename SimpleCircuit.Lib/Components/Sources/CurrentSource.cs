using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Sources
{
    /// <summary>
    /// A voltage source.
    /// </summary>
    [Drawable("I", "A current source.", "Sources")]
    public class CurrentSource : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(),
                new LabelAnchorPoint());

            private readonly string _programmable = "programmable";

            /// <inheritdoc />
            public override string Type => "cs";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The current end point.", this, new(-6, 0), new(-1, 0)), "p", "b");
                Pins.Add(new FixedOrientedPin("negative", "The current starting point.", this, new(6, 0), new(1, 0)), "n", "a");
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
                        switch (Variants.Select(Options.American, Options.European))
                        {
                            case 1:
                                SetPinOffset(0, new(-4, 0));
                                SetPinOffset(1, new(4, 0));
                                break;

                            case 0:
                            default:
                                SetPinOffset(0, new(-6, 0));
                                SetPinOffset(1, new(6, 0));
                                break;
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.Modify(Style);
                builder.ExtendPins(Pins, style);
                switch (Variants.Select(Options.American, Options.European))
                {
                    case 1:
                        DrawEuropeanSource(builder, style);
                        break;

                    case 0:
                    default:
                        DrawAmericanSource(builder, style);
                        break;
                }
            }

            private void DrawAmericanSource(IGraphicsBuilder builder, IStyle style)
            {
                _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                builder.Circle(new(0, 0), 6, style);
                switch (Variants.Select("arrow", "ac"))
                {
                    case 1:
                        builder.Line(new(-3.5, 0), new(3.5, 0), style);
                        var marker = new Arrow(new(-3.5, 0), new(-1, 0));
                        marker.Draw(builder, style);
                        marker.Location = new(3.5, 0);
                        marker.Orientation = new(1, 0);
                        marker.Draw(builder, style);
                        break;

                    default:
                        builder.Arrow(new(-3, 0), new(3, 0), style);
                        break;
                }

                if (Variants.Contains(_programmable))
                {
                    builder.Arrow(new(-6, -6), new(7.5, 7.5), style);
                    if (_anchors[0].Location.Y > -7)
                        _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                    if (_anchors[1].Location.Y < 8.5)
                        _anchors[1] = new LabelAnchorPoint(new(0, 8.5), new(0, 1));
                }
                _anchors.Draw(builder, this, style);
            }
            private void DrawEuropeanSource(IGraphicsBuilder builder, IStyle style)
            {
                builder.Circle(new(), 4, style);
                builder.Line(new(0, -4), new(0, 4), style);

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));

                if (Variants.Contains(_programmable))
                {
                    builder.Arrow(new(-4, -4), new(6, 6), style);
                    if (_anchors[0].Location.Y > -5)
                        _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                    if (_anchors[1].Location.Y < 7)
                        _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                }
                _anchors.Draw(builder, this, style);
            }
        }
    }
}