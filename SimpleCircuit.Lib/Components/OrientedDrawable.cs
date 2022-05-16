using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Default implementation for a component that has a location and an orientation.
    /// </summary>
    public abstract class OrientedDrawable : LocatedDrawable, IOrientedDrawable
    {
        private int _dof = 2;
        private bool _flipped = false;
        private Vector2 _p, _b;

        /// <summary>
        /// Gets or sets a flag that flips the drawable if the item is not fully constrained.
        /// </summary>
        [Description("Flips the element if possible.")]
        public bool Flipped
        {
            get => _flipped;
            set
            {
                _flipped = value;
                UpdateTransform();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrientedDrawable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="flipAxis">The relevant coordinates for flipping.</param>
        protected OrientedDrawable(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        public override void AddVariant(string variant)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(variant, "flip"))
                Flipped = true;
            base.AddVariant(variant);
        }

        /// <inheritdoc />
        public override void RemoveVariant(string variant)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(variant, "flip"))
                Flipped = false;
            base.RemoveVariant(variant);
        }

        /// <inheritdoc />
        public override bool HasVariant(string variant)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(variant, "flip"))
                return Flipped;
            return base.HasVariant(variant);
        }

        /// <inheritdoc />
        public override void CollectPossibleVariants(ISet<string> variants)
        {
            base.CollectPossibleVariants(variants);
            variants.Add("flip");
        }

        /// <inheritdoc />
        protected override Transform CreateTransform() => new(Location, Transform);

        /// <inheritdoc />
        public Matrix2 Transform { get; set; } = Matrix2.Identity;

        /// <inheritdoc />
        public bool ConstrainOrientation(Vector2 p, Vector2 b, IDiagnosticHandler diagnostics)
        {
            switch (_dof)
            {
                case 2:
                    // Nothing is known yet, so we can just use use it!
                    _p = p;
                    _b = b;
                    _dof = 1;
                    UpdateTransform();
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

        /// <summary>
        /// Updates the transform if possible with new information.
        /// </summary>
        private void UpdateTransform()
        {
            switch (_dof)
            {
                case 0:
                    // Just whatever
                    Transform = _flipped ? (new(-1, 0, 0, -1)) : Matrix2.Identity;
                    break;

                case 1:
                    // We already have one axis, we want to flip around that instead
                    Vector2 altp = new(_p.Y, -_p.X);
                    Vector2 altb = new(_b.Y, -_b.X);
                    if (_flipped)
                        altp = -altp;
                    Transform = new Matrix2(_b.X, altb.X, _b.Y, altb.Y) * new Matrix2(_p.X, altp.X, _p.Y, altp.Y).Inverse;
                    break;

                case 2:
                    // Fully constrained, don't do anything
                    break;
            }
        }

        /// <inheritdoc />
        public virtual Vector2 TransformNormal(Vector2 local) => Transform * local;

        /// <inheritdoc />
        public virtual Vector2 TransformOffset(Vector2 local) => Transform * local;
    }
}
