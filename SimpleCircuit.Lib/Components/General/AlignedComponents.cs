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
public class AlignedComponents : ILocatedPresence, IBoundedComponent
{
    private readonly List<ILocatedPresence> _presences = [];
    private readonly List<IBoundedComponent> _boundedPresences = [];
    private readonly VirtualChainConstraints _flags;
    private readonly string _filter;
    private bool _usedLeft, _usedTop, _usedRight, _usedBottom;

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
    public string Left
    {
        get
        {
            _usedLeft = true;

            // Make sure we also access all bounded presences
            foreach (var bounded in _boundedPresences)
                _ = bounded.Left;
            return $"{Name}.l";
        }
    }

    /// <inheritdoc />
    public string Top
    {
        get
        {
            _usedTop = true;

            // Make sure we also access all bounded presences
            foreach (var bounded in _boundedPresences)
                _ = bounded.Top;
            return $"{Name}.t";
        }
    }

    /// <inheritdoc />
    public string Right
    {
        get
        {
            _usedRight = true;

            // Make sure we also access all bounded presences
            foreach (var bounded in _boundedPresences)
                _ = bounded.Right;
            return $"{Name}.r";
        }
    }

    /// <inheritdoc />
    public string Bottom
    {
        get
        {
            _usedBottom = true;

            // Make sure we also access all bounded presences
            foreach (var bounded in _boundedPresences)
                _ = bounded.Bottom;
            return $"{Name}.b";
        }
    }

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
                _usedLeft = false;
                _usedTop = false;
                _usedRight = false;
                _usedBottom = false;
                _presences.Clear();
                _boundedPresences.Clear();
                foreach (var presence in context.FindFilter(_filter))
                {
                    if (presence is not ILocatedPresence located)
                    {
                        context.Diagnostics?.Post(Sources, ErrorCodes.ComponentCannotChangeLocation, presence.Name);
                        continue;
                    }
                    _presences.Add(located);
                    if (located is IBoundedComponent bounded)
                        _boundedPresences.Add(bounded);
                }
                if (_presences.Count == 0)
                {
                    context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotFindComponentMatching, _filter);
                    return PresenceResult.Success;
                }
                break;

            case PreparationMode.Offsets:
                if (_boundedPresences.Count == 0)
                {
                    context.Offsets.Group(Left, X, 0.0);
                    context.Offsets.Group(Top, Y, 0.0);
                    context.Offsets.Group(Right, X, 0.0);
                    context.Offsets.Group(Bottom, Y, 0.0);
                }
                else if (_boundedPresences.Count == 1)
                {
                    if (_usedLeft)
                        context.Offsets.Group(Left, _boundedPresences[0].Left, 0.0);
                    if (_usedTop)
                        context.Offsets.Group(Top, _boundedPresences[0].Top, 0.0);
                    if (_usedRight)
                        context.Offsets.Group(Right, _boundedPresences[0].Right, 0.0);
                    if (_usedBottom)
                        context.Offsets.Group(Bottom, _boundedPresences[0].Bottom, 0.0);
                }
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

            case PreparationMode.Groups:
                if (_boundedPresences.Count > 1)
                {
                    foreach (var bounded in _boundedPresences)
                    {
                        if (_usedLeft)
                            context.Group(bounded.Left, Left);
                        if (_usedTop)
                            context.Group(bounded.Top, Top);
                        if (_usedRight)
                            context.Group(bounded.Right, Right);
                        if (_usedBottom)
                            context.Group(bounded.Bottom, Bottom);
                    }
                }
                break;
        }
        return PresenceResult.Success;
    }

    /// <inheritdoc />
    public void Register(IRegisterContext context)
    {
        if (_boundedPresences.Count > 1)
        {
            // Define the bounded through solving
            var left = _usedLeft ? context.GetOffset(Left) : default;
            var top = _usedTop ? context.GetOffset(Top) : default;
            var right = _usedRight ? context.GetOffset(Right) : default;
            var bottom = _usedBottom ? context.GetOffset(Bottom) : default;

            int index = 0;
            foreach (var bounded in _boundedPresences)
            {
                index++;
                if (_usedLeft)
                {
                    if (!MinimumConstraint.AddMinimum(context.Circuit, $"{Name}.ml{index}", left, context.GetOffset(bounded.Left), 0.0, 1e-3))
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
                }
                if (_usedTop)
                {
                    if (!MinimumConstraint.AddMinimum(context.Circuit, $"{Name}.mt{index}", top, context.GetOffset(bounded.Top), 0.0, 1e-3))
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
                }
                if (_usedRight)
                {
                    if (!MinimumConstraint.AddMinimum(context.Circuit, $"{Name}.mr{index}", context.GetOffset(bounded.Right), right, 0.0, 1e-3))
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
                }
                if (_usedBottom)
                {
                    if (!MinimumConstraint.AddMinimum(context.Circuit, $"{Name}.mb{index}", context.GetOffset(bounded.Bottom), bottom, 0.0, 1e-3))
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotSatisfyMinimumDistance, Name);
                }
            }
        }
    }

    /// <inheritdoc />
    public void Update(IUpdateContext context)
    {
    }
}
