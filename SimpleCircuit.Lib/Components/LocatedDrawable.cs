using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components;

/// <summary>
/// A generic implementation of a component.
/// </summary>
public abstract class LocatedDrawable : Drawable, ILocatedDrawable
{
    private bool _usedBounds;

    /// <inheritdoc />
    public string X { get; }

    /// <inheritdoc />
    public string Y { get; }

    /// <inheritdoc />
    public string Left
    {
        get
        {
            _usedBounds = true;
            return $"{Name}.l";
        }
    }

    /// <inheritdoc />
    public string Top
    {
        get
        {
            _usedBounds = true;
            return $"{Name}.t";
        }
    }

    /// <inheritdoc />
    public string Right
    {
        get
        {
            _usedBounds = true;
            return $"{Name}.r";
        }
    }

    /// <inheritdoc />
    public string Bottom
    {
        get
        {
            _usedBounds = true;
            return $"{Name}.b";
        }
    }

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
                var r = RegisterBoundOffsets(context);
                if (r == PresenceResult.GiveUp)
                    return PresenceResult.GiveUp;
                else if (r == PresenceResult.Incomplete)
                    result = PresenceResult.Incomplete;

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

    /// <summary>
    /// Register the offsets that will define the bounds of the drawable.
    /// The <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> and <see cref="Bottom"/>
    /// coordinates are linked here.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Returns the result.</returns>
    protected virtual PresenceResult RegisterBoundOffsets(IPrepareContext context)
    {
        if (_usedBounds)
        {
            // Simply draw it and figure out the bounds from there
            var builder = new BoundsBuilder(context.TextFormatter, context.Style, context.Diagnostics);
            builder.BeginBounds();
            builder.BeginTransform(CreateTransform());
            Draw(builder);
            builder.EndTransform();
            builder.EndBounds(out var bounds);

            context.Offsets.Group(X, Left, bounds.Left);
            context.Offsets.Group(Y, Top, bounds.Top);
            context.Offsets.Group(X, Right, bounds.Right);
            context.Offsets.Group(Y, Bottom, bounds.Bottom);
        }
        return PresenceResult.Success;
    }

    /// <inheritdoc />
    public override void Register(IRegisterContext context)
    {
        foreach (var pin in Pins)
            pin.Register(context);
    }
}
