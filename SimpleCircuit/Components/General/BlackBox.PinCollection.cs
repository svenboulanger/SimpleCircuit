using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
    public partial class BlackBox
    {
        private class PinCollection : IPinCollection
        {
            private readonly BlackBox _parent;
            private readonly Dictionary<string, Pin> _pins = new Dictionary<string, Pin>();

            /// <summary>
            /// Gets the last pin on the north side.
            /// </summary>
            public Pin North { get; private set; }

            /// <summary>
            /// Gets the last pin on the south side.
            /// </summary>
            public Pin South { get; private set; }

            /// <summary>
            /// Gets the last pin on the west side.
            /// </summary>
            public Pin West { get; private set; }

            /// <summary>
            /// Gets the last pin on the east side.
            /// </summary>
            public Pin East { get; private set; }

            /// <inheritdoc/>
            public IPin this[string name]
            {
                get
                {
                    if (_pins.TryGetValue(name, out var pin))
                        return pin;

                    // It doesn't exist yet, so let's create it!
                    Pin newPin;
                    switch (name[0])
                    {
                        case 'N':
                        case 'n':
                            newPin = new Pin(name.Substring(1), _parent, new Vector2(0, -1))
                            {
                                Previous = North
                            };
                            newPin.X = (North != null ? North.X : _parent.X) + newPin.Length;
                            newPin.Y = _parent.Y;
                            North = newPin;
                            break;

                        case 'S':
                        case 's':
                            newPin = new Pin(name.Substring(1), _parent, new Vector2(0, 1))
                            {
                                Previous = South
                            };
                            newPin.X = (South != null ? South.X : _parent.X) + newPin.Length;
                            newPin.Y = _parent.Y + _parent.Height;
                            South = newPin;
                            break;

                        case 'E':
                        case 'e':
                            newPin = new Pin(name.Substring(1), _parent, new Vector2(1, 0))
                            {
                                Previous = East
                            };
                            newPin.X = _parent.X + _parent.Width;
                            newPin.Y = (East != null ? East.Y : _parent.Y) + newPin.Length;
                            East = newPin;
                            break;

                        case 'W':
                        case 'w':
                        default:
                            newPin = new Pin(name.Substring(1), _parent, new Vector2(-1, 0))
                            {
                                Previous = West
                            };
                            newPin.X = _parent.X;
                            newPin.Y = (West != null ? West.Y : _parent.Y) + newPin.Length;
                            West = newPin;
                            break;
                    }
                    _pins.Add(name, newPin);
                    return newPin;
                }
            }

            /// <inheritdoc/>
            public IPin this[int index] => _pins.Values.ElementAt(index);

            /// <inheritdoc/>
            public int Count => _pins.Count;

            /// <summary>
            /// Creates a new <see cref="PinCollection"/>.
            /// </summary>
            /// <param name="parent">the parent component.</param>
            public PinCollection(BlackBox parent)
            {
                _parent = parent;
            }

            /// <inheritdoc/>
            public IEnumerator<IPin> GetEnumerator() => _pins.Values.GetEnumerator();

            /// <inheritdoc/>
            public IEnumerable<string> NamesOf(IPin pin)
            {
                if (pin is Pin p)
                    yield return p.Name;
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
