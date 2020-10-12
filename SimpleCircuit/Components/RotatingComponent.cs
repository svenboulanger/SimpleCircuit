using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A component that can rotate.
    /// </summary>
    /// <seealso cref="TranslatingComponent" />
    /// <seealso cref="IRotating" />
    public abstract class RotatingComponent : TranslatingComponent, IRotating
    {
        /// <summary>
        /// Gets the unknown normal x.
        /// </summary>
        /// <value>
        /// The unknown normal x.
        /// </value>
        protected Unknown UnknownNormalX { get; }

        /// <summary>
        /// Gets the unknown normal y.
        /// </summary>
        /// <value>
        /// The unknown normal y.
        /// </value>
        protected Unknown UnknownNormalY { get; }

        /// <inheritdoc/>
        public Function NormalX => UnknownNormalX;

        /// <inheritdoc/>
        public Function NormalY => UnknownNormalY;

        /// <summary>
        /// Initializes a new instance of the <see cref="RotatingComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected RotatingComponent(string name)
            : base(name)
        {
            UnknownNormalX = new Unknown($"{Name}.nx", UnknownTypes.NormalX);
            UnknownNormalY = new Unknown($"{Name}.ny", UnknownTypes.NormalY);
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            drawing.TF = new Transform(X.Value, Y.Value, normal, normal.Perpendicular);
            Draw(drawing);
        }
    }
}
