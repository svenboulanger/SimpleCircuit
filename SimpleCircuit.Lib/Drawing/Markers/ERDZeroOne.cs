using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Drawing.Markers
{
    /// <summary>
    /// An entity-relation diagram marker for "zero or one".
    /// </summary>
    public class ERDZeroOne : Marker
    {
        /// <summary>
        /// Default options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "erd", "zero-one");

        /// <summary>
        /// Creates an entity-relationship diagram marker for "zero or one".
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The graphic options.</param>
        public ERDZeroOne(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options ?? DefaultOptions)
        {
        }

        /// <inheritdoc />
        protected override void DrawMarker(SvgDrawing drawing)
        {
            drawing.Circle(new(-5.5, 0), 1.5, Options);
            drawing.Line(new(-2, -1.5), new(-2, 1.5), Options);
        }
    }
}
