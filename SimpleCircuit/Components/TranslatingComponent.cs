using SimpleCircuit.Functions;
using System;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A component that can translate.
    /// </summary>
    /// <seealso cref="IComponent" />
    /// <seealso cref="ITranslating" />
    public abstract class TranslatingComponent : IComponent, ITranslating
    {
        /// <summary>
        /// Gets the unknown x.
        /// </summary>
        /// <value>
        /// The unknown x.
        /// </value>
        protected Unknown UnknownX { get; }

        /// <summary>
        /// Gets the unknown y.
        /// </summary>
        /// <value>
        /// The unknown y.
        /// </value>
        protected Unknown UnknownY { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public PinCollection Pins { get; }

        /// <inheritdoc/>
        public Function X => UnknownX;

        /// <inheritdoc/>
        public Function Y => UnknownY;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslatingComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected TranslatingComponent(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UnknownX = new Unknown(name + ".x", UnknownTypes.X);
            UnknownY = new Unknown(name + ".y", UnknownTypes.Y);
            Pins = new PinCollection(this);
        }

        /// <inheritdoc/>
        public virtual void Apply(Minimizer minimizer)
        {
            minimizer.Minimize += new Squared(0.01 * X) + new Squared(0.01 * Y);
        }

        /// <inheritdoc/>
        public virtual void Render(SvgDrawing drawing)
        {
            drawing.TF = new Transform(X.Value, Y.Value, new Vector2(1, 0), new Vector2(0, 1));
            Draw(drawing);
        }

        /// <summary>
        /// Draws the transforming component (after the transform has been applied).
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected abstract void Draw(SvgDrawing drawing);
    }
}
