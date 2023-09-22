using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Markers;
using System;
using System.ComponentModel.Design;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes common graphical building blocks possibly shared by multiple components.
    /// </summary>
    public static class CommonGraphical
    {
        /// <summary>
        /// Variant name for placing a label left.
        /// </summary>
        public const string Left = "left";

        /// <summary>
        /// Variant name for placing a label in the center horizontally.
        /// </summary>
        public const string Center = "center";

        /// <summary>
        /// Variant name for placing a label on the right.
        /// </summary>
        public const string Right = "right";

        /// <summary>
        /// Variant name for placing a label at the top.
        /// </summary>
        public const string Top = "top";

        /// <summary>
        /// Variant name for placing a label in the middle vertically.
        /// </summary>
        public const string Middle = "middle";

        /// <summary>
        /// Variant name for placing a label at the bottom.
        /// </summary>
        public const string Bottom = "bottom";

        /// <summary>
        /// Variant name for placing a label on the inside of the box.
        /// </summary>
        public const string Inside = "inside";

        /// <summary>
        /// Variant name for placing the label on the outside of the box.
        /// </summary>
        public const string Outside = "outside";

        /// <summary>
        /// Draws a rectangle centered around the given point.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="x">The left coordinate.</param>
        /// <param name="y">The top coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="rx">The radius along the x-axis.</param>
        /// <param name="ry">The radius along the y-axis.</param>
        /// <param name="options">Path options.</param>
        public static void Rectangle(this SvgDrawing drawing, double x, double y, double width, double height,
            double rx = double.NaN, double ry = double.NaN, GraphicOptions options = null)
        {
            // Deal with rounded corners
            if (double.IsNaN(rx) && double.IsNaN(ry))
            {
                rx = 0.0;
                ry = 0.0;
            }
            else if (double.IsNaN(rx))
                rx = ry;
            else if (double.IsNaN(ry))
                ry = rx;

            if (rx == 0.0)
            {
                // Simple version
                drawing.Polygon(new Vector2[]
                {
                    new Vector2(x, y),
                    new Vector2(x + width, y),
                    new Vector2(x + width, y + height),
                    new Vector2(x, y + height)
                }, options);
            }
            else
            {
                // Draw the rectangle
                double kx = 0.55191502449351057 * rx;
                double ky = 0.55191502449351057 * ry;
                drawing.Path(b =>
                {
                    b.MoveTo(x + rx, y);
                    b.Horizontal(width - 2 * rx);
                    if (rx != 0.0)
                        b.Curve(new(kx, 0), new(rx, ry - ky), new(rx, ry));
                    b.Vertical(height - 2 * ry);
                    if (ry != 0.0)
                        b.Curve(new(0, ky), new(-rx + kx, ry), new(-rx, ry));
                    b.Horizontal(2 * rx - width);
                    if (rx != 0.0)
                        b.Curve(new(-kx, 0), new(-rx, ky - ry), new(-rx, -ry));
                    b.Vertical(2 * ry - height);
                    if (ry != 0)
                        b.Curve(new(0, -ky), new(rx - kx, -ry), new(rx, -ry));
                    b.Close();
                }, options);
            }
        }

        /// <summary>
        /// Draws a rectangle center around the given point.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="size">The size.</param>
        /// <param name="center">The center of the rectangle, default is the origin.</param>
        /// <param name="options">Path options.</param>
        public static void Rectangle(this SvgDrawing drawing, Vector2 size, Vector2 center = new(), GraphicOptions options = null)
        {
            size *= 0.5;
            drawing.Polygon(new Vector2[]
            {
                new Vector2(-size.X, size.Y) + center,
                new Vector2(size.X, size.Y) + center,
                new Vector2(size.X, -size.Y) + center,
                new Vector2(-size.X, -size.Y) + center
            }, options);
        }

        /// <summary>
        /// Draws an arrow.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="start">The starting point of the arrow.</param>
        /// <param name="end">The ending point of the arrow.</param>
        public static void Arrow(this SvgDrawing drawing, Vector2 start, Vector2 end, GraphicOptions options = null)
        {
            drawing.BeginGroup(options);
            drawing.Line(start, end);
            var normal = end - start;
            normal /= normal.Length;

            var marker = new Arrow(end, normal);
            marker.Draw(drawing);
            drawing.EndGroup();
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
            drawing.BeginGroup(new("signs"));

            // Plus sign
            drawing.Path(b => b.MoveTo(plus.X, plus.Y - size * 0.5).Vertical(size).MoveTo(plus.X - size * 0.5, plus.Y).Horizontal(size), new("plus"));

            // Minus sign
            size *= 0.5;
            if (vertical)
                drawing.Line(new(minus.X, minus.Y - size), new(minus.X, minus.Y + size), new("minus"));
            else
                drawing.Line(new(minus.X - size, minus.Y), new(minus.X + size, minus.Y), new("minus"));

            drawing.EndGroup();
        }

        /// <summary>
        /// Draws a cross.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="center">The center.</param>
        /// <param name="size">The size.</param>
        /// <param name="options">The options.</param>
        public static void Cross(this SvgDrawing drawing, Vector2 center, double size, GraphicOptions options = null)
        {
            drawing.Path(b =>
                b
                .MoveTo(center - new Vector2(size, size) * 0.5)
                .Line(size, size)
                .MoveTo(center - new Vector2(-size, size) * 0.5)
                .Line(-size, size), options);
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

        /// <summary>
        /// Extends a wire from the given pin.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="pin">The pin.</param>
        /// <param name="length">The length of the wire.</param>
        public static void ExtendPin(this SvgDrawing drawing, IPin pin, double length = 2)
        {
            if (pin.Connections == 0)
            {
                if (pin is FixedOrientedPin fop)
                    drawing.Line(fop.Offset, fop.Offset + fop.RelativeOrientation * length, new("wire"));
                else if (pin is FixedPin fp)
                {
                    var marker = new Dot(fp.Offset, new(1, 0), new("marker", "dot", "wire"));
                    marker.Draw(drawing);
                }
            }
        }

        /// <summary>
        /// Extends all pins.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="length">The length of the pin wire.</param>
        public static void ExtendPins(this SvgDrawing drawing, IPinCollection pins, double length = 2)
        {
            foreach (var pin in pins)
                drawing.ExtendPin(pin, length);
        }

        /// <summary>
        /// Extends the specified pins.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="length">The length of the pins.</param>
        /// <param name="names">The names of the pins to extend.</param>
        public static void ExtendPins(this SvgDrawing drawing, IPinCollection pins, double length, params string[] names)
        {
            foreach (string name in names)
                drawing.ExtendPin(pins[name], length);
        }

        /// <summary>
        /// Draws a label around or inside a box, depending on variants.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        /// <param name="labels">The labels.</param>
        /// <param name="variants">The variants.</param>
        /// <param name="topLeft">The top-left corner of the box.</param>
        /// <param name="bottomRight">The bottom-right corner of the box.</param>
        /// <param name="index">The label index.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="defaultHorizontal">The default horizontal location. -1 for left, 0 for center or 1 for right.</param>
        /// <param name="defaultVertical">The default vertical location. -1 for top, 0 for middle or 1 for bottom.</param>
        /// <param name="defaultInside">The default location inside/outside. 0 for inside, 1 for outside.</param>
        /// <param name="margin">The margin from the edge inside or outside.</param>
        /// <param name="options">The graphic options.</param>
        public static void BoxedLabel(this SvgDrawing drawing,
            Labels labels,
            int index,
            VariantSet variants,
            Vector2 topLeft,
            Vector2 bottomRight,
            Vector2 offset = default,
            int defaultHorizontal = 0,
            int defaultVertical = 0,
            int defaultInside = 0,
            double margin = 1.0)
        {
            // No need to deal with all this if there is no label to draw
            if (string.IsNullOrWhiteSpace(labels[index]))
                return;

            // Get horizontal alignment
            int horizontal = variants.Select(Left, Center, Right);
            if (horizontal < 0)
                horizontal = defaultHorizontal + 1;

            // Get vertical alignment
            int vertical = variants.Select(Top, Middle, Bottom);
            if (vertical < 0)
                vertical = defaultVertical + 1;

            // Short out for centered text
            if (horizontal == 1 && vertical == 1)
            {
                // Centered text
                labels.Draw(drawing, index, 0.5 * (topLeft + bottomRight) + offset, new Vector2());
                return;
            }

            // Determine whether the label should be in- or outside the box
            int inout = variants.Select(Inside, Outside);
            if (inout < 0)
                inout = defaultInside;

            // Determine the location and expansion vector
            Vector2 loc, n;
            if (inout == 0)
            {
                // Inside the box
                if (horizontal == 1)
                {
                    loc = new Vector2(0.5 * (topLeft.X + bottomRight.X), vertical == 0 ? topLeft.Y + margin : bottomRight.Y - margin);
                    n = new Vector2(0, vertical == 0 ? 1 : -1);
                }
                else if (vertical == 1)
                {
                    loc = new Vector2(horizontal == 0 ? topLeft.X + margin : bottomRight.X - margin, 0.5 * (topLeft.Y + bottomRight.Y));
                    n = new Vector2(horizontal == 0 ? 1 : -1, 0);
                }
                else
                {
                    loc = new Vector2(
                        horizontal == 0 ? topLeft.X + margin : bottomRight.X - margin,
                        vertical == 0 ? topLeft.Y + margin : bottomRight.Y - margin);
                    n = new Vector2(horizontal == 0 ? 1 : -1, vertical == 0 ? 1 : -1);
                }
            }
            else
            {
                // Outside the box
                if (horizontal == 1)
                {
                    loc = new Vector2(0.5 * (topLeft.X + bottomRight.X), vertical == 0 ? topLeft.Y - margin : bottomRight.Y + margin);
                    n = new Vector2(0, vertical == 0 ? -1 : 1);
                }
                else if (vertical == 1)
                {
                    loc = new Vector2(horizontal == 0 ? topLeft.X - margin : bottomRight.X + margin, 0.5 * (topLeft.Y + bottomRight.Y));
                    n = new Vector2(horizontal == 0 ? -1 : 1, 0);
                }
                else
                {
                    loc = new Vector2(
                        horizontal == 0 ? topLeft.X : bottomRight.X,
                        vertical == 0 ? topLeft.Y - margin : bottomRight.Y + margin);
                    n = new Vector2(horizontal == 0 ? 1 : -1, vertical == 0 ? -1 : 1);
                }
            }

            // Draw the label
            labels.Draw(drawing, index, loc + offset, n);
        }
    }
}