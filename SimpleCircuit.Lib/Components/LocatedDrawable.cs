using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;

namespace SimpleCircuit.Components;

/// <summary>
/// A generic implementation of a component.
/// </summary>
public abstract class LocatedDrawable : Drawable, ILocatedDrawable
{
    /// <inheritdoc />
    public string X { get; }

    /// <inheritdoc />
    public string Y { get; }

    /// <inheritdoc />
    public Vector2 Location { get; private set; }

    /// <inheritdoc />
    protected override Transform CreateTransform() => new(Location, Matrix2.Identity);

    /// <summary>
    /// Initializes a new instance of the <see cref="LocatedDrawable"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="pinCollection">An optional pin collection.</param>
    protected LocatedDrawable(string name, IPinCollection pinCollection = null)
        : base(name, pinCollection)
    {
        X = $"{name}.x";
        Y = $"{name}.y";
    }

    /// <inheritdoc />
    public override void Update(IUpdateContext context)
    {
        Location = context.GetValue(X, Y);

        // Give the pins a chance to update as well
        foreach (var pin in Pins)
            pin.Update(context);
    }

    /// <inheritdoc />
    public override PresenceResult Prepare(IPrepareContext context)
    {
        var result = base.Prepare(context);
        if (result == PresenceResult.GiveUp)
            return result;

        // Deal with the pins
        foreach (var pin in Pins)
        {
            var r = pin.Prepare(context);
            if (r == PresenceResult.GiveUp)
                return PresenceResult.GiveUp;
            else if (r == PresenceResult.Incomplete)
                result = PresenceResult.Incomplete;
        }

        switch (context.Mode)
        {
            case PreparationMode.Reset:
                Location = new();
                break;

            case PreparationMode.Offsets:

                // Register the origin of the drawable
                context.Offsets.Add(X);
                context.Offsets.Add(Y);
                break;

            case PreparationMode.DrawableGroups:
                context.GroupDrawableTo(this, X, Y);
                break;
        }
        return result;
    }

    /// <inheritdoc />
    public override void Register(IRegisterContext context)
    {
        foreach (var pin in Pins)
            pin.Register(context);
    }
}
