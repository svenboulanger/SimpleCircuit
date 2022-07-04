using SimpleCircuit.Components.Pins;
using System;

namespace SimpleCircuit.Components.Inputs
{
    /// <summary>
    /// A connector.
    /// </summary>
    [Drawable("CONN", "A connector or fastener.", "Inputs")]
    public class Connector : DrawableFactory
    {
        private const string _male = "male";
        private const string _female = "female";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("Adds a label next to the connector.")]
            public string Label { get; set; }

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
                Variants.Changed += UpdatePins;
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                switch (Variants.Select(Options.American))
                {
                    case 0:
                        switch (Variants.Select(_male, _female))
                        {
                            case 0:
                                drawing.ExtendPin(Pins["n"], 5);
                                drawing.Polyline(new Vector2[]
                                {
                                    new(4, 4), new(), new(4, -4)
                                });
                                if (Pins["p"].Connections > 0)
                                    drawing.Text(Label, new(5, 1), new(1, 1));
                                else
                                    drawing.Text(Label, new(5, 0), new(1, 0));
                                break;

                            case 1:
                                drawing.ExtendPin(Pins["n"], 5);
                                drawing.Polyline(new Vector2[]
                                {
                                    new(-4, 4), new(), new(-4, -4)
                                });
                                if (Pins["p"].Connections > 0)
                                    drawing.Text(Label, new(1, 1), new(1, 1));
                                else
                                    drawing.Text(Label, new(1, 0), new(1, 0));
                                break;

                            default:
                                drawing.ExtendPins(Pins, 5);
                                drawing.Polyline(new Vector2[]
                                {
                                    new(-6, 4), new(-2, 0), new(-6, -4)
                                });
                                drawing.Polyline(new Vector2[]
                                {
                                    new(-2, 4), new(2, 0), new(-2, -4)
                                });
                                drawing.Text(Label, new(0, -5), new(0, -1));
                                break;
                        }
                        break;

                    default:
                        drawing.ExtendPin(Pins["n"]);
                        drawing.ExtendPin(Pins["p"], 4);
                        drawing.Circle(new(), 1.5);
                        drawing.Arc(new(), Math.PI / 4, -Math.PI / 4, 4, null, 3);

                        drawing.Text(Label, new(0, -5), new(0, -1));
                        break;
                }
            }

            private void UpdatePins(object sender, EventArgs e)
            {
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
                        SetPinOffset(0, new());
                        SetPinOffset(1, new());
                        break;
                }
            }
        }
    }
}