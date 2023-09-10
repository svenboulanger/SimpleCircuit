using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Wire information.
    /// </summary>
    public class WireInfo
    {
        private Wire _wire;

        /// <summary>
        /// Gets the source.
        /// </summary>
        public Token Source { get; }

        /// <summary>
        /// Gets the full name of the wire.
        /// </summary>
        public string Fullname { get; }

        /// <summary>
        /// Gets the pin where the wire starts.
        /// </summary>
        public PinInfo PinToWire { get; set; }

        /// <summary>
        /// The segments for the wire.
        /// </summary>
        public List<WireSegmentInfo> Segments { get; set; }

        /// <summary>
        /// Gets the pin where the wire ends.
        /// </summary>
        public PinInfo WireToPin { get; set; }

        /// <summary>
        /// Gets the path options for the wire.
        /// </summary>
        public GraphicOptions Options { get; set; }

        /// <summary>
        /// Gets or sets whether the wire should jump over other wires.
        /// </summary>
        public bool JumpOverWires { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the wire is visible or hidden.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the radius for rounding corners.
        /// </summary>
        public double RoundRadius { get; set; } = 0.0;

        /// <summary>
        /// Creates a new <see cref="WireInfo"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="fullname">The full name.</param>
        public WireInfo(Token source, string fullname)
        {
            Source = source;
            Fullname = fullname;
        }

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
                if (prevSegment.StartMarkers != null || segment.StartMarkers != null ||
                    prevSegment.EndMarkers != null || segment.EndMarkers != null)
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

        /// <summary>
        /// Gets or creates a wire.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the wire, or <c>null</c> if the wire could not be found or created.</returns>
        public Wire GetOrCreate(ParsingContext context)
        {
            if (_wire != null)
                return _wire;

            if (context.Circuit.TryGetValue(Fullname, out var presence) && presence is Wire wire)
                _wire = wire;
            else
            {
                _wire = new Wire(Fullname, PinToWire, Segments, WireToPin)
                {
                    JumpOverWires = JumpOverWires,
                    IsVisible = IsVisible,
                    RoundRadius = RoundRadius,
                    Options = Options
                };
                context.Circuit.Add(_wire);
            }
            return _wire;
        }

        /// <summary>
        /// Gets a wire, potentially using a context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the wire; or <c>null</c> if the wire doesn't exist.</returns>
        public Wire Get(IPrepareContext context)
        {
            if (_wire != null)
                return _wire;

            _wire = context.Find(Fullname) as Wire;
            if (_wire == null)
                context.Diagnostics?.Post(Source, ErrorCodes.CouldNotFindWire, Source.Content);
            return _wire;
        }

        /// <inheritdoc />
        public override string ToString() => Fullname;
    }
}
