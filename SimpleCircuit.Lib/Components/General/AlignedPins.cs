using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Components.General;

/// <summary>
/// A presence that represents a set of aligned pins.
/// </summary>
public class AlignedPins : ILocatedPresence
{
    private readonly string _componentFilter, _pinFilter;
    private readonly List<IPin> _pins = [];
    private readonly VirtualChainConstraints _flags;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Vector2 Location => _pins[0].Location;

    /// <inheritdoc />
    public string X => _pins[0].X;

    /// <inheritdoc />
    public string Y => _pins[0].Y;

    /// <inheritdoc />
    public List<TextLocation> Sources { get; } = [];

    /// <inheritdoc />
    public int Order => 50;

    /// <summary>
    /// Creates a new set of aligned pins.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="componentFilter">The component filter.</param>
    /// <param name="pinFilter">The pin filter.</param>
    /// <param name="flags">The flags.</param>
    public AlignedPins(string name, string componentFilter, string pinFilter, VirtualChainConstraints flags)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"Invalid {nameof(name)}");
        if (string.IsNullOrWhiteSpace(componentFilter))
            throw new ArgumentException($"Invalid {nameof(componentFilter)}");
        if (string.IsNullOrWhiteSpace(pinFilter))
            throw new ArgumentException($"Invalid {nameof(pinFilter)}");
        Name = name;
        _componentFilter = componentFilter;
        _pinFilter = pinFilter;
        _flags = flags;
    }

    /// <inheritdoc />
    public PresenceResult Prepare(IPrepareContext context)
    {
        switch (context.Mode)
        {
            case PreparationMode.Reset:
                _pins.Clear();
                break;

            case PreparationMode.Find:
                if (_pins.Count == 0)
                {
                    var regex = new Regex(_pinFilter);

                    // Find the components that match the filter
                    bool foundComponent = false;
                    foreach (var component in context.FindFilter(_componentFilter))
                    {
                        foundComponent = true;
                        bool foundPin = false;
                        if (component is IDrawable drawable)
                        {
                            foreach (var pin in drawable.Pins)
                            {
                                if (regex.IsMatch(pin.Name))
                                {
                                    _pins.Add(pin);
                                    foundPin = true;
                                }
                            }

                            // We need to at least find the pin, but in some cases the pin may not have been created yet
                            // So we'll mark it as incomplete if we don't find the pin to try again later
                            if (!foundPin)
                            {
                                if (context.Desparateness == DesperatenessLevel.GiveUp)
                                {
                                    context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotFindMatchingPinOnComponent, _pinFilter, drawable.Name);
                                    return PresenceResult.Success;
                                }
                                return PresenceResult.Incomplete;
                            }
                        }
                        else
                            context.Diagnostics?.Post(Sources, ErrorCodes.ComponentDoesNotHaveAnyPins, component.Name);
                    }
                    if (!foundComponent)
                    {
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotFindComponentMatching, _componentFilter);
                        return PresenceResult.GiveUp;
                    }
                    if (_pins.Count == 0)
                    {
                        context.Diagnostics?.Post(Sources, ErrorCodes.CouldNotFindAnyPinsMatching, _pinFilter, _componentFilter);
                        return PresenceResult.GiveUp;
                    }
                }
                break;

            case PreparationMode.Offsets:
                if (_pins.Count < 2)
                    return PresenceResult.Success;

                // Align components along the axis necessary
                ILocatedPresence last = null;
                foreach (var presence in _pins)
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
