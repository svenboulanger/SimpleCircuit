using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Default implementation for a component that has a location, an orientation,
    /// and can be scaled.
    /// </summary>
    public abstract class ScaledOrientedDrawable : OrientedDrawable
    {
        private double _scale = 1.0;

        /// <summary>
        /// Gets or sets the scale of the drawable.
        /// </summary>
        [Description("Scales the component by a scaling factor.")]
        public double Scale
        {
            get => _scale;
            set => _scale = Math.Max(0.1, value);
        }

        /// <summary>
        /// Creates a new scaled, oriented drawable.
        /// </summary>
        /// <param name="name">The name of the drawable.</param>
        /// <param name="options">The options.</param>
        protected ScaledOrientedDrawable(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Sets the offset of the specified pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOffset(int index, Vector2 offset)
        {
            if (index < Pins.Count && index >= 0)
                SetPinOffset(Pins[index], offset);
            else if (index < 0 && index >= -Pins.Count)
                SetPinOffset(Pins[Pins.Count + index], offset);
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Sets the offset of the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOffset(IPin pin, Vector2 offset)
        {
            if (pin is FixedOrientedPin fop)
                fop.Offset = offset;
            else if (pin is FixedPin fp)
                fp.Offset = offset;
            else
                throw new ArgumentException("Wanted to set offset of an invalid pin");
        }

        /// <summary>
        /// Sets the orientation of the specified pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <param name="orientation">The orientation.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOrientation(int index, Vector2 orientation)
        {
            if (index < Pins.Count && index >= 0)
                SetPinOffset(Pins[index], orientation);
            else if (index < 0 && index >= Pins.Count)
                SetPinOffset(Pins[Pins.Count + index], orientation);
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Sets the orientation of the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="orientation">The orientation.</param>
        /// <exception cref="ArgumentException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOrientation(IPin pin, Vector2 orientation)
        {
            if (pin is FixedOrientedPin fop)
                fop.RelativeOrientation = orientation;
            else
                throw new ArgumentException("Wanted to set orientation of an invalid pin");
        }

        /// <inheritdoc />
        protected override Transform CreateTransform() => new(Location, Transform * Matrix2.Scale(_scale, _scale));

        /// <inheritdoc />
        public override Vector2 TransformOffset(Vector2 local) => base.TransformOffset(local) * _scale;

        /// <inheritdoc />
        public override Vector2 TransformNormal(Vector2 local) => base.TransformNormal(local) * _scale;
    }
}
