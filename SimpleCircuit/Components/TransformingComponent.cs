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
        public PinCollection Pins { get; }

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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
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
        public abstract void Apply(Minimizer minimizer);

        /// <inheritdoc/>
        public abstract void Render(SvgDrawing drawing);
    }
}
