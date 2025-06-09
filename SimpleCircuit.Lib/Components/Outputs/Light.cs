using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A light.
    /// </summary>
    [Drawable("LIGHT", "A light point.", "Outputs", "direction directional diverging projector emergency wall arei")]
    public class Light : DrawableFactory
    {
        private const string _direction = "direction";
        private const string _diverging = "diverging";
        private const string _projector = "projector";
        private const string _emergency = "emergency";
        private const string _wall = "wall";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable
        {
            private readonly CustomLabelAnchorPoints _anchors = new(2);
            private static readonly double _sqrt2 = Math.Sqrt(2) * 4;

            /// <inheritdoc />
            public override string Type => "light";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-4, 0), new(-1, 0)), "a", "p", "pos");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(4, 0), new(1, 0)), "b", "n", "neg");
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
                        if (Variants.Contains(Options.Arei))
                        {
                            SetPinOffset(0, new());
                            SetPinOffset(1, new());
                        }
                        else
                        {
                            SetPinOffset(0, new(-4, 0));
                            SetPinOffset(1, new(4, 0));
                        }
                        break;
                }
                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(this);

                _anchors[0] = new LabelAnchorPoint(new(0, -5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5), new(0, 1));

                if (!Variants.Contains(Options.Arei))
                    builder.Circle(new Vector2(), 4, style);
                else
                {
                    if (Variants.Contains(_wall))
                        DrawWall(builder, style);
                    if (Variants.Contains(_projector))
                        DrawProjector(builder, style);
                    if (Variants.Contains(_direction))
                        DrawDirectional(builder, Variants.Contains(_diverging), style);
                    if (Variants.Contains(_emergency))
                        DrawEmergency(builder, style);
                }

                builder.Cross(new(), _sqrt2, style);
                _anchors.Draw(builder, this, style);
            }

            private void DrawWall(IGraphicsBuilder builder, IStyle style)
            {
                builder.Line(new Vector2(-3, 5), new Vector2(3, 5), style);
                if (_anchors[1].Location.Y < 6)
                    _anchors[1] = new LabelAnchorPoint(new(0, 6), new(0, 1));
            }
            private void DrawProjector(IGraphicsBuilder builder, IStyle style)
            {
                builder.Path(b =>
                {
                    double c = Math.Cos(Math.PI * 0.95) * 6;
                    double s = Math.Sin(Math.PI * 0.95) * 6;
                    b.MoveTo(new(-c, -s));
                    b.ArcTo(6, 6, 0.0, false, false, new(c, -s));
                }, style);
                if (_anchors[0].Location.Y > -7)
                    _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
            }
            private void DrawDirectional(IGraphicsBuilder builder, bool diverging, IStyle style)
            {
                if (diverging)
                {
                    builder.Arrow(new(-2, 6), new(-6, 12), style);
                    builder.Arrow(new(2, 6), new(6, 12), style);
                }
                else
                {
                    builder.Arrow(new(-2, 6), new(-2, 12), style);
                    builder.Arrow(new(2, 6), new(2, 12), style);
                }
                if (_anchors[1].Location.Y < 13)
                    _anchors[1] = new LabelAnchorPoint(new(0, 13), new(0, 1));
            }
            private void DrawEmergency(IGraphicsBuilder builder, IStyle style)
            {
                builder.Circle(new(), 1.5, style.AsFilledMarker());
            }
        }
    }
}