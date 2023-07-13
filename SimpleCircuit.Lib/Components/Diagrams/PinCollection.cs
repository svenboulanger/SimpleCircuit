using SimpleCircuit.Components.Pins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Diagrams
{
    public class PinCollection : IPinCollection
    {
        private int _count = 0;
        private readonly ILocatedDrawable _parent;
        private readonly List<LooselyOrientedPin> _orderedPins = new();
        private readonly Dictionary<string, LooselyOrientedPin> _pinsByName = new();

        /// <inheritdoc />
        public IPin this[string name]
        {
            get
            {
                if (!_pinsByName.TryGetValue(name, out var pin))
                {
                    pin = new LooselyOrientedPin(name, name, _parent);
                    _pinsByName.Add(name, pin);
                    _orderedPins.Add(pin);
                }
                return pin;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// There is no first/last pin, all pins are separate for ERD.
        /// This allows us to decide on which side the pin has to be.
        /// </remarks>
        public IPin this[int index]
        {
            get
            {
                string name = $"#{_count++}";
                var pin = new LooselyOrientedPin(name, name, _parent);
                _pinsByName.Add(name, pin);
                _orderedPins.Add(pin);
                return pin;
            }
        }

        /// <inheritdoc />
        public int Count => _pinsByName.Count;

        /// <summary>
        /// Creates a new pin collection.
        /// </summary>
        /// <param name="parent">The parent drawable.</param>
        public PinCollection(ILocatedDrawable parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <inheritdoc />
        public IEnumerable<string> NamesOf(IPin pin)
        {
            yield return pin.Name;
        }

        /// <inheritdoc />
        public bool TryGetValue(string name, out IPin pin)
        {
            if (_pinsByName.TryGetValue(name, out var p))
            {
                pin = p;
                return true;
            }

            // Didn't exist yet
            var np = new LooselyOrientedPin(name, name, _parent);
            _pinsByName.Add(name, np);
            _orderedPins.Add(np);
            pin = np;
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _orderedPins.Clear();
            _pinsByName.Clear();
            _count = 0;
        }

        /// <summary>
        /// Sort the pins by their orientation.
        /// </summary>
        public void SortClockwise()
        {
            _orderedPins.Sort((x, y) =>
            {
                double a = Math.Atan2(x.Orientation.Y, x.Orientation.X);
                double b = Math.Atan2(y.Orientation.Y, y.Orientation.X);
                if (a < b - 0.001)
                    return -1;
                else if (a > b + 0.001)
                    return 1;
                else
                    return 0;
            });
        }

        /// <inheritdoc />
        public IEnumerator<IPin> GetEnumerator()
            => _orderedPins.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => _orderedPins.GetEnumerator();
    }
}
