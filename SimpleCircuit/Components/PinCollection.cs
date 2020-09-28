using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    public class PinCollection : IEnumerable<Pin>
    {
        private readonly IComponent _parent;
        private readonly Dictionary<string, Pin> _pins;
        private readonly List<Pin> _ordered = new List<Pin>();

        public int Count => _ordered.Count;

        public PinCollection(IComponent parent, IEqualityComparer<string> comparer = null)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _pins = new Dictionary<string, Pin>(comparer);
        }

        /// <summary>
        /// Adds the specified pin.
        /// </summary>
        /// <param name="names">The names of the pin.</param>
        /// <param name="offset">The offset of the pin.</param>
        /// <param name="normal">The normal of the pin.</param>
        public void Add(string[] names, Vector2 offset, Vector2 normal)
        {
            // Use the component orientation to transform the offset and normal
            var x = _parent.X + offset.X * _parent.NormalX - offset.Y * _parent.MirrorScale * _parent.NormalY;
            var y = _parent.Y + offset.X * _parent.NormalY + offset.Y * _parent.MirrorScale * _parent.NormalX;
            var nx = normal.X * _parent.NormalX - normal.Y * _parent.MirrorScale * _parent.NormalY;
            var ny = normal.X * _parent.NormalY + normal.Y * _parent.MirrorScale * _parent.NormalX;
            var pin = new Pin(x, y, nx, ny);

            _ordered.Add(pin);
            foreach (var name in names)
                _pins[name] = pin;
        }

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

        public Pin this[int index] => _ordered[index];
    }
}
