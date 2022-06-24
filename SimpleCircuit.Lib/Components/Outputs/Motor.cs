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
        public override IDrawable Create(string key, string name, Options options)
            => new Instance(name, options);

        private class Instance : ScaledOrientedDrawable, ILabeled
        {
            [Description("The label of the motor.")]
            public string Label { get; set; }

            /// <inheritdoc />
            public override string Type => "motor";

            public Instance(string name, Options options)
                : base(name, options)
            {
                Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
                Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(5, 0), new(1, 0)), "n", "neg", "b");
            }

            protected override void Draw(SvgDrawing drawing)
            {
                drawing.ExtendPins(Pins);
                drawing.Circle(new(), 5);
                drawing.Text("M", new(), new());

                if (Variants.Contains(_signs))
                {
                    drawing.Path(b => b.MoveTo(-7, -4).LineTo(-5, -4).MoveTo(-6, -3).LineTo(-6, -5), new("plus"));
                    drawing.Line(new(5, -4), new(7, -4), new("minus"));
                }

                if (!string.IsNullOrWhiteSpace(Label))
                    drawing.Text(Label, new(0, 6), new(0, 1));
            }
        }
    }
}