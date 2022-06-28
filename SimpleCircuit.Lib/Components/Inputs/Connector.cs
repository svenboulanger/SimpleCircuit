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
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPin(Pins["n"]);
                drawing.ExtendPin(Pins["p"], 4);
                drawing.Circle(new(), 1.5);
                drawing.Arc(new(), Math.PI / 4, -Math.PI / 4, 4, null, 3);

                drawing.Text(Label, new(0, -5), new(0, -1));
            }
        }
    }
}