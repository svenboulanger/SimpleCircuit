using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Base class for transforming components.
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="I2DTransforming" />
    public abstract class TransformingComponent : IComponent, I2DTransforming
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public PinCollection Pins { get; protected set; }

        /// <inheritdoc/>
        public Function X { get; }

        /// <inheritdoc/>
        public Function Y { get; }

        /// <inheritdoc/>
        public Function NormalX { get; }

        /// <inheritdoc/>
        public Function NormalY { get; }

        /// <inheritdoc/>
        public Function Scale { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformingComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pc">The pin collection.</param>
        protected TransformingComponent(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            X = new Unknown(name + ".x", UnknownTypes.X);
            Y = new Unknown(name + ".y", UnknownTypes.Y);
            NormalX = new Unknown(name + ".nx", UnknownTypes.NormalX);
            NormalY = new Unknown(name + ".ny", UnknownTypes.NormalY);
            Scale = new Unknown(name + ".s", UnknownTypes.Scale);
            Pins = new PinCollection(this);
        }

        /// <inheritdoc/>
        public virtual void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(X) + new Squared(Y);
            minimizer.AddConstraint(new Squared(Scale) - 1);
        }

        /// <inheritdoc/>
        public virtual void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            drawing.TF = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            Draw(drawing);
        }

        /// <summary>
        /// Draws the transforming component (the transform has been applied).
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected abstract void Draw(SvgDrawing drawing);
    }
}
