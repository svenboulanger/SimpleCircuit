using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A component that translates, rotates and scales.
    /// </summary>
    /// <seealso cref="RotatingComponent" />
    /// <seealso cref="IScaling" />
    public abstract class TransformingComponent : RotatingComponent, IScaling
    {
        /// <summary>
        /// Gets the unknown scale.
        /// </summary>
        /// <value>
        /// The unknown scale.
        /// </value>
        protected Unknown UnknownScale { get; }

        /// <inheritdoc/>
        public Function Scale => UnknownScale;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformingComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pc">The pin collection.</param>
        protected TransformingComponent(string name)
            : base(name)
        {
            UnknownScale = new Unknown(name + ".s", UnknownTypes.Scale);
        }

        /// <inheritdoc/>
        public override void Apply(Minimizer minimizer)
        {
            base.Apply(minimizer);
            minimizer.Minimize += 1e3 * (new Squared(Scale) + 1.0 / new Squared(Scale));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            drawing.TF = new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value);
            Draw(drawing);
        }
    }
}
