using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using System.Collections.Generic;

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
            private readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -5.5), new(0, -1)),
                new LabelAnchorPoint(new(0, 5.5), new(0, 1)));

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
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
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
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, 3.5);
                switch (Variants.Select(_curved, _electrolytic))
                {
                    case 0:
                        // Plates
                        builder.RequiredCSS.Add(".plane { stroke-width: 1pt; }");
                        builder.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                        builder.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), new("neg"));
                        if (Variants.Contains(_signs))
                            builder.Signs(new(-4, 3), new(5, 3), vertical: true);
                        break;

                    case 1:
                        // Assymetric plates
                        builder.Rectangle(-2.25, -4, 1.5, 8, options: new("pos"));
                        builder.Rectangle(0.75, -4, 1.5, 8, options: new("neg", "marker"));
                        if (Variants.Contains(_signs))
                            builder.Signs(new(-5, 3), new(5, 3), vertical: true);
                        break;

                    default:
                        // Plates
                        builder.RequiredCSS.Add(".plane { stroke-width: 1pt; }");
                        builder.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                        builder.Line(new(1.5, -4), new(1.5, 4), new("neg", "plane"));
                        if (Variants.Contains(_signs))
                            builder.Signs(new(-4, 3), new(4, 3), vertical: true);
                        break;
                }

                _anchors[0] = new LabelAnchorPoint(new(0, -5.5), new(0, -1));
                _anchors[1] = new LabelAnchorPoint(new(0, 5.5), new(0, 1));
                switch (Variants.Select(_programmable, _sensor))
                {
                    case 0:
                        builder.Arrow(new(-4, 4), new(6, -5));
                        _anchors[0] = new LabelAnchorPoint(new(0, -6), new(0, -1));
                        break;

                    case 1:
                        builder.Polyline([
                            new(-6, 6),
                            new(-4, 6),
                            new(4, -6)
                        ]);
                        _anchors[0] = new LabelAnchorPoint(new(0, -7), new(0, -1));
                        _anchors[1] = new LabelAnchorPoint(new(0, 7), new(0, 1));
                        break;
                }
                _anchors.Draw(builder, Labels);
            }
        }
    }
}