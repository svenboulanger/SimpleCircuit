using SimpleCircuit.Drawing;
using System;

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
        public static void Rectangle(this SvgDrawing drawing, double width = 12.0, double height = 6.0, Vector2 center = new(), PathOptions options = null)
        {
            width *= 0.5;
            height *= 0.5;
            drawing.Polygon(new Vector2[]
            {
                new Vector2(-width, height) + center,
                new Vector2(width, height) + center,
                new Vector2(width, -height) + center,
                new Vector2(-width, -height) + center
            }, options);
        }

        /// <summary>
        /// Draws an arrow.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="start">The starting point of the arrow.</param>
        /// <param name="end">The ending point of the arrow.</param>
        public static void Arrow(this SvgDrawing drawing, Vector2 start, Vector2 end, PathOptions options = null)
        {
            if (options == null)
                options = new PathOptions();
            options.EndMarker = PathOptions.MarkerTypes.Arrow;
            options.Classes.Add("arrow");
            drawing.Line(start, end, options);
        }

        /// <summary>
        /// Draws a plus and a minus.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="plus">The center of the plus sign.</param>
        /// <param name="minus">The center of the minus sign.</param>
        /// <param name="size">The size of the signs. The default is 2.</param>
        /// <param name="vertical">If <c>true</c>, the minus sign is drawn vertically.</param>
        public static void Signs(this SvgDrawing drawing, Vector2 plus, Vector2 minus, double size = 2, bool vertical = false)
        {
            // Plus sign
            drawing.Path(b => b.MoveTo(plus.X, plus.Y - size * 0.5).Vertical(size).MoveTo(plus.X - size * 0.5, plus.Y).Horizontal(size), new("plus"));

            // Minus sign
            size *= 0.5;
            if (vertical)
                drawing.Line(new(minus.X, minus.Y - size), new(minus.X, minus.Y + size), new("minus"));
            else
                drawing.Line(new(minus.X - size, minus.Y), new(minus.X + size, minus.Y), new("minus"));
        }

        /// <summary>
        /// Draws a cross.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="center">The center.</param>
        /// <param name="size">The size.</param>
        /// <param name="options">The options.</param>
        public static void Cross(this SvgDrawing drawing, Vector2 center, double size, PathOptions options = null)
        {
            drawing.Path(b =>
                b
                .MoveTo(center - new Vector2(size, size) * 0.5)
                .Line(size, size)
                .MoveTo(center - new Vector2(-size, size) * 0.5)
                .Line(-size, size), options);
        }

        /// <summary>
        /// Draws a dot.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="center">The center.</param>
        /// <param name="size">The size of the dot.</param>
        /// <param name="options">The path options.</param>
        public static void Dot(this SvgDrawing drawing, Vector2 center = new(), double size = 1.0, PathOptions options = null)
        {
            if (options == null)
                options = new();
            options.Classes.Add("dot");
            drawing.Circle(center, size, options);
        }

        /// <summary>
        /// Draws an AC symbol.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="center">The center of the AC wiggle.</param>
        /// <param name="size">The size of the AC wiggle.</param>
        /// <param name="vertical">If <c>true</c>, the wiggle is placed vertically.</param>
        public static void AC(this SvgDrawing drawing, Vector2 center = new(), double size = 3, bool vertical = false)
        {
            drawing.BeginTransform(new Transform(center, (vertical ? Matrix2.Identity : Matrix2.Rotate(Math.PI / 2)) * size / 3.0));
            drawing.OpenBezier(new Vector2[]
            {
                new(0, -3),
                new(1.414, -2.293),
                new(1.414, -0.707),
                new(),
                new(-1.414, 0.707),
                new(-1.414, 2.293),
                new(0, 3)
            }, new("ac"));
            drawing.EndTransform();
        }
    }
}