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
        private readonly LinkedList<IPin> _points = new();
        private readonly HashSet<Unknown> _lengths = new();

        /// <summary>
        /// Gets or sets a flag that determines whether the wire is shown as a bus.
        /// </summary>
        public bool IsBus { get; set; }

        /// <summary>
        /// Gets or sets a flag that determines whether the wire has a bus cross.
        /// </summary>
        public bool HasBusCross { get; set; }

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
        public Wire(IPin start)
        {
            _points.AddLast(start);
        }

        /// <summary>
        /// Adds a wire segment to the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="length">The length of the segment.</param>
        public void To(IPin pin, Unknown length)
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
            string @class = IsBus ? "wire bus" : "wire";
            drawing.Polyline(_points.Select(p => new Vector2(p.X.Value, p.Y.Value)), @class, $"W{index}");

            if (HasBusCross && _points.Count >= 2)
            {
                // Draw a nice little line typically used for busses
                var p2 = _points.Last.Value;
                var p1 = _points.Last.Previous.Value;
                var normal = new Vector2(p2.X.Value - p1.X.Value, p2.Y.Value - p1.Y.Value);
                normal /= normal.Length;
                var center = new Vector2(p1.X.Value + p2.X.Value, p1.Y.Value + p2.Y.Value) * 0.5;

                drawing.BeginTransform(new Transform(center.X, center.Y, normal, normal.Perpendicular));
                drawing.Line(new Vector2(-1, 3), new Vector2(1, -3), "bus cross");
                drawing.EndTransform();
            }
        }
    }
}
