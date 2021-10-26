using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A collection of pins.
    /// </summary>
    /// <seealso cref="IEnumerable{Pin}" />
    public class PinCollection : IPinCollection
    {
        private class Node
        {
            public IPin Pin;
            public bool Used;
        }

        private readonly IComponent _parent;
        private readonly Dictionary<string, Node> _pins;
        private readonly List<Node> _ordered = new List<Node>();

        /// <summary>
        /// Gets the number of pins.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _ordered.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="PinCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="comparer">The comparer.</param>
        public PinCollection(IComponent parent, IEqualityComparer<string> comparer = null)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _pins = new Dictionary<string, Node>(comparer);
        }

        /// <summary>
        /// Adds a pin with the specified names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="description">The description.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="normal">The normal.</param>
        public void Add(string[] names, string description, Vector2 offset, Vector2 normal)
        {
            // Use the component orientation to transform the offset and normal
            Function x = 0.0;
            Function y = 0.0;
            if (_parent is ITranslating pos)
            {
                x = pos.X;
                y = pos.Y;
            }
            Function nx = normal.X;
            Function ny = normal.Y;
            if (_parent is IRotating or)
            {
                var ms = _parent is IScaling m ? m.Scale : 1.0;
                x += offset.X * or.NormalX - offset.Y * ms * or.NormalY;
                y += offset.X * or.NormalY + offset.Y * ms * or.NormalX;
                nx = normal.X * or.NormalX - normal.Y * ms * or.NormalY;
                ny = normal.X * or.NormalY + normal.Y * ms * or.NormalX;
            }
            else
            {
                var ms = _parent is IScaling m ? m.Scale : 1.0;
                x += offset.X;
                y += offset.Y * ms;
            }

            var pin = new Node
            {
                Pin = new RotatingPin(names[0], description, _parent, x, y, nx, ny),
                Used = false
            };
            _ordered.Add(pin);
            foreach (var name in names)
                _pins[name] = pin;
        }

        /// <summary>
        /// Adds a pin with the specified names. The pin does not have a specific orientation and defaults to (1, 0) but
        /// cannot be rotated.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="description">The description.</param>
        /// <param name="offset">The offset.</param>
        public void Add(string[] names, string description, Vector2 offset)
        {
            // Use the component orientation to transform the offset and normal
            Function x = 0.0;
            Function y = 0.0;
            if (_parent is ITranslating pos)
            {
                x = pos.X;
                y = pos.Y;
            }
            if (_parent is IRotating or)
            {
                var ms = _parent is IScaling m ? m.Scale : 1.0;
                x += offset.X * or.NormalX - offset.Y * ms * or.NormalY;
                y += offset.X * or.NormalY + offset.Y * ms * or.NormalX;
            }
            else
            {
                x += offset.X;
                y += offset.Y;
            }

            var pin = new Node
            {
                Pin = new TranslatingPin(names[0], description, _parent, x, y),
                Used = false
            };
            _ordered.Add(pin);
            foreach (var name in names)
                _pins[name] = pin;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IPin> GetEnumerator() => _ordered.Select(o => o.Pin).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether the pin with the specified name is used.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified name is used; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUsed(string name)
        {
            if (_pins.TryGetValue(name, out var pin))
                return pin.Used;
            return false;
        }

        /// <summary>
        /// Gets the <see cref="RotatingPin"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="RotatingPin"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IPin this[string name]
        {
            get
            {
                if (_pins.TryGetValue(name, out var pin))
                {
                    pin.Used = true;
                    return pin.Pin;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="RotatingPin"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="RotatingPin"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The pin.</returns>
        public IPin this[int index]
        {
            get
            {
                var pin = _ordered[index];
                pin.Used = true;
                return pin.Pin;
            }
        }

        /// <summary>
        /// Gets the names of a specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <returns>The names.</returns>
        public IEnumerable<string> NamesOf(IPin pin) => _pins.Where(p => ReferenceEquals(p.Value.Pin, pin)).Select(p => p.Key);
    }
}
