using SimpleCircuit.Drawing;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public class WireInfo
    {
        /// <summary>
        /// The segments for the wire.
        /// </summary>
        public List<WireSegment> Segments { get; } = new List<WireSegment>();

        /// <summary>
        /// Gets the path options for the wire.
        /// </summary>
        public PathOptions Options { get; } = new PathOptions("wire");

        /// <summary>
        /// Gets or sets whether the wire is visible or hidden.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
