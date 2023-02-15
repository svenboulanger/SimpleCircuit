using SimpleCircuit.Drawing.Markers;
using SimpleCircuit.Parser;

namespace SimpleCircuit.Components.Wires
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public class WireSegmentInfo
    {
        /// <summary>
        /// Gets the source of the wire segment.
        /// </summary>
        public Token Source { get; }

        /// <summary>
        /// Gets the angle of the wire.
        /// </summary>
        public Vector2 Orientation { get; set; }

        /// <summary>
        /// Gets whether the wire segment has a fixed length.
        /// </summary>
        public bool IsFixed { get; set; }

        /// <summary>
        /// Gets whether or not the wire segment is unconstrained.
        /// </summary>
        public bool IsUnconstrained { get; set; }

        /// <summary>
        /// Gets the length of the wire.
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Gets or sets the start marker.
        /// </summary>
        public Marker StartMarker { get; set; }

        /// <summary>
        /// Gets or sets the end marker.
        /// </summary>
        public Marker EndMarker { get; set; }

        /// <summary>
        /// Creates a new <see cref="WireSegmentInfo"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        public WireSegmentInfo(Token source)
        {
            Source = source;
        }
    }
}
