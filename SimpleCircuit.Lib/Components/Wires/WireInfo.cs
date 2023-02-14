using SimpleCircuit.Diagnostics;
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

        /// <summary>
        /// Simplifies the wire information.
        /// </summary>
        public void Simplify()
        {
            // Only single segment, nothing to simplify...
            if (Segments.Count < 2)
                return;

            // Try to combine segments that have the same direction
            for (int i = Segments.Count - 1; i > 0; i--)
            {
                var segment = Segments[i];
                var prevSegment = Segments[i - 1];

                // If the markers are not the same, skip simplification of these two segments
                if (prevSegment.StartMarker != segment.StartMarker ||
                    prevSegment.EndMarker != segment.EndMarker)
                    continue;

                if (segment.IsUnconstrained || prevSegment.IsUnconstrained)
                {
                    // If both are unconstrained, then combine them into a single one, it wouldn't make much sense otherwise
                    if (segment.IsUnconstrained && prevSegment.IsUnconstrained)
                        Segments.RemoveAt(i);
                }
                else if (segment.Orientation.X.IsZero() && segment.Orientation.Y.IsZero())
                {
                    // Wire that will receive its direction from neighboring pins
                    // If this wire is not the first or last wire, then it will become an unconstrained pin
                    if (i != 0 && i != Segments.Count - 1)
                    {
                        if (prevSegment.IsUnconstrained)
                            Segments.RemoveAt(i);
                        else
                            segment.IsUnconstrained = true;
                    }
                }
                else
                {
                    // Succession of same-orientation wires can be combined in a single wire
                    if (segment.Orientation.Equals(prevSegment.Orientation))
                    {
                        prevSegment.Length += segment.Length;
                        prevSegment.IsFixed &= segment.IsFixed;
                        Segments.RemoveAt(i);
                    }
                }
            }
        }
    }
}
