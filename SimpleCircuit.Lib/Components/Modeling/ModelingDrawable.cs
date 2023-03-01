using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Components.Modeling
{
    /// <summary>
    /// A generic drawable used for modeling block diagrams.
    /// These blocks don't have an orientation, but they can be square or circular and have 8 pins in all major directions.
    /// </summary>
    public abstract class ModelingDrawable : LocatedDrawable, IScaledDrawable
    {
        public const string Square = "square";

        /// <summary>
        /// Gets the width of the drawable.
        /// </summary>
        protected virtual double Size => 8;

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        [Description("The scale of the block")]
        public double Scale { get; set; }
        public int OrientationDegreesOfFreedom { get; }

        /// <summary>
        /// Creates a new <see cref="ModelingDrawable"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        protected ModelingDrawable(string name)
            : base(name)
        {
            Pins.Add(new FixedOrientedPin("west", "The west pin", this, new(), new(-1, 0)), "w", "west", "l", "left");
            Pins.Add(new FixedOrientedPin("northwest", "The north-west pin", this, new(), new(-1, -1)), "nw", "northwest", "ul", "upleft");
            Pins.Add(new FixedOrientedPin("north", "The north pin", this, new(), new(0, -1)), "n", "north", "u", "up");
            Pins.Add(new FixedOrientedPin("northeast", "The north-east pin", this, new(), new(1, -1)), "ne", "northeast", "ur", "upright");
            Pins.Add(new FixedOrientedPin("southeast", "The south-east pin", this, new(), new(1, 1)), "se", "southeast", "dr", "downright");
            Pins.Add(new FixedOrientedPin("south", "The south pin", this, new(), new(0, 1)), "s", "south", "d", "down");
            Pins.Add(new FixedOrientedPin("southwest", "The south-west pin", this, new(), new(-1, 1)), "sw", "southwest", "dl", "downleft");
            Pins.Add(new FixedOrientedPin("east", "The east pin", this, new(), new(1, 0)), "e", "east", "r", "right");
        }

        /// <inheritdoc />
        protected override Transform CreateTransform() => new(Location, Matrix2.Scale(Scale, Scale));

        private void SetPinOffset(int index, Vector2 offset)
            => ((FixedOrientedPin)Pins[index]).Offset = offset;

        /// <inheritdoc />
        public override bool Reset(IResetContext context)
        {
            if (!base.Reset(context))
                return false;

            double s = Size * 0.5;
            SetPinOffset(0, new(-s, 0));
            SetPinOffset(2, new(0, -s));
            SetPinOffset(5, new(0, s));
            SetPinOffset(7, new(s, 0));

            if (!Variants.Contains(Square))
                s /= Math.Sqrt(2.0);
            SetPinOffset(1, new(-s, -s));
            SetPinOffset(3, new(s, -s));
            SetPinOffset(4, new(s, s));
            SetPinOffset(6, new(-s, s));
            return true;
        }

        /// <inheritdoc />
        protected override void Draw(SvgDrawing drawing)
        {
            if (Variants.Contains(Square))
                drawing.Rectangle(Size, Size);
            else
                drawing.Circle(new(), Size * 0.5);
        }

        /// <inheritdoc />
        public Vector2 TransformOffset(Vector2 local) => local * Scale;

        /// <inheritdoc />
        public Vector2 TransformNormal(Vector2 local) => local;
    }
}
