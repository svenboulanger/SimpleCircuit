using SimpleCircuit.Contributors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A pin that is fixed to a parent component.
    /// </summary>
    /// <seealso cref="IPin" />
    public class Pin : IPin
    {
        private readonly string[] _names;
        private readonly double _angle;
        private readonly Contributor _sx, _sy, _a;

        /// <inheritdoc/>
        public IComponent Parent { get; }

        /// <inheritdoc/>
        public Contributor X { get; }

        /// <inheritdoc/>
        public Contributor Y { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pin2D"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="relative">The relative.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="names">The names.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> or <paramref name="names"/> is <c>null</c>.</exception>
        public Pin(IComponent parent, Contributor cx, Contributor cy, Contributor sx, Contributor sy, Contributor a, Vector2 relative, double angle, string[] names)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            X = new OffsetXContributor(cx, sx, sy, a, relative);
            Y = new OffsetYContributor(cy, sx, sy, a, relative);
            _angle = angle;
            _sx = sx;
            _sy = sy;
            _a = a;
        }

        /// <inheritdoc/>
        public Contributor Projection(Vector2 normal) => new ProjectionContributor(_sx, _sy, _a, _angle, normal);

        /// <inheritdoc/>
        public bool Is(string name, IEqualityComparer<string> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<string>.Default;
            return _names.Any(n => comparer.Equals(n, name));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Parent}.{(_names != null && _names.Length > 0 ? _names[0] : "?")}";
        }
    }
}
