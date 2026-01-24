using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Analog;

/// <summary>
/// Single-pole double throw switch.
/// </summary>
[Drawable("SPDT", "A single-pole double throw switch. The controlling pin is optional.", "Analog")]
public class SinglePoleDoubleThrow : DrawableFactory
{
    private const string _t1 = "t1";
    private const string _t2 = "t2";
    private const string _swap = "swap";

    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    private class Instance : ScaledOrientedDrawable
    {
        private readonly CustomLabelAnchorPoints _anchors = new(1);

        /// <inheritdoc />
        public override string Type => "spdt";

        [Description("The margin for labels.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Instance"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        public Instance(string name)
            : base(name)
        {
            AddPin(new FixedOrientedPin("pole", "The pole pin.", this, new(-6, 0), new(-1, 0)), "p", "pole");
            AddPin(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 0), new(0, 1)), "c", "ctrl");
            AddPin(new FixedOrientedPin("control2", "The backside controlling pin.", this, new(0, 0), new(0, -1)), "c2", "ctrl2");
            AddPin(new FixedOrientedPin("throw1", "The first throwing pin.", this, new(6, 4), new(1, 0)), "t1");
            AddPin(new FixedOrientedPin("throw2", "The second throwing pin.", this, new(6, -4), new(1, 0)), "t2");
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
                    if (Variants.Contains(_swap))
                    {
                        SetPinOffset(3, new(6, -4));
                        SetPinOffset(4, new(6, 4));
                    }
                    else
                    {
                        SetPinOffset(3, new(6, 4));
                        SetPinOffset(4, new(6, -4));
                    }

                    Vector2 loc = Variants.Select(_t1, _t2) switch
                    {
                        0 => new(0, Variants.Contains(_swap) ? -2 : 2),
                        1 => new(0, Variants.Contains(_swap) ? 2 : -2),
                        _ => new()
                    };
                    SetPinOffset(1, loc);
                    SetPinOffset(2, loc);

                    Vector2 a = new(-5, 0), b = new(5, 4);
                    Vector2 n = (b - a).Perpendicular;
                    n /= n.Length;
                    var style = context.Style.ModifyDashedDotted(this);
                    double m = 0.5 + 0.5 * style.LineThickness + LabelMargin; // Add 0.5 from the circles representing the terminals
                    _anchors[0] = new LabelAnchorPoint(Vector2.AtX(-2, a, b) + n * m, n);
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);
            builder.ExtendPins(Pins, style, 2, "p", "t1", "t2");

            // Terminals
            builder.Circle(new(-5, 0), 1, style);
            builder.Circle(new(5, 4), 1, style);
            builder.Circle(new(5, -4), 1, style);

            // Switch position
            switch (Variants.Select(_t1, _t2))
            {
                case 0: builder.Line(new(-4, 0), new(4, Variants.Contains(_swap) ? -4 : 4), style); break;
                case 1: builder.Line(new(-4, 0), new(4, Variants.Contains(_swap) ? 4 : -4), style); break;
                default: builder.Line(new(-4, 0), new(5, 0), style); break;
            }

            // Label
            _anchors.Draw(builder, this, style);
        }
    }
}