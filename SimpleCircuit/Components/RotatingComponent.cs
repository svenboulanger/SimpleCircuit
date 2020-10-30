using SimpleCircuit.Functions;
using System;

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
        /// The conversion factor for radians to degrees.
        /// </summary>
        public const double Rad2Deg = 57.2957795131;

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
        public Function NormalX { get; }

        /// <inheritdoc/>
        public Function NormalY { get; }

        /// <summary>
        /// Gets the angle.
        /// </summary>
        /// <value>
        /// The angle.
        /// </value>
        public Function Angle { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotatingComponent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected RotatingComponent(string name)
            : base(name)
        {
            UnknownNormalX = new Unknown($"{Name}.nx", UnknownTypes.NormalX);
            UnknownNormalY = new Unknown($"{Name}.ny", UnknownTypes.NormalY);
            NormalX = UnknownNormalX;
            NormalY = UnknownNormalY;
            Angle = new NormalAtan2(NormalY, NormalX) * Rad2Deg;
        }

        /// <inheritdoc/>
        public override void Render(SvgDrawing drawing)
        {
            var normal = new Vector2(NormalX.Value, NormalY.Value);
            drawing.BeginTransform(new Transform(X.Value, Y.Value, normal, normal.Perpendicular));
            Draw(drawing);
            drawing.EndTransform();
        }

        public override void Apply(Minimizer minimizer)
        {
            base.Apply(minimizer);
            minimizer.Minimize += new Squared(NormalX - 1) / 1e4;
            minimizer.AddConstraint(new Squared(NormalX) + new Squared(NormalY) - 1, $"fix orientation of {Name}");
        }
    }
}
