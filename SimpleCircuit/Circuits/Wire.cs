using SimpleCircuit.Algebra;
using SimpleCircuit.Components;
using SimpleCircuit.Functions;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Circuits
{
    /// <summary>
    /// A wire.
    /// </summary>
    public class Wire
    {
        private readonly LinkedList<TranslatingPin> _points = new LinkedList<TranslatingPin>();
        private readonly HashSet<Unknown> _lengths = new HashSet<Unknown>();

        /// <summary>
        /// Gets the lengths for each wire segment.
        /// </summary>
        /// <value>
        /// The lengths.
        /// </value>
        public IEnumerable<Unknown> Lengths => _lengths;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wire"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        public Wire(TranslatingPin start)
        {
            _points.AddLast(start);
        }

        /// <summary>
        /// Adds a wire segment to the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="length">The length of the segment.</param>
        public void To(TranslatingPin pin, Unknown length)
        {
            pin.ThrowIfNull(nameof(pin));
            length.ThrowIfNull(nameof(length));
            _points.AddLast(pin);
            _lengths.Add(length);
        }

        /// <summary>
        /// Renders the wire.
        /// </summary>
        /// <param name="drawn">The drawn wires.</param>
        /// <param name="drawing">The drawing.</param>
        public void Render(ISet<Wire> drawn, SvgDrawing drawing)
        {
            var index = drawn.Count + 1;
            drawing.Polyline(_points.Select(p => new Vector2(p.X.Value, p.Y.Value)), "wire", $"W{index}");
        }
    }
}
