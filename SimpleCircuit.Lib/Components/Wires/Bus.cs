using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Wires;

/// <summary>
/// A bus wire segment.
/// </summary>
[Drawable("BUS", "A bus or wire segment.", "Wires")]
public class Bus : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors;

        private const string _straight = "straight";

        [Description("The number of crossings. Can be used to indicate a bus.")]
        [Alias("c")]
        public int Crossings { get; set; } = 1;

        /// <inheritdoc />
        public override string Type => "bus";

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">The name.</param>
        public Instance(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("input", "The input.", this, new(0, 0), new(-1, 0)), "i", "a", "in", "input");
            Pins.Add(new FixedOrientedPin("output", "The output.", this, new(0, 0), new(1, 0)), "o", "b", "out", "output");
            _anchors = new(
                new LabelAnchorPoint(new(0, -4), new(0, -1)),
                new LabelAnchorPoint(new(0, 4), new(0, 1)));
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            builder.ExtendPins(Pins, style, Crossings + 2);

            bool straight = Variants.Contains(_straight);
            if (Crossings > 0)
            {
                builder.Path(b =>
                {
                    for (int i = 0; i < Crossings; i++)
                    {
                        double x = i * 2 - Crossings + 1;
                        if (straight)
                            b.MoveTo(new(x, 3)).Line(new(0, -6));
                        else
                            b.MoveTo(new(x - 1.5, 3)).Line(new(3, -6));
                    }
                }, style);
            }

            _anchors.Draw(builder, this, style);
        }
    }
}