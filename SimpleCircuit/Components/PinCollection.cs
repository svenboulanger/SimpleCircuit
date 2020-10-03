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
    public class PinCollection : IEnumerable<Pin>
    {
        private readonly IComponent _parent;
        private readonly Dictionary<string, Pin> _pins;
        private readonly List<Pin> _ordered = new List<Pin>();

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
            _pins = new Dictionary<string, Pin>(comparer);
        }

        /// <summary>
        /// Adds the specified pin.
        /// </summary>
        /// <param name="names">The names of the pin.</param>
        /// <param name="description">The node description.</param>
        /// <param name="offset">The offset of the pin.</param>
        /// <param name="normal">The normal of the pin.</param>
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

            var pin = new Pin(description, x, y, nx, ny);

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
        public IEnumerator<Pin> GetEnumerator() => _ordered.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the <see cref="Pin"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="Pin"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>The pin.</returns>
        public Pin this[string name]
        {
            get
            {
                if (_pins.TryGetValue(name, out var pin))
                    return pin;
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Pin"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="Pin"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The pin.</returns>
        public Pin this[int index] => _ordered[index];

        /// <summary>
        /// Gets the names of the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <returns>The pins.</returns>
        public IEnumerable<string> NamesOf(Pin pin) => _pins.Where(p => ReferenceEquals(p.Value, pin)).Select(p => p.Key);
    }
}
