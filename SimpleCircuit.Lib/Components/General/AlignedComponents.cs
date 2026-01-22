using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.General;

/// <summary>
/// A presence that represents a set of aligned components.
/// </summary>
public class AlignedComponents : ILocatedPresence
{
    private readonly List<ILocatedPresence> _presences = [];
    private readonly VirtualChainConstraints _flags;
    private readonly string _filter;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public List<TextLocation> Sources { get; } = [];

    /// <inheritdoc />
    public int Order => 50;

    /// <inheritdoc />
    public string X => _presences[0].X;

    /// <inheritdoc />
    public string Y => _presences[0].Y;

    /// <inheritdoc />
    public Vector2 Location => _presences[0].Location;

    /// <summary>
    /// Creates a new <see cref="AlignedComponents"/>.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="flag">The flag.</param>
    public AlignedComponents(string name, string filter, VirtualChainConstraints flag)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"Invalid {nameof(name)}");
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException($"Invalid {nameof(filter)}");
        Name = name;
        _filter = filter;
        _flags = flag;
    }

    /// <inheritdoc />
    public PresenceResult Prepare(IPrepareContext context)
    {
        switch (context.Mode)
        {
            case PreparationMode.Reset:
                _presences.Clear();
                foreach (var presence in context.FindFilter(_filter))
                {
                    if (presence is not ILocatedPresence located)
                    {
                        context.Diagnostics?.Post(Sources, ErrorCodes.ComponentCannotChangeLocation, presence.Name);
                        continue;
                    }
                    _presences.Add(located);
                }
                if (_presences.Count == 0)
                {
                    context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotFindComponentMatching, _filter);
                    return PresenceResult.Success;
                }
                break;

            case PreparationMode.Offsets:
                if (_presences.Count < 2)
                    return PresenceResult.Success;

                // Align components along the axis necessary
                ILocatedPresence last = null;
                foreach (var presence in _presences)
                {
                    if (last is not null)
                    {
                        if ((_flags & VirtualChainConstraints.X) != 0)
                        {
                            if (!context.Offsets.Group(last.X, presence.X, 0.0))
                            {
                                context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongX, last.X, presence.X);
                                return PresenceResult.GiveUp;
                            }
                        }
                        if ((_flags & VirtualChainConstraints.Y) != 0)
                        {
                            if (!context.Offsets.Group(last.Y, presence.Y, 0.0))
                            {
                                context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongY, last.Y, presence.Y);
                                return PresenceResult.GiveUp;
                            }
                        }
                    }
                    last = presence;
                }
                break;
        }
        return PresenceResult.Success;
    }

    /// <inheritdoc />
    public void Register(IRegisterContext context)
    {
    }

    /// <inheritdoc />
    public void Update(IUpdateContext context)
    {
    }
}
