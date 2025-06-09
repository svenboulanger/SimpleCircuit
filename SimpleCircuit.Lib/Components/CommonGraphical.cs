using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using System;

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
        /// The variant used for a wire that is dashed.
        /// </summary>
        public const string Dashed = "dashed";

        /// <summary>
        /// The variant used for a wire that is dotted.
        /// </summary>
        public const string Dotted = "dotted";

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="x">The left coordinate.</param>
        /// <param name="y">The top coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="rx">The radius along the x-axis.</param>
        /// <param name="ry">The radius along the y-axis.</param>
        /// <param name="appearance">Path options.</param>
        public static void Rectangle(this IGraphicsBuilder builder, double x, double y, double width, double height, IStyle appearance,
            double rx = double.NaN, double ry = double.NaN)
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
                builder.Polygon(
                [
                    new Vector2(x, y),
                    new Vector2(x + width, y),
                    new Vector2(x + width, y + height),
                    new Vector2(x, y + height)
                ], appearance);
            }
            else
            {
                // Draw the rectangle
                double kx = 0.55191502449351057 * rx;
                double ky = 0.55191502449351057 * ry;
                builder.Path(b =>
                {
                    b.MoveTo(new(x + rx, y));
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
                }, appearance);
            }
        }

        /// <summary>
        /// Draws a diamond shape centered around the given point.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="x">The center X-coordinate.</param>
        /// <param name="y">The center Y-coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="rx">The radius for the left and right corners.</param>
        /// <param name="ry">The radius for the top and bottom corners.</param>
        /// <param name="options">The path options.</param>
        public static void Diamond(this IGraphicsBuilder builder, double x, double y, double width, double height, IStyle options,
            double rx = 0.0, double ry = 0.0)
        {
            DiamondSize(width, height, rx, ry, out var n, out var ox, out var oy);
            var loc = new Vector2(x, y);
            var ax = new Vector2(width * 0.5, 0);
            var ay = new Vector2(0, height * 0.5);

            builder.Path(b =>
            {
                b.MoveTo(loc - ax + ox);
                b.LineTo(loc - ay + oy);
                if (!ry.IsZero())
                     b.ArcTo(ry, ry, 0.0, false, true, loc - ay + new Vector2(-oy.X, oy.Y));
                b.LineTo(loc + ax + new Vector2(-ox.X, ox.Y));
                if (!rx.IsZero())
                     b.ArcTo(rx, rx, 0.0, false, true, loc + ax - ox);
                b.LineTo(loc + ay - oy);
                if (!ry.IsZero())
                    b.ArcTo(ry, ry, 0.0, false, true, loc + ay - new Vector2(-oy.X, oy.Y));
                b.LineTo(loc - ax + new Vector2(ox.X, -ox.Y));
                if (!rx.IsZero())
                    b.ArcTo(rx, rx, 0.0, false, true, loc - ax + ox);
            }, options);
        }

        /// <summary>
        /// Computes the normal and entry points of the rounded edges for a given width and height.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="rx">The radius for the left and right corners.</param>
        /// <param name="ry">The radius for the top and bottom corners.</param>
        /// <param name="n">The normal of the straight lines.</param>
        /// <param name="ox">The top-left offset for the left corner compared to (-width/2,0).</param>
        /// <param name="oy">The top-left offset for the top corner compared to (0,-height/2).</param>
        public static void DiamondSize(double width, double height, double rx, double ry, out Vector2 n, out Vector2 ox, out Vector2 oy)
        {
            if (rx.IsZero() && ry.IsZero())
            {
                n = new(width, height);
                n /= n.Length;
                ox = new();
                oy = new();
                return;
            }

            double x0 = width * 0.5 - rx;
            double y0 = height * 0.5 - ry;
            double x0sq = x0 * x0;
            double y0sq = y0 * y0;
            double ap = 1.0 / x0sq + 1.0 / y0sq;
            double bp = rx / x0sq + ry / y0sq;
            double cp = rx * rx / x0sq + ry * ry / y0sq - 1.0;
            double c = (bp + Math.Sqrt(bp * bp - ap * cp)) / ap;

            n = new Vector2(-(ry - c) / y0, -(rx - c) / x0);
            double hp = c / n.X;
            double wp = c / n.Y;

            double dot = n.X * n.X - n.Y * n.Y; // left-right corners
            double lx = rx / Math.Tan(Math.Acos(dot) * 0.5);
            ox = new Vector2(-wp + width * 0.5 + lx * n.X, -lx * n.Y);

            dot = -dot; // top-down
            lx = ry / Math.Tan(Math.Acos(dot) * 0.5);
            oy = new Vector2(-lx * n.X, -hp + height * 0.5 + lx * n.Y);
        }

        /// <summary>
        /// Computes the offset of a key point based on the location.
        /// </summary>
        /// <param name="width">The diamond width.</param>
        /// <param name="height">The diamond height.</param>
        /// <param name="ox">The top-left offset for left corner compared to (-width/2,0).</param>
        /// <param name="oy">The top-left offset for the top corner compared to (0,-height/2).</param>
        /// <param name="location">The location.</param>
        /// <returns>The offset compared to the center of the diamond.</returns>
        public static Vector2 GetDiamondOffset(double width, double height, Vector2 ox, Vector2 oy, DiamondLocation location)
        {
            return location switch
            {
                DiamondLocation.Left => new(-width * 0.5, 0),
                DiamondLocation.TopLeftLeft => new(-width * 0.5 + ox.X, ox.Y),
                DiamondLocation.TopLeftTop => new(oy.X, -height * 0.5 + oy.Y),
                DiamondLocation.Top => new(0, -height * 0.5),
                DiamondLocation.TopRightTop => new(-oy.X, -height * 0.5 + oy.Y),
                DiamondLocation.TopRightRight => new(width * 0.5 - ox.X, ox.Y),
                DiamondLocation.Right => new(width * 0.5, 0),
                DiamondLocation.BottomRightRight => new(width * 0.5 - ox.X, -ox.Y),
                DiamondLocation.BottomRightBottom => new(-oy.X, height * 0.5 - oy.Y),
                DiamondLocation.Bottom => new(0, height * 0.5),
                DiamondLocation.BottomLeftBottom => new(oy.X, height * 0.5 - oy.Y),
                DiamondLocation.BottomLeftLeft => new(-width * 0.5 + ox.X, -ox.Y),
                _ => default,
            };
        }

        /// <summary>
        /// Draws a parallellogram centered around the given point.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="x">The center X-coordinate.</param>
        /// <param name="y">The center Y-coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="edge">The slanted edge.</param>
        /// <param name="style">The path options.</param>
        /// <param name="radiusSharp">The radius for the sharp corners.</param>
        /// <param name="radiusBlunt">The radius for the blunt corners.</param>
        public static void Parallellogram(this IGraphicsBuilder builder, double x, double y, double width,
            Vector2 edge, IStyle style, double radiusSharp = 0.0, double radiusBlunt = 0.0)
        {
            var pcorner = new Vector2(-width * 0.5, -edge.Y * 0.5);
            var horiz = new Vector2((width - edge.X) * 0.5, 0);
            var edgeh = edge * 0.5;
            RoundedCorner(pcorner, horiz, edgeh, radiusSharp,
                out var ps1, out var ps2, out bool cornerSharp);
            RoundedCorner(pcorner + edge, -edgeh, horiz, radiusBlunt,
                out var pb1, out var pb2, out bool cornerBlunt);
            builder.BeginTransform(new(new(x, y), Matrix2.Identity));
            builder.Path(b =>
            {
                b.MoveTo(ps1);
                if (cornerSharp)
                    b.ArcTo(radiusSharp, radiusSharp, 0.0, false, true, ps2);

                b.LineTo(pb1);
                if (cornerBlunt)
                    b.ArcTo(radiusBlunt, radiusBlunt, 0.0, false, true, pb2);

                b.LineTo(-ps1);
                if (cornerSharp)
                    b.ArcTo(radiusSharp, radiusSharp, 0.0, false, true, -ps2);

                b.LineTo(-pb1);
                if (cornerBlunt)
                    b.ArcTo(radiusBlunt, radiusBlunt, 0.0, false, true, -pb2);
                b.Close();
            }, style);
            builder.EndTransform();
        }

        /// <summary>
        /// Draws an arrow.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="drawable">The drawable parent.</param>
        public static void Arrow(this IGraphicsBuilder builder, Vector2 start, Vector2 end, IStyle appearance)
        {
            builder.Line(start, end, appearance.AsStroke());

            // Draw the marker
            var normal = end - start;
            normal /= normal.Length;
            var marker = new Arrow(end, normal);
            marker.Draw(builder, appearance);
        }

        /// <summary>
        /// Draws a plus and a minus.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="plus">The center of the plus sign.</param>
        /// <param name="minus">The center of the minus sign.</param>
        /// <param name="size">The size of the signs. The default is 2.</param>
        /// <param name="vertical">If <c>true</c>, the minus sign is drawn vertically.</param>
        public static void Signs(this IGraphicsBuilder builder, Vector2 plus, Vector2 minus, IStyle style, double size = 2, bool vertical = false, bool upright=false)
        {
            style = style.AsStrokeMarker(Style.DefaultLineThickness);
            size *= 0.5;

            // Make sure the plus and minus are upright
            var invMatrix = upright ? builder.CurrentTransform.Matrix.Inverse : Matrix2.Identity;

            // Plus sign
            builder.BeginTransform(new(plus, invMatrix));
            builder.Path(b => b.MoveTo(new(0, -size)).Vertical(2 * size).MoveTo(new(-size, 0)).Horizontal(2 * size), style);
            builder.EndTransform();

            // Minus sign
            builder.BeginTransform(new(minus, invMatrix));
            if (vertical)
                builder.Line(new(0, -size), new(0, size), style);
            else
                builder.Line(new(-size, 0), new(size, 0), style);
            builder.EndTransform();
        }

        /// <summary>
        /// Draws a cross.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="center">The center.</param>
        /// <param name="size">The size.</param>
        /// <param name="options">The options.</param>
        public static void Cross(this IGraphicsBuilder builder, Vector2 center, double size, IStyle options)
        {
            builder.Path(b =>
                b
                .MoveTo(center - new Vector2(size, size) * 0.5)
                .Line(new(size, size))
                .MoveTo(center - new Vector2(-size, size) * 0.5)
                .Line(new(-size, size)), options);
        }

        /// <summary>
        /// Draws an AC symbol.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="center">The center of the AC wiggle.</param>
        /// <param name="size">The size of the AC wiggle.</param>
        /// <param name="vertical">If <c>true</c>, the wiggle is placed vertically.</param>
        public static void AC(this IGraphicsBuilder builder, IStyle options, Vector2 center = new(), double size = 3, bool vertical = false)
        {
            builder.BeginTransform(new Transform(center, (vertical ? Matrix2.Identity : Matrix2.Rotate(Math.PI / 2)) * size / 3.0));
            builder.Path(b =>
            {
                b
                    .MoveTo(new(0, -3))
                    .CurveTo(new(1.414, -2.293), new(1.414, -0.707), new())
                    .CurveTo(new(-1.414, 0.707), new(-1.414, 2.293), new(0, 3));
            }, options);
            builder.EndTransform();
        }

        /// <summary>
        /// Extends a wire from the given pin.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="pin">The pin.</param>
        /// <param name="length">The length of the wire.</param>
        public static void ExtendPin(this IGraphicsBuilder builder, IPin pin, IStyle appearance, double length = 2)
        {
            if (pin.Connections == 0)
            {
                if (pin is FixedOrientedPin fop)
                    builder.Line(fop.Offset, fop.Offset + fop.RelativeOrientation * length, appearance);
                else if (pin is FixedPin fp)
                {
                    var marker = new Dot(fp.Offset, new(1, 0));
                    marker.Draw(builder, appearance);
                }
            }
        }

        /// <summary>
        /// Extends all pins.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="length">The length of the pin wire.</param>
        public static void ExtendPins(this IGraphicsBuilder builder, IPinCollection pins, IStyle appearance, double length = 2)
        {
            foreach (var pin in pins)
                builder.ExtendPin(pin, appearance, length);
        }

        /// <summary>
        /// Extends the specified pins.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="pins">The pins.</param>
        /// <param name="length">The length of the pins.</param>
        /// <param name="names">The names of the pins to extend.</param>
        public static void ExtendPins(this IGraphicsBuilder builder, IPinCollection pins, IStyle appearance, double length, params string[] names)
        {
            foreach (string name in names)
                builder.ExtendPin(pins[name], appearance, length);
        }

        /// <summary>
        /// Expands a vector in the downward direction. If the y-coordinate of the vector
        /// is below <paramref name="y"/>, then the vector is modified to have that y-coordinate.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="y">The y-coordinate.</param>
        public static void ExpandDown(ref this Vector2 vector, double y)
        {
            if (vector.Y < y)
                vector = new Vector2(vector.X, y);
        }

        /// <summary>
        /// Expands a vector in the upward direction. If the y-coordinate of the vector
        /// is above <paramref name="y"/>, then the vector is modified to have that y-coordinate.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="y">The y-coordinate.</param>
        public static void ExpandUp(ref this Vector2 vector, double y)
        {
            if (vector.Y > y)
                vector = new Vector2(vector.X, y);
        }

        /// <summary>
        /// Computes the entry points when rounding corners.
        /// </summary>
        /// <param name="point">The corner location.</param>
        /// <param name="a">The first edge, starting in <paramref name="point"/>.</param>
        /// <param name="b">The second edge, starting in <paramref name="point"/>.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="p1">The first entry point, along the first edge.</param>
        /// <param name="p2">The second entry point, along the second edge.</param>
        /// <param name="corner">If <c>true</c> the corner can be drawn; otherwise, <c>false</c>.</param>
        /// <returns>Returns the inset along the edges.</returns>
        public static double RoundedCorner(Vector2 point,
            Vector2 a, Vector2 b, double radius,
            out Vector2 p1, out Vector2 p2, out bool corner)
        {
            double x = RoundedCorner(a, b, radius);
            corner = false;
            p2 = p1 = point;
            if (x < a.Length && x < b.Length)
            {
                p1 += a / a.Length * x;
                p2 += b / b.Length * x;
                corner = true;
            }
            return x;
        }

        /// <summary>
        /// Computes the inset for the points when rounding corners.
        /// </summary>
        /// <param name="a">The direction of the first edge.</param>
        /// <param name="b">The direction of the second edge.</param>
        /// <param name="radius">The radius of the corner.</param>
        /// <returns>Returns the distance from the origin to where the arc would touch the edges.</returns>
        public static double RoundedCorner(Vector2 a, Vector2 b, double radius)
        {
            var u = a / a.Length;
            var v = b / b.Length;
            double alpha = Math.Acos(u.Dot(v));
            double x = radius / Math.Tan(alpha * 0.5);
            return x;
        }

        /// <summary>
        /// Computes an interpolated point on a rounded corner.
        /// </summary>
        /// <param name="point">The corner point.</param>
        /// <param name="a">The first edge, starting in <paramref name="point"/>.</param>
        /// <param name="b">The second edge, starting in <paramref name="point"/>.</param>
        /// <param name="radius">The radius of the corner.</param>
        /// <param name="fraction">The fraction. If <c>0</c>, uses the point on <paramref name="a"/>; if <c>1</c>, uses the point on <paramref name="b"/>.</param>
        /// <param name="p">The point location.</param>
        /// <param name="n">The normal at the interpolated point.</param>
        /// <param name="corner">If <c>true</c>, the corner can be drawn; otherwise, <c>false</c>.</param>
        public static void InterpRoundedCorner(Vector2 point,
            Vector2 a, Vector2 b, double radius, double fraction,
            out Vector2 p, out bool corner)
        {
            var u = a / a.Length;
            var v = b / b.Length;
            double alpha = Math.Acos(u.Dot(v));
            double x = radius / Math.Tan(alpha * 0.5);

            if (x >= a.Length || x > b.Length)
            {
                p = point;
                corner = false;
                return;
            }

            corner = true;
            if (fraction < 0.001)
                p = point + x * u;
            else if (fraction > 0.999)
                p = point + x * v;
            else
            {
                // Compute the center of the arc
                var center = point;
                if (u.X * v.Y - u.Y * v.X > 0)
                    center += u * x + u.Perpendicular * radius;
                else
                    center += u * x - u.Perpendicular * radius;

                // Compute the angles to the end points
                double a0 = Math.Atan2(-u.Y, -u.X);
                if (a0 < 0)
                    a0 += 2 * Math.PI;
                double a1 = Math.Atan2(-v.Y, -v.X);
                if (a1 < 0)
                    a1 += 2 * Math.PI;
                p = center + Vector2.Normal(a0 + fraction * (a1 - a0)) * radius;
            }
        }

        /// <summary>
        /// Formats and draws the text.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="value">The value.</param>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="style">The style.</param>
        public static void Text(this IGraphicsBuilder builder, string value, Vector2 location, TextOrientation orientation, IStyle style)
        {
            var span = builder.TextFormatter.Format(value, style);
            if (span is not null)
            {
                builder.Text(span, location, orientation);
            }
        }
    }
}