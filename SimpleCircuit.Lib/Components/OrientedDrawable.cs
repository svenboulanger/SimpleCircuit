using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Default implementation for a component that has a location and an orientation.
    /// </summary>
    public abstract class OrientedDrawable : LocatedDrawable, IOrientedDrawable
    {
        private int _dof = 2;
        private Vector2 _p, _b;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrientedDrawable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected OrientedDrawable(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        public Matrix2 Transform { get; set; } = Matrix2.Identity;

        /// <inheritdoc />
        public bool ConstrainOrientation(Vector2 p, Vector2 b, IDiagnosticHandler diagnostics)
        {
            switch (_dof)
            {
                case 2:
                    // Nothing is known yet, so we can just use this!
                    _p = p;
                    _b = b;

                    // We can already find a transform at this point. We just "invent"
                    // the second vector as a perpendicular item.
                    var a = new Matrix2(p.X, p.Y, p.Y, -p.X).Inverse * b;
                    Transform = new(a.X, a.Y, -a.Y, a.X);
                    _dof = 1;
                    break;

                case 1:
                    var pMatrix = new Matrix2(_p.X, p.X, _p.Y, p.Y);
                    var bMatrix = new Matrix2(_b.X, b.X, _b.Y, b.Y);

                    // If the pMatrix is not invertible
                    if (pMatrix.TryInvert(out var pInv))
                    {
                        var newOrientation = bMatrix * pInv;
                        if (!newOrientation.IsOrthonormal)
                        {
                            // We don't want non-orthonormal orientations...
                            diagnostics.Post(new DiagnosticMessage(SeverityLevel.Warning, "CE001", $"Could not constrain orientation of {Name}."));
                            return false;
                        }
                        Transform = newOrientation;
                    }
                    else
                    {
                        // We couldn't use the new orientation, but maybe we didn't have to use it...
                        if ((Transform * p - b).Equals(new Vector2()))
                            return true;
                        else
                        {
                            diagnostics.Post(new DiagnosticMessage(SeverityLevel.Warning, "CE001", $"Could not constrain orientation of {Name}."));
                            return false;
                        }
                    }
                    break;

                case 0:

                    // We are fully constrained, so only return true if it matches already
                    return (Transform * p - b).Equals(new Vector2());
            }
            return true;
        }

        /// <inheritdoc />
        public override void Render(SvgDrawing drawing)
        {
            drawing.StartGroup(Name, GetType().Name.ToLower());
            drawing.BeginTransform(new(Location, Transform));
            Draw(drawing);
            drawing.EndTransform();
            drawing.EndGroup();
        }

        /// <inheritdoc />
        public virtual Vector2 TransformNormal(Vector2 local) => Transform * local;

        /// <inheritdoc />
        public virtual Vector2 TransformOffset(Vector2 local) => Transform * local;
    }
}
