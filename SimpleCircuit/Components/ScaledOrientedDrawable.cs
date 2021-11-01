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
        public double Scale
        {
            get => _scale;
            set => _scale = Math.Max(0.1, value);
        }

        /// <summary>
        /// Creates a new scaled, oriented drawable.
        /// </summary>
        /// <param name="name"></param>
        protected ScaledOrientedDrawable(string name)
            : base(name)
        {
            _scale = GlobalOptions.Scale;
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            drawing.StartGroup(Name, GetType().Name.ToLower());
            drawing.BeginTransform(new(Location, Transform * Matrix2.Scale(_scale, _scale)));
            Draw(drawing);
            drawing.EndTransform();
            drawing.EndGroup();
        }

        /// <inheritdoc />
        public override Vector2 TransformOffset(Vector2 local) => base.TransformOffset(local) * _scale;

        /// <inheritdoc />
        public override Vector2 TransformNormal(Vector2 local) => base.TransformNormal(local) * _scale;
    }
}
