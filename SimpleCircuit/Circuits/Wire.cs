using SimpleCircuit.Components;
using SimpleCircuit.Constraints;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Circuits
{
    /// <summary>
    /// A wire between two pins.
    /// </summary>
    public class Wire
    {
        /// <summary>
        /// Gets the starting pin.
        /// </summary>
        /// <value>
        /// The pin a.
        /// </value>
        public IPin PinA { get; }

        /// <summary>
        /// Gets the pin b.
        /// </summary>
        /// <value>
        /// The pin b.
        /// </value>
        public IPin PinB { get; }

        /// <summary>
        /// Gets the preferred orientation.
        /// </summary>
        /// <value>
        /// The preferred orientation.
        /// </value>
        public double PreferredOrientation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wire"/> class.
        /// </summary>
        /// <param name="pinA">The pin of the first component.</param>
        /// <param name="pinB">The pin of the second component.</param>
        /// <param name="preferredOrientation">The preferred orientation of the wire.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="a"/> or <paramref name="b"/> is <c>null</c>.</exception>
        public Wire(IPin pinA, IPin pinB, double preferredOrientation)
        {
            PinA = pinA ?? throw new ArgumentNullException(nameof(pinA));
            PinB = pinB ?? throw new ArgumentNullException(nameof(pinB));
            PreferredOrientation = preferredOrientation;

            if (PinA.Parent is Point ptA)
                ptA.Wires++;
            if (PinB.Parent is Point ptB)
                ptB.Wires++;
        }

        /// <summary>
        /// Renders the wire.
        /// </summary>
        /// <param name="drawnWires">The drawn wires.</param>
        /// <param name="drawing">The drawing.</param>
        public void Render(IReadOnlyList<Wire> drawnWires, SvgDrawing drawing)
        {
            var start = new Vector2(PinA.X.Value, PinA.Y.Value);
            var end = new Vector2(PinB.X.Value, PinB.Y.Value);
            drawing.Line(start, end);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Wire from pin '{PinA}' to pin '{PinB}'";
        }
    }
}
