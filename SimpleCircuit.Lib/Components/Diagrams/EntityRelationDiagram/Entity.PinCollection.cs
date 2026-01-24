using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram;

public partial class Entity
{
    /// <summary>
    /// Creates a new <see cref="PinCollection"/>.
    /// </summary>
    /// <param name="parent">The parent component.</param>
    private class PinCollection(Instance parent) : IPinCollection
    {
        private readonly Instance _parent = parent;
        private readonly Dictionary<string, IPin> _pinsByName = [];
        private readonly List<IPin> _pinsByIndex = [];
        private readonly List<(LoosePin, Orientation)> _pinOrientations = [];
        private int _anonymousIndex = 0;

        /// <summary>
        /// The possible orientations for a pin.
        /// </summary>
        private enum Orientation
        {
            Left,
            Up,
            Right,
            Down
        }

        public const double MinimumWeight = 100.0;

        /// <inheritdoc/>
        public IPin this[string name]
        {
            get
            {
                if (!_pinsByName.TryGetValue(name, out var pin))
                {
                    pin = new LoosePin(name, name, _parent);
                    _pinsByName.Add(name, pin);
                    _pinsByIndex.Add(pin);

                    // Try to add an alias if possible
                    int separator = name.IndexOf('_');
                    if (separator > 0)
                    {
                        string alias = name.Substring(0, separator);
                        if (!_pinsByName.ContainsKey(alias))
                            _pinsByName.Add(alias, pin);
                    }
                }
                return pin;
            }
        }

        /// <inheritdoc/>
        public IPin this[int index]
        {
            get
            {
                if (index == 0 || index == _pinsByIndex.Count - 1)
                {
                    // If we asked the last or first pin, let's create a new one
                    _anonymousIndex++;
                    string name = $"[ap{_anonymousIndex}]_";
                    var pin = new LoosePin(name, name, _parent);
                    _pinsByIndex.Add(pin);
                    return pin;
                }
                return _pinsByIndex[index];
            }
        }

        /// <summary>
        /// Gets the number of anonymous pins on the left side.
        /// </summary>
        public int LeftCount { get; private set; }

        /// <summary>
        /// Gets the number of anonymous pins on the top side.
        /// </summary>
        public int TopCount { get; private set; }

        /// <summary>
        /// Gets the number of anonymous pins on the right side.
        /// </summary>
        public int RightCount { get; private set; }

        /// <summary>
        /// Gets the number of anonymous pins on the bottom side.
        /// </summary>
        public int BottomCount { get; private set; }

        /// <inheritdoc/>
        public int Count => _pinsByIndex.Count;

        /// <inheritdoc/>
        public IEnumerable<string> NamesOf(IPin pin)
        {
            yield return pin.Name;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context)
        {
        }

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            var pins = _pinsByIndex.OfType<LoosePin>();
            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    _pinOrientations.Clear();
                    LeftCount = 0;
                    TopCount = 0;
                    RightCount = 0;
                    BottomCount = 0;
                    break;

                case PreparationMode.Sizes:
                    // The orientations have just finished, let's track the pin orientations
                    foreach (var anyPin in _pinsByIndex)
                    {
                        // Only deal with loose pins
                        if (anyPin is not LoosePin pin)
                            continue;

                        if (Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y < 0)
                        {
                            _pinOrientations.Add((pin, Orientation.Up));
                            TopCount++;
                        }
                        else if (Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y > 0)
                        {
                            _pinOrientations.Add((pin, Orientation.Down));
                            BottomCount++;
                        }
                        else if (Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X < 0)
                        {
                            _pinOrientations.Add((pin, Orientation.Left));
                            LeftCount++;
                        }
                        else if (Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X > 0)
                        {
                            _pinOrientations.Add((pin, Orientation.Right));
                            RightCount++;
                        }
                        else
                        {
                            context.Diagnostics?.Post(pin.Sources, ErrorCodes.InvalidBlackBoxPinDirection, _parent.Name);
                            return PresenceResult.GiveUp;
                        }
                    }
                    break;

                case PreparationMode.Offsets:

                    // Distribute the pins evenly along the side of the entity
                    var bounds = _parent.RelativeBounds;
                    if (_parent.CornerRadius * 2.0 > bounds.Width && _parent.CornerRadius * 2.0 > bounds.Height)
                        bounds = bounds.Expand(-_parent.CornerRadius);
                    int iLeft = 0, iTop = 0, iRight = 0, iBottom = 0;
                    for (int i = 0; i < _pinOrientations.Count; i++)
                    {
                        var (pin, orientation) = _pinOrientations[i];
                        switch (orientation)
                        {
                            case Orientation.Up:
                                if (!context.Offsets.Group(_parent.Y, pin.Y, bounds.Top))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongY, _parent.Y, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                double f = (iTop + 0.5) / TopCount;
                                context.Offsets.Group(_parent.X, pin.X, (1.0 - f) * bounds.Left + f * bounds.Right);
                                iTop++;
                                break;

                            case Orientation.Down:
                                if (!context.Offsets.Group(_parent.Y, pin.Y, bounds.Bottom))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongY, _parent.Y, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                f = (iBottom + 0.5) / BottomCount;
                                context.Offsets.Group(_parent.X, pin.X, (1.0 - f) * bounds.Left + f * bounds.Right);
                                iBottom++;
                                break;

                            case Orientation.Left:
                                if (!context.Offsets.Group(_parent.X, pin.X, bounds.Left))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongX, _parent.X, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                f = (iLeft + 0.5) / LeftCount;
                                context.Offsets.Group(_parent.Y, pin.Y, (1.0 - f) * bounds.Top + f * bounds.Bottom);
                                iLeft++;
                                break;

                            case Orientation.Right:
                                if (!context.Offsets.Group(_parent.X, pin.X, bounds.Right))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongX, _parent.Name, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                f = (iRight + 0.5) / RightCount;
                                context.Offsets.Group(_parent.Y, pin.Y, (1.0 - f) * bounds.Top + f * bounds.Bottom);
                                iRight++;
                                break;

                            default:
                                return PresenceResult.GiveUp;
                        }
                    }
                    break;
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public IEnumerator<IPin> GetEnumerator() => _pinsByIndex.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Sets a pin offset of a defined pin.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="NotImplementedException">Thrown if the pin is not a <see cref="FixedOrientedPin"/>.</exception>
        public void SetPinOffset(int index, Vector2 offset)
        {
            if (_pinsByIndex[index] is FixedOrientedPin pin)
                pin.Offset = offset;
            else
                throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryGetValue(string name, out IPin pin)
        {
            pin = this[name];
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _pinsByIndex.Clear();
            _pinsByName.Clear();
        }

        /// <summary>
        /// Adds a pin to the collection.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="names">The names.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pin"/> is <c>null</c>.</exception>
        public void Add(IPin pin, params string[] names)
        {
            _pinsByIndex.Add(pin ?? throw new ArgumentNullException(nameof(pin)));
            foreach (string name in names)
                _pinsByName.Add(name, pin);
        }
    }
}
