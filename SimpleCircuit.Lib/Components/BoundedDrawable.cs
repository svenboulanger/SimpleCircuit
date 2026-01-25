using SimpleCircuit.Circuits;
using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components;

/// <summary>
/// A drawable that is bounded in size.
/// </summary>
public abstract class BoundedDrawable : LocatedDrawable, IBoundedComponent
{
    /// <summary>
    /// Gets whether the bounds have been used.
    /// </summary>
    protected bool UsedBounds { get; private set; } = false;

    /// <inheritdoc />
    public string Left
    {
        get
        {
            UsedBounds = true;
            return $"{Name}.l";
        }
    }

    /// <inheritdoc />
    public string Top
    {
        get
        {
            UsedBounds = true;
            return $"{Name}.t";
        }
    }

    /// <inheritdoc />
    public string Right
    {
        get
        {
            UsedBounds = true;
            return $"{Name}.r";
        }
    }

    /// <inheritdoc />
    public string Bottom
    {
        get
        {
            UsedBounds = true;
            return $"{Name}.b";
        }
    }

    /// <summary>
    /// Creates a new <see cref="BoundedDrawable"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="pinCollection">An optional pin collection.</param>
    protected BoundedDrawable(string name, IPinCollection pinCollection = null)
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
            case PreparationMode.Offsets:
                var r = RegisterBoundOffsets(context);
                if (r == PresenceResult.GiveUp)
                    return PresenceResult.GiveUp;
                else if (r == PresenceResult.Incomplete)
                    result = PresenceResult.Incomplete;
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
        if (UsedBounds)
        {
            // Simply draw it and figure out the bounds from there
            var builder = new BoundsBuilder(context.TextFormatter, context.Style, context.Diagnostics);
            builder.BeginBounds();
            builder.BeginTransform(CreateTransform());
            Draw(builder);
            builder.EndTransform();
            builder.EndBounds(out var bounds);

            if (!context.Offsets.Group(X, Left, bounds.Left) ||
                !context.Offsets.Group(Y, Top, bounds.Top) ||
                !context.Offsets.Group(X, Right, bounds.Right) ||
                !context.Offsets.Group(Y, Bottom, bounds.Bottom))
                return PresenceResult.GiveUp;
        }
        return PresenceResult.Success;
    }
}
