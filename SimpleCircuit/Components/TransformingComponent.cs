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
        private readonly bool _strictMirror;

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
        /// <param name="strictlyMirror">If <c>true</c>, the component can be mirrored but not scaled.</param>
        protected TransformingComponent(string name, bool strictlyMirror = false)
            : base(name)
        {
            UnknownScale = new Unknown(name + ".s", UnknownTypes.Scale);
            _strictMirror = strictlyMirror;
        }

        /// <inheritdoc/>
        public override void Apply(Minimizer minimizer)
        {
            base.Apply(minimizer);
            if (_strictMirror)
                minimizer.AddConstraint(new Squared(Scale) - 1);
            else
                minimizer.Minimize += 1e6 * (new Squared(Scale) + 1.0 / new Squared(Scale));
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            drawing.BeginTransform(new Transform(X.Value, Y.Value, normal, normal.Perpendicular * Scale.Value));
            Draw(drawing);
            drawing.EndTransform();
        }
    }
}
