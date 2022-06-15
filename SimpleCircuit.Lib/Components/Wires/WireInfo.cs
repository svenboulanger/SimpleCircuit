using SimpleCircuit.Drawing;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public class WireInfo
    {
        /// <summary>
        /// The segments for the wire.
        /// </summary>
        public List<WireSegmentInfo> Segments { get; } = new List<WireSegmentInfo>();

        /// <summary>
        /// Gets the path options for the wire.
        /// </summary>
        public PathOptions Options { get; } = new PathOptions("wire");

        /// <summary>
        /// Gets or sets whether the wire should jump over other wires.
        /// </summary>
        public bool JumpOverWires { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the wire is visible or hidden.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
