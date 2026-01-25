using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;

namespace SimpleCircuit.Components;

/// <summary>
/// Default implementation for a component that has a location and an orientation.
/// </summary>
public abstract class OrientedDrawable : BoundedDrawable, IOrientedDrawable
{
    private int _dof = 2;
    private Vector2 _p, _b;

    /// <summary>
    /// Gets or sets a flag that flips the drawable if the item is not fully constrained.
    /// </summary>
    [Description("Flips the element if possible.")]
    public bool Flipped
    {
        get => Variants.Contains("flip");
        set
        {
            if (value)
                Variants.Add("flip");
            else
                Variants.Remove("flip");
            UpdateTransform();
        }
    }

    /// <inheritdoc />
    public int OrientationDegreesOfFreedom => _dof;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrientedDrawable"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="pinCollection">An optional pin collection.</param>
    protected OrientedDrawable(string name, IPinCollection pinCollection = null)
        : base(name, pinCollection)
    {
    }

    /// <inheritdoc />
    public override PresenceResult Prepare(IPrepareContext context)
    {
        var result = base.Prepare(context);
        if (result == PresenceResult.GiveUp)
            return result;

        switch (context.Mode)
        {
            case PreparationMode.Reset:
                _dof = 2;
                UpdateTransform();
                break;
        }
        return result;
    }

    /// <inheritdoc />
    protected override Transform CreateTransform() => new(Location, Transform);

    /// <inheritdoc />
    public Matrix2 Transform { get; set; } = Matrix2.Identity;

    /// <inheritdoc />
    public bool IsConstrained(Vector2 p)
    {
        switch (_dof)
        {
            case 0: return true;
            case 1:
                // If the relative orientation is a scalar of the already specified _p, it is constrained
                if ((_p.X * p.Y - _p.Y * p.X).IsZero())
                    return true;
                return false;
            case 2: return false;
            default:
                return true;
        }
    }

    /// <inheritdoc />
    public bool ConstrainOrientation(Vector2 p, Vector2 b, TextLocation source, IDiagnosticHandler diagnostics)
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
                        diagnostics.Post(source, ErrorCodes.CouldNotConstrainOrientation, Name);
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
                        diagnostics.Post(source, ErrorCodes.CouldNotConstrainOrientation, Name);
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
            case 2:
                // Just whatever
                Transform = Flipped ? (new(-1, 0, 0, -1)) : Matrix2.Identity;
                break;

            case 1:
                // We already have one axis, we want to flip around that instead
                Vector2 altp = new(_p.Y, -_p.X);
                Vector2 altb = new(_b.Y, -_b.X);
                if (Flipped)
                    altp = -altp;
                Transform = new Matrix2(_b.X, altb.X, _b.Y, altb.Y) * new Matrix2(_p.X, altp.X, _p.Y, altp.Y).Inverse;
                break;

            case 0:
                // Fully constrained, don't do anything
                break;
        }
    }

    /// <inheritdoc />
    public virtual Vector2 TransformNormal(Vector2 local) => Transform * local;

    /// <inheritdoc />
    public virtual Vector2 TransformOffset(Vector2 local) => Transform * local;
}
