using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Analog
{
    /// <summary>
    /// A capacitor.
    /// </summary>
    [Drawable("C", "A capacitor.", "Analog")]
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

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            /// <inheritdoc />
            public Labels Labels { get; } = new();

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
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins, 3.5);
                switch (Variants.Select(_curved, _electrolytic))
                {
                    case 0:
                        // Plates
                        drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                        drawing.Path(b => b.MoveTo(new(3, -4)).CurveTo(new(1.5, -2), new(1.5, -0.5), new(1.5, 0)).SmoothTo(new(1.5, 2), new(3, 4)), new("neg"));
                        if (Variants.Contains(_signs))
                            drawing.Signs(new(-4, 3), new(5, 3), vertical: true);
                        Labels.SetDefaultPin(-1, location: new(0, -6), expand: new(0, -1));
                        Labels.SetDefaultPin(1, location: new(0, 6), expand: new(0, 1));
                        break;

                    case 1:
                        // Assymetric plates
                        drawing.Rectangle(-2.25, -4, 1.5, 8, options: new("pos"));
                        drawing.Rectangle(0.75, -4, 1.5, 8, options: new("neg", "marker"));
                        if (Variants.Contains(_signs))
                            drawing.Signs(new(-5, 3), new(5, 3), vertical: true);
                        Labels.SetDefaultPin(-1, location: new(0, -6), expand: new(0, -1));
                        Labels.SetDefaultPin(1, location: new(0, 6), expand: new(0, 1));
                        break;

                    default:
                        // Plates
                        drawing.Line(new(-1.5, -4), new(-1.5, 4), new("pos", "plane"));
                        drawing.Line(new(1.5, -4), new(1.5, 4), new("neg", "plane"));
                        if (Variants.Contains(_signs))
                            drawing.Signs(new(-4, 3), new(4, 3), vertical: true);
                        Labels.SetDefaultPin(-1, location: new(0, -6), expand: new(0, -1));
                        Labels.SetDefaultPin(1, location: new(0, 6), expand: new(0, 1));
                        break;
                }

                switch (Variants.Select(_programmable, _sensor))
                {
                    case 0:
                        drawing.Arrow(new(-4, 4), new(6, -5));
                        Labels.SetDefaultPin(-1, location: new(0, -7), expand: new(0, -1));
                        Labels.SetDefaultPin(1, location: new(0, 6), expand: new(0, 1));
                        break;

                    case 1:
                        drawing.Polyline(new Vector2[] { new(-6, 6), new(-4, 6), new(4, -6) });
                        Labels.SetDefaultPin(-1, location: new(0, -8), expand: new(0, -1));
                        Labels.SetDefaultPin(1, location: new(0, 6), expand: new(0, 1));
                        break;
                }
                Labels.Draw(drawing);
            }
        }
    }
}