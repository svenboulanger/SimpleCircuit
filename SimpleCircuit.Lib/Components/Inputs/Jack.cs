using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Inputs
{
    [SimpleKey("JACK", "A (phone) jack.", Category = "Inputs")]
    public class Jack : ScaledOrientedDrawable, ILabeled
    {
        [Description("Adds a label next to the jack.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new jack plug.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">Options.</param>
        public Jack(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(0, 6), new(0, 1)), "p", "a", "pos");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(6, 0), new(1, 0)), "n", "b", "neg");
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            drawing.Segments(new Vector2[]
            {
                new(0, 2), new(0, 6),
                new(4, 0), new(6, 0)
            }, new("wire"));

            drawing.Circle(new(), 1.5);
            drawing.Circle(new(), 4);
            drawing.Circle(new(4, 0), 1, new("dot"));

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(-6, 0), new(-1, 0));
        }

        public override string ToString() => $"Jack {Name}";
    }
}
