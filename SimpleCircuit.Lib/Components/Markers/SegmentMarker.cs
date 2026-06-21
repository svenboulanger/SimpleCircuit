using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// A type of <see cref="Marker"/> that will apply to only one segment.
    /// </summary>
    public abstract class SegmentMarker : Marker
    {
        /// <summary>
        /// Gets or sets the segment that the marker applies to.
        /// </summary>
        public int Segment { get; set; }

        /// <inheritdoc />
        public override void Draw(IGraphicsBuilder builder, IStyle style, IReadOnlyList<SegmentInfo> segments)
        {
            if (segments.Count == 0)
                return;

            Vector2 location, orientation;
            if (Segment == 0)
            {
                // Draw the marker at the start
                location = segments[0].Start;
                orientation = -segments[0].StartNormal;
            }
            else
            {
                var segment = segments[Math.Min(segments.Count - 1, Segment - 1)];
                location = segment.End;
                orientation = segment.EndNormal;
            }
            builder.BeginTransform(new(location, new(orientation.X, -orientation.Y, orientation.Y, orientation.X)));
            DrawMarker(builder, style);
            builder.EndTransform();
        }

        /// <summary>
        /// Draw the segment marker (the transformation is already applied).
        /// </summary>
        /// <param name="builder">The graphics builder.</param>
        /// <param name="style">The style.</param>
        protected abstract void DrawMarker(IGraphicsBuilder builder, IStyle style);
    }
}
