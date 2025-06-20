﻿using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Markers
{
    /// <summary>
    /// An entity-relationship diagram marker for "zero or many".
    /// </summary>
    /// <remarks>
    /// Creates a new entity-relationship diagram marker for "zero or many".
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    [Drawable("erd-zero-many", "An Entity-Relationship Diagram zero or many-symbol.", "ERD")]
    public class ERDZeroMany(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1.5), new(-3, 0), new(0, 1.5)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder, IStyle appearance)
        {   
            builder.Polyline(_points.Select(p => p * appearance.LineThickness), appearance);
            builder.Circle(new Vector2(-9, 0) * appearance.LineThickness, 1.5, appearance);
        }
    }
}
