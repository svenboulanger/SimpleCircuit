﻿using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components.Annotations
{
    /// <summary>
    /// An annotation box.
    /// </summary>
    /// <param name="name">The name of the box.</param>
    public class Box(string name) : IAnnotation, IBoxDrawable, IRoundedBox
    {
        private readonly HashSet<IDrawable> _drawables = [];
        private Vector2 _topLeft, _bottomRight;
        private static readonly OffsetAnchorPoints<IBoxDrawable> _anchors = new(BoxLabelAnchorPoints.Default, 1);

        /// <summary>
        /// Class identifier for an annotation box that shapes like a polygon.
        /// </summary>
        public const string Poly = "poly";

        /// <summary>
        /// Class identifier that indicates the annotation should be drawn on top.
        /// </summary>
        public const string Over = "over";

        /// <inheritdoc />
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

        /// <inheritdoc />
        public List<TextLocation> Sources { get; } = [];

        /// <inheritdoc />
        public int Order => 100;

        /// <inheritdoc />
        public VariantSet Variants { get; } = [];

        /// <inheritdoc />
        public IPinCollection Pins => null;

        /// <inheritdoc />
        public IEnumerable<string[]> Properties => Drawable.GetProperties(this);

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

        /// <inheritdoc />
        public Labels Labels { get; } = new();

        /// <summary>
        /// Gets or sets the margin for components inside the box.
        /// </summary>
        [Description("The margins for components inside the box.")]
        [Alias("m")]
        public Margins Margin { get; set; } = new(3, 3, 3, 3);

        /// <summary>
        /// Gets or sets the margin for wires inside the box.
        /// </summary>
        [Description("The margin for the box along wires.")]
        [Alias("wm")]
        public double WireMargin { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets the margin for the start of wires inside the box.
        /// </summary>
        [Description("The margin for the box for the start of wires.")]
        [Alias("wsm")]
        public double WireStartMargin { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets the margin for the end of wires inside the box.
        /// </summary>
        [Description("The margin for the annotation box for the end of wires.")]
        [Alias("wem")]
        public double WireEndMargin { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets corner radius of the box outline.
        /// </summary>
        [Description("The round-off corner radius.")]
        [Alias("r")]
        [Alias("radius")]
        public double CornerRadius { get; set; }

        /// <summary>
        /// Gets or sets the tolerance for points along one side to place labels.
        /// </summary>
        [Description("The tolerance to group points together as a single side of the box for placing label anchors.")]
        [Alias("tol")]
        public double Tolerance { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the margin for labels to the box outline.
        /// </summary>
        [Description("The margin for labels to the edge.")]
        [Alias("lm")]
        public double LabelMargin { get; set; } = 1.0;

        /// <inheritdoc />
        public IStyleModifier Style { get; set; } = new ColorStyleModifier($"--primary", null);

        /// <inheritdoc />
        Vector2 IBoxDrawable.TopLeft => _topLeft;

        /// <inheritdoc />
        Vector2 IBoxDrawable.Center => (_topLeft + _bottomRight) / 2;

        /// <inheritdoc />
        Vector2 IBoxDrawable.BottomRight => _bottomRight;

        /// <inheritdoc />
        public void Add(IDrawable drawable)
        {
            _drawables.Add(drawable ?? throw new ArgumentNullException(nameof(drawable)));
        }

        /// <inheritdoc />
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            => Drawable.SetProperty(this, propertyToken, value, diagnostics);

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context) => PresenceResult.Success;

        /// <inheritdoc />
        public void Register(IRegisterContext context) { }

        /// <inheritdoc />
        public void Render(IGraphicsBuilder builder)
        {
            // All components should have been rendered by now
            if (_drawables.Count > 0)
            {
                var style = builder.Style.Modify(Style);

                // Expand the bounds by the margins
                builder.BeginGroup(Name, ["annotation"], !Variants.Contains(Over));
                var matrix = builder.CurrentTransform.Matrix.Inverse;
                builder.BeginTransform(new Transform(-matrix * builder.CurrentTransform.Offset, matrix));
                switch (Variants.Select(Poly))
                {
                    case 0:
                        DrawPolygon(builder, style);
                        break;

                    default:
                        DrawBox(builder, style);
                        break;
                }
                builder.EndTransform();
                builder.EndGroup();
            }
        }

        /// <summary>
        /// Draws a simple box around what needs to be annotated.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private void DrawBox(IGraphicsBuilder builder, IStyle style)
        {
            // Compute the boxes
            var bounds = new ExpandableBounds();
            foreach (var drawable in _drawables)
            {
                if (drawable is Wire)
                {
                    bounds.Expand(drawable.Bounds.Left - WireMargin, drawable.Bounds.Top - WireMargin);
                    bounds.Expand(drawable.Bounds.Right + WireMargin, drawable.Bounds.Bottom + WireMargin);
                }
                else
                {
                    bounds.Expand(drawable.Bounds.Left - Margin.Left, drawable.Bounds.Top - Margin.Top);
                    bounds.Expand(drawable.Bounds.Right + Margin.Right, drawable.Bounds.Bottom + Margin.Bottom);
                }
            }

            // Draw the rectangle that encompasses them all
            var total = bounds.Bounds;
            double x = total.Left;
            double y = total.Top;
            double width = total.Width;
            double height = total.Height;
            builder.Rectangle(x, y, width, height, style, CornerRadius, CornerRadius);

            _topLeft = new Vector2(total.Left, total.Top);
            _bottomRight = new Vector2(total.Right, total.Bottom);
            _anchors.Draw(builder, this, style);
        }

        /// <summary>
        /// Draws a concave polygon around what needs to be annotated.
        /// </summary>
        /// <param name="builder"></param>
        private void DrawPolygon(IGraphicsBuilder builder, IStyle style)
        {
            // Track the point cloud
            var sortedPoints = new SortedDictionary<double, ExpandableLine>();
            var bounds = new ExpandableBounds();
            void AddPoint(Vector2 point)
            {
                bounds.Expand(point);
                if (!sortedPoints.TryGetValue(point.X, out var expandable))
                {
                    expandable = new ExpandableLine();
                    sortedPoints.Add(point.X, expandable);
                }
                expandable.Expand(point.Y);
            }
            foreach (var drawable in _drawables)
            {
                if (drawable is Wire wire)
                {
                    if (wire.Points.Count > 1)
                    {
                        bool isFirst = true;
                        var last = wire.Points[0];
                        Vector2 lastNormal = default, lastPerpendicular = default, normal;
                        for (int i = 1; i < wire.Points.Count; i++)
                        {
                            var current = wire.Points[i];
                            if ((current - last).IsZero())
                                continue;
                            normal = current - last;
                            normal /= normal.Length;
                            var perpendicular = normal.Perpendicular;
                            if (isFirst)
                            {
                                isFirst = false;

                                // Add the two starting points now that we know the normal
                                AddPoint(last - normal * WireStartMargin + perpendicular * WireMargin);
                                AddPoint(last - normal * WireStartMargin - perpendicular * WireMargin);
                            }
                            else
                            {
                                // Add the two intermediate points
                                double determinant = lastNormal.X * normal.Y - lastNormal.Y * normal.X;
                                if (determinant.IsZero())
                                {
                                    if ((lastNormal - normal).IsZero())
                                        continue; // No change in direction
                                    else
                                    {
                                        // Opposite direction
                                        AddPoint(last + (-normal + perpendicular) * WireMargin);
                                        AddPoint(last + (-normal - perpendicular) * WireMargin);
                                    }
                                }
                                else
                                {
                                    var matrix = new Matrix2(-lastNormal.X, normal.X, -lastNormal.Y, normal.Y);
                                    var inv = matrix.Inverse;
                                    var kl = inv * (lastPerpendicular - perpendicular);
                                    AddPoint(last + (lastNormal * kl.X + lastPerpendicular) * WireMargin);
                                    AddPoint(last - (lastNormal * kl.X + lastPerpendicular) * WireMargin);
                                }
                            }

                            last = current;
                            lastNormal = normal;
                            lastPerpendicular = perpendicular;
                        }

                        // Complete the last two points
                        AddPoint(last + lastNormal * WireEndMargin + lastPerpendicular * WireMargin);
                        AddPoint(last + lastNormal * WireEndMargin - lastPerpendicular * WireMargin);
                    }
                    else if (wire.Points.Count == 1)
                    {
                        var pt = wire.Points[0];
                        AddPoint(new Vector2(pt.X - WireMargin, pt.Y - WireMargin));
                        AddPoint(new Vector2(pt.X + WireMargin, pt.Y - WireMargin));
                        AddPoint(new Vector2(pt.X + WireMargin, pt.Y + WireMargin));
                        AddPoint(new Vector2(pt.X - WireMargin, pt.Y + WireMargin));
                    }
                }
                else
                {
                    var localBounds = drawable.Bounds;
                    AddPoint(new Vector2(localBounds.Left - Margin.Left, localBounds.Top - Margin.Top));
                    AddPoint(new Vector2(localBounds.Right + Margin.Right, localBounds.Top - Margin.Top));
                    AddPoint(new Vector2(localBounds.Right + Margin.Right, localBounds.Bottom + Margin.Bottom));
                    AddPoint(new Vector2(localBounds.Left - Margin.Left, localBounds.Bottom + Margin.Bottom));
                }
            }

            // Build the polygon boundary lines top and bottom
            var top = new LinkedList<Vector2>();
            var bottom = new LinkedList<Vector2>();
            foreach (var pair in sortedPoints)
            {
                var vline = pair.Value;

                // Empty
                if (top.Count == 0)
                {
                    if (vline.Minimum == vline.Maximum)
                    {
                        top.AddLast(new Vector2(pair.Key, vline.Minimum));
                        bottom.AddLast(new Vector2(pair.Key, vline.Maximum));
                    }
                    else
                    {
                        top.AddLast(new Vector2(pair.Key, vline.Minimum));
                        bottom.AddLast(new Vector2(pair.Key, vline.Minimum));
                        bottom.AddLast(new Vector2(pair.Key, vline.Maximum));
                    }
                }
                else
                {
                    // Add a new point to the top boundary line
                    var newPoint = new Vector2(pair.Key, vline.Minimum);
                    while (top.Count > 1)
                    {
                        var last = top.Last.Value;
                        var secondLast = top.Last.Previous.Value;
                        double vector = (newPoint.X - last.X) * (last.Y - secondLast.Y) - (last.X - secondLast.X) * (newPoint.Y - last.Y);
                        if (vector.IsZero() || vector > 0.0)
                            top.RemoveLast();
                        else
                            break;
                    }
                    top.AddLast(newPoint);

                    // Add a new point to the bottom boundary line
                    newPoint = new Vector2(pair.Key, vline.Maximum);
                    while (bottom.Count > 1)
                    {
                        var last = bottom.Last.Value;
                        var secondLast = bottom.Last.Previous.Value;
                        double vector = (newPoint.X - last.X) * (last.Y - secondLast.Y) - (last.X - secondLast.X) * (newPoint.Y - last.Y);
                        if (vector.IsZero() || vector < 0.0)
                            bottom.RemoveLast();
                        else
                            break;
                    }
                    bottom.AddLast(newPoint);
                }
            }

            // String the points together to make the polygon
            var node = bottom.Last;
            while (node != null)
            {
                top.AddLast(node.Value);
                node = node.Previous;
            }
            bottom.Clear();
            top.RemoveLast();

            // Draw a polygon with rounded corners accordingly
            builder.Path(builder =>
            {
                if (CornerRadius.IsZero())
                {
                    var node = top.First;
                    builder.MoveTo(node.Value);
                    node = node.Next;
                    while (node != null)
                    {
                        builder.LineTo(node.Value);
                        node = node.Next;
                    }
                    builder.Close();
                }
                else
                {
                    // We will need to fit in rounded radius wherever possible
                    // The first point should be a bit special
                    var last = top.Last.Value;
                    var node = top.First;
                    var current = node.Value;

                    var nu = last - current;
                    double lu = nu.Length;
                    var nv = node.Next.Value - current;
                    double lv = nv.Length;
                    if (lu > 0 && lv > 0)
                    {
                        nu /= lu;
                        nv /= lv;
                        double dot = nu.Dot(nv);
                        if (dot > 0.999 || dot < -0.999)
                            builder.MoveTo(current);
                        else
                        {
                            // Rounded corner
                            double x = CornerRadius / Math.Tan(Math.Acos(dot) * 0.5);
                            if (x > lu * 0.5 || x > lv * 0.5)
                            {
                                // We don't have space for an arc, just straight line
                                builder.MoveTo(current);
                            }
                            else
                            {
                                // Segments
                                builder.MoveTo(current + nu * x);
                                builder.ArcTo(CornerRadius, CornerRadius, 0.0, false, nu.X * nv.Y - nu.Y * nv.X < 0.0, current + nv * x);
                            }
                        }
                    }
                    else
                        builder.MoveTo(current);

                    // Move to the next points
                    last = current;
                    node = node.Next;
                    while (node != null)
                    {
                        current = node.Value;
                        var next = node.Next?.Value ?? top.First.Value;

                        nu = last - current;
                        lu = nu.Length;
                        nv = next - current;
                        lv = nv.Length;
                        if (lu > 0 && lv > 0)
                        {
                            nu /= lu;
                            nv /= lv;
                            double dot = nu.Dot(nv);
                            if (dot > 0.999 || dot < -0.999)
                                builder.LineTo(current);
                            else
                            {
                                // Rounded corner
                                double x = CornerRadius / Math.Tan(Math.Acos(dot) * 0.5);
                                if (x > lu * 0.5 || x > lv * 0.5)
                                {
                                    // We don't have space for an arc, just straight line
                                    builder.LineTo(current);
                                }
                                else
                                {
                                    // Segments
                                    builder.LineTo(current + nu * x);
                                    builder.ArcTo(CornerRadius, CornerRadius, 0.0, false, nu.X * nv.Y - nu.Y * nv.X < 0.0, current + nv * x);
                                }
                            }
                        }
                        else
                            builder.LineTo(current);
                        last = current;
                        node = node.Next;
                    }
                    builder.Close();
                }
            }, style);

            // Draw the labels
            if (Labels.Count == 0)
                return;

            var anchors = new LabelAnchorPoint[25];
            double radiusOffset = CornerRadius * 0.29289321881;

            // Points 0-11 are the outside points, add them here
            FindCenter(sortedPoints, out double xCenter, out double yCenter);
            anchors[0] = new LabelAnchorPoint(new(xCenter, yCenter), new());
            FindTop(sortedPoints, bounds, out double xTop, out double yTop, out double lengthTop);
            anchors[1] = new LabelAnchorPoint(new(xTop + CornerRadius, yTop - LabelMargin), new(1, -1));
            anchors[2] = new LabelAnchorPoint(new(xTop + 0.5 * lengthTop, yTop - LabelMargin), new(0, -1));
            anchors[3] = new LabelAnchorPoint(new(xTop + lengthTop - CornerRadius, yTop - LabelMargin), new(-1, -1));
            FindRight(sortedPoints, bounds, out double xRight, out double yRight, out double lengthRight);
            anchors[4] = new LabelAnchorPoint(new(xRight + LabelMargin, yRight + CornerRadius), new(1, 1));
            anchors[5] = new LabelAnchorPoint(new(xRight + LabelMargin, yRight + 0.5 * lengthRight), new(1, 0));
            anchors[6] = new LabelAnchorPoint(new(xRight + LabelMargin, yRight + lengthRight - CornerRadius), new(1, -1));
            FindBottom(sortedPoints, bounds, out double xBottom, out double yBottom, out double lengthBottom);
            anchors[7] = new LabelAnchorPoint(new(xBottom + lengthBottom - CornerRadius, yBottom + LabelMargin), new(-1, 1));
            anchors[8] = new LabelAnchorPoint(new(xBottom + 0.5 * lengthBottom, yBottom + LabelMargin), new(0, 1));
            anchors[9] = new LabelAnchorPoint(new(xBottom + CornerRadius, yBottom + LabelMargin), new(1, 1));
            FindLeft(sortedPoints, bounds, out double xLeft, out double yLeft, out double lengthLeft);
            anchors[10] = new LabelAnchorPoint(new(xLeft - LabelMargin, yLeft + lengthLeft - CornerRadius), new(-1, -1));
            anchors[11] = new LabelAnchorPoint(new(xLeft - LabelMargin, yLeft + 0.5 * lengthLeft), new(-1, 0));
            anchors[12] = new LabelAnchorPoint(new(xLeft - LabelMargin, yLeft + CornerRadius), new(-1, 1));

            // Points 12-23 are the inside points, add them here
            double s = Math.Max(CornerRadius, LabelMargin);
            anchors[13] = new LabelAnchorPoint(new(xTop + s, yTop + LabelMargin), new(1, 1));
            anchors[14] = new LabelAnchorPoint(new(xTop + 0.5 * lengthTop, yTop + LabelMargin), new(0, 1));
            anchors[15] = new LabelAnchorPoint(new(xTop + lengthTop - s, yTop + LabelMargin), new(-1, 1));
            anchors[16] = new LabelAnchorPoint(new(xRight - LabelMargin, yRight + s), new(-1, 1));
            anchors[17] = new LabelAnchorPoint(new(xRight - LabelMargin, yRight + 0.5 * lengthRight), new(-1, 0));
            anchors[18] = new LabelAnchorPoint(new(xRight - LabelMargin, yRight + lengthRight - s), new(-1, -1));
            anchors[19] = new LabelAnchorPoint(new(xBottom + lengthBottom - s, yBottom - LabelMargin), new(-1, -1));
            anchors[20] = new LabelAnchorPoint(new(xBottom + 0.5 * lengthBottom, yBottom - LabelMargin), new(0, -1));
            anchors[21] = new LabelAnchorPoint(new(xBottom + s, yBottom - LabelMargin), new(1, -1));
            anchors[22] = new LabelAnchorPoint(new(xLeft + LabelMargin, yLeft + lengthLeft - s), new(1, -1));
            anchors[23] = new LabelAnchorPoint(new(xLeft + LabelMargin, yLeft + 0.5 * lengthLeft), new(1, 0));
            anchors[24] = new LabelAnchorPoint(new(xLeft + LabelMargin, yLeft + s), new(1, 1));

            new OffsetAnchorPoints<IDrawable>(new CustomLabelAnchorPoints(anchors), 1).Draw(builder, this, style);
        }

        /// <summary>
        /// Finds a section that represents the top of a point cloud.
        /// </summary>
        /// <param name="points">The point cloud.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="x">The result x.</param>
        /// <param name="y">The result y.</param>
        /// <param name="length">The length of the segment.</param>
        void FindTop(SortedDictionary<double, ExpandableLine> points, ExpandableBounds bounds, out double x, out double y, out double length)
        {
            x = 0.0;
            y = bounds.Bounds.Top;
            length = -1.0;
            foreach (var point in points)
            {
                if (Math.Abs(point.Value.Minimum - y) < Tolerance)
                {
                    if (length < 0.0)
                    {
                        x = point.Key;
                        length = 0.0;
                    }
                    else
                        length = point.Key - x;
                }
            }
        }

        /// <summary>
        /// Finds a section that represents the bottom of a point cloud.
        /// </summary>
        /// <param name="points">The point cloud.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="x">The result x.</param>
        /// <param name="y">The result y.</param>
        /// <param name="length">The length of the segment.</param>
        void FindBottom(SortedDictionary<double, ExpandableLine> points, ExpandableBounds bounds, out double x, out double y, out double length)
        {
            x = 0.0;
            y = bounds.Bounds.Bottom;
            length = -1.0;
            foreach (var point in points)
            {
                if (Math.Abs(point.Value.Maximum - y) < Tolerance)
                {
                    if (length < 0.0)
                    {
                        x = point.Key;
                        length = 0.0;
                    }
                    else
                        length = point.Key - x;
                }
            }
        }

        /// <summary>
        /// Finds a section that represents the left of a point cloud.
        /// </summary>
        /// <param name="points">The point cloud.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="x">The result x.</param>
        /// <param name="y">The result y.</param>
        /// <param name="length">The length of the segment.</param>
        void FindLeft(SortedDictionary<double, ExpandableLine> points, ExpandableBounds bounds, out double x, out double y, out double length)
        {
            x = bounds.Bounds.Left;
            y = double.PositiveInfinity;
            length = double.NegativeInfinity;
            foreach (var point in points)
            {
                if (Math.Abs(point.Key - x) < Tolerance)
                {
                    if (point.Value.Minimum < y)
                        y = point.Value.Minimum;
                    if (point.Value.Maximum > length)
                        length = point.Value.Maximum;
                }
            }
            length -= y;
        }

        /// <summary>
        /// Finds a section that represents the right of a point cloud.
        /// </summary>
        /// <param name="points">The point cloud.</param>
        /// <param name="bounds">The bounds.</param>
        /// <param name="x">The result x.</param>
        /// <param name="y">The result y.</param>
        /// <param name="length">The length of the segment.</param>
        void FindRight(SortedDictionary<double, ExpandableLine> points, ExpandableBounds bounds, out double x, out double y, out double length)
        {
            x = bounds.Bounds.Right;
            y = double.PositiveInfinity;
            length = double.NegativeInfinity;
            foreach (var point in points)
            {
                if (Math.Abs(point.Key - x) < Tolerance)
                {
                    if (point.Value.Minimum < y)
                        y = point.Value.Minimum;
                    if (point.Value.Maximum > length)
                        length = point.Value.Maximum;
                }
            }
            length -= y;
        }

        /// <summary>
        /// Finds the center of a point cloud.
        /// </summary>
        /// <param name="points">The point cloud.</param>
        /// <param name="x">The center X-coordinate.</param>
        /// <param name="y">The center Y-coordinate.</param>
        void FindCenter(SortedDictionary<double, ExpandableLine> points, out double x, out double y)
        {
            var avg = new Vector2();
            int count = 0;
            foreach (var point in points)
            {
                avg += new Vector2(point.Key, point.Value.Minimum);
                avg += new Vector2(point.Key, point.Value.Maximum);
                count += 2;
            }
            avg /= count;
            x = avg.X;
            y = avg.Y;
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context) { }
    }
}
