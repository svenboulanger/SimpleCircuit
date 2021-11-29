using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Outputs
{
    [SimpleKey("MOTOR", "A motor.", Category = "Outputs")]
    public class Motor : ScaledOrientedDrawable, ILabeled
    {
        [Description("The label of the motor.")]
        public string Label { get; set; }

        /// <summary>
        /// Creates a new motor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public Motor(string name, Options options)
            : base(name, options)
        {
            Pins.Add(new FixedOrientedPin("positive", "The positive pin.", this, new(-5, 0), new(-1, 0)), "p", "pos", "a");
            Pins.Add(new FixedOrientedPin("negative", "The negative pin.", this, new(5, 0), new(1, 0)), "n", "neg", "b");

            DrawingVariants = Variant.All(
                Variant.Do(DrawMotor),
                Variant.If("signs").Do(DrawSigns));
        }

        private void DrawMotor(SvgDrawing drawing)
        {
            drawing.Circle(new(), 5);
            drawing.Text("M", new(), new());

            if (!string.IsNullOrWhiteSpace(Label))
                drawing.Text(Label, new(0, 5), new(0, 1));
        }
        private void DrawSigns(SvgDrawing drawing)
        {
            drawing.Segments(new Vector2[]
            {
                new(-7, -4), new(-5, -4),
                new(-6, -3), new(-6, -5)
            }, new("plus"));
            drawing.Segments(new Vector2[]
            {
                new(5, -4), new(7, -4)
            }, new("minus"));
        }

        public override string ToString() => $"Motor {Name}";
    }
}
