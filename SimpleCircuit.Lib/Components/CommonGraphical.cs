using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes common graphical building blocks possibly shared by multiple components.
    /// </summary>
    public static class CommonGraphical
    {
        /// <summary>
        /// Draws a rectangle centered around the origin
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="width">The width, default is 12.</param>
        /// <param name="height">The height, default is 6.</param>
        /// <param name="center">The center of the rectangle, default is the origin.</param>
        public static void Rectangle(SvgDrawing drawing, double width = 12.0, double height = 6.0, Vector2 center = new())
        {
            width *= 0.5;
            height *= 0.5;
            drawing.Polygon(new Vector2[]
            {
                new Vector2(-width, height) + center,
                new Vector2(width, height) + center,
                new Vector2(width, -height) + center,
                new Vector2(-width, -height) + center
            });
        }

        /// <summary>
        /// Draws an arrow.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="start">The starting point of the arrow.</param>
        /// <param name="end">The ending point of the arrow.</param>
        public static void Arrow(SvgDrawing drawing, Vector2 start, Vector2 end)
        {
            drawing.Line(start, end, new("wire") { EndMarker = Drawing.PathOptions.MarkerTypes.Arrow });
        }
    }
}
