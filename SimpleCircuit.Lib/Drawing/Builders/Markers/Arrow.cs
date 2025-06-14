﻿using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Drawing.Builders.Markers
{
    /// <summary>
    /// An arrow marker.
    /// </summary>
    /// <remarks>
    /// Creates a new arrow marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class Arrow(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(-2.5, -1), new(0, 0), new(-2.5, 1)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {
            appearance = appearance.AsFilledMarker();
            builder.Polygon(_points.Select(pt => pt * 2.0 * appearance.LineThickness), appearance);
        }
    }
}
