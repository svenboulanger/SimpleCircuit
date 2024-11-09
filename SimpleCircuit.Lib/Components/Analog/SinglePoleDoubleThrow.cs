using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Analog
{
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

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private readonly static CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(-3, 3), new(-1, 1)));

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "spdt";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("pole", "The pole pin.", this, new(-6, 0), new(-1, 0)), "p", "pole");
                Pins.Add(new FixedOrientedPin("control", "The controlling pin.", this, new(0, 0), new(0, 1)), "c", "ctrl");
                Pins.Add(new FixedOrientedPin("control2", "The backside controlling pin.", this, new(0, 0), new(0, -1)), "c2", "ctrl2");
                Pins.Add(new FixedOrientedPin("throw1", "The first throwing pin.", this, new(6, 4), new(1, 0)), "t1");
                Pins.Add(new FixedOrientedPin("throw2", "The second throwing pin.", this, new(6, -4), new(1, 0)), "t2");
            }

            /// <inheritdoc />
            public override bool Reset(IResetContext context)
            {
                if (!base.Reset(context))
                    return false;
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
                return true;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                builder.ExtendPins(Pins, 2, "p", "t1", "t2");

                // Terminals
                builder.Circle(new(-5, 0), 1);
                builder.Circle(new(5, 4), 1);
                builder.Circle(new(5, -4), 1);

                // Switch position
                switch (Variants.Select(_t1, _t2))
                {
                    case 0: builder.Line(new(-4, 0), new(4, Variants.Contains(_swap) ? -4 : 4)); break;
                    case 1: builder.Line(new(-4, 0), new(4, Variants.Contains(_swap) ? 4 : -4)); break;
                    default: builder.Line(new(-4, 0), new(5, 0)); break;
                }

                // Label
                _anchors.Draw(builder, this);
            }
        }
    }
}