using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;

namespace SimpleCircuit.Components.Outputs
{
    /// <summary>
    /// A motor.
    /// </summary>
    [Drawable("MOTOR", "A motor.", "Outputs")]
    public class Motor : DrawableFactory
    {
        private const string _signs = "signs";

        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            private static readonly CustomLabelAnchorPoints _anchors = new(
                new LabelAnchorPoint(new(0, -6), new(0, -1)),
                new LabelAnchorPoint(new(0, 6), new(0, 1)));

            /// <inheritdoc />
            public Labels Labels { get; } = new();

            /// <inheritdoc />
            public override string Type => "motor";

            /// <summary>
            /// Creates a new <see cref="Instance"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            public Instance(string name)
                : base(name)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(5, 0), new(1, 0)), "n", "neg", "b");
            }

            /// <inheritdoc />
            protected override void Draw(SvgDrawing drawing)
            {
                if (!Variants.Contains(Options.Arei))
                    drawing.ExtendPins(Pins);
                drawing.Circle(new(), 5);
                drawing.Text("M", new(), new());

                if (Variants.Contains(_signs))
                {
                    drawing.Path(b => b.MoveTo(-7, -4).LineTo(-5, -4).MoveTo(-6, -3).LineTo(-6, -5), new("plus"));
                    drawing.Line(new(5, -4), new(7, -4), new("minus"));
                }

                _anchors.Draw(drawing, Labels, this);
            }
        }
    }
}