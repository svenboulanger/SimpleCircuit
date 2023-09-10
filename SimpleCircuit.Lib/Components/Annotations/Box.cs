using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Outputs;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Annotations
{
    /// <summary>
    /// An annotation box.
    /// </summary>
    public class Box : IDrawable, ILabeled, IAnnotation
    {
        private readonly HashSet<ComponentInfo> _components = new();
        private readonly HashSet<WireInfo> _wires = new();
        private readonly HashSet<IDrawable> _drawables = new();

        private static readonly string _poly = "poly";
        private static readonly string _top = "top";
        private static readonly string _middle = "middle";
        private static readonly string _bottom = "bottom";
        private static readonly string _left = "left";
        private static readonly string _center = "center";
        private static readonly string _right = "right";
        private static readonly string _inside = "inside";
        private static readonly string _outside = "outside";

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int Order => 100;

        /// <inheritdoc />
        public VariantSet Variants { get; } = new VariantSet();

        /// <inheritdoc />
        public IPinCollection Pins => null;

        /// <inheritdoc />
        public IEnumerable<string> Properties { get; }

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

        /// <inheritdoc />
        public Labels Labels { get; } = new Labels();

        /// <summary>
        /// Gets or sets the margin at the left side.
        /// </summary>
        public double MarginLeft { get; set; } = 5.0;

        /// <summary>
        /// Get sor sets the margin at the right side.
        /// </summary>
        public double MarginRight { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the margin at the top.
        /// </summary>
        public double MarginTop { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the margin at the bottom.
        /// </summary>
        public double MarginBottom { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the radius of the corners.
        /// </summary>
        public double RoundRadius { get; set; }

        /// <summary>
        /// Gets or sets the tolerance for detering an edge of the annotation box.
        /// </summary>
        public double Tolerance { get; set; } = 4.0;

        /// <summary>
        /// Creates a new <see cref="Box"/>.
        /// </summary>
        public Box(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public void Add(ComponentInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            _components.Add(info);
        }

        /// <inheritdoc />
        public void Add(WireInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            _wires.Add(info);
        }

        /// <inheritdoc />
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
        {
            string key = propertyToken.Content.ToString().ToLower();
            switch (key)
            {
                case "radius": RoundRadius = (double)value; break;

                case "margin":
                    double margin = (double)value;
                    MarginLeft = MarginTop = MarginRight = MarginBottom = margin;
                    break;

                case "marginleft": MarginLeft = (double)value; break;

                case "margintop": MarginTop = (double)value; break;

                case "marginright": MarginRight = (double)value; break;

                case "marginbottom": MarginBottom = (double)value; break;

                case "tol":
                case "tolerance": Tolerance = (double)value; break;

                default:
                    diagnostics?.Post(propertyToken, ErrorCodes.CouldNotFindPropertyOrVariant, propertyToken.Content, Name);
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public bool DiscoverNodeRelationships(IRelationshipContext context)
            => true;

        /// <inheritdoc />
        public PresenceResult Prepare(IPrepareContext context)
        {
            // Find all component info items
            foreach (var info in _components)
            {
                var drawable = info.Get(context);
                if (drawable == null)
                    return PresenceResult.GiveUp;
                _drawables.Add(drawable);
            }
            foreach (var info in _wires)
            {
                var drawable = info.Get(context);
                if (drawable == null)
                    return PresenceResult.GiveUp;
                _drawables.Add(drawable);
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context) { }

        /// <inheritdoc />
        public void Render(SvgDrawing drawing)
        {
            // All components should have been rendered by now
            if (_drawables.Count > 0)
            {
                // Expand the bounds by the margins
                drawing.BeginGroup(new("annotation") { Id = Name });
                var matrix = drawing.CurrentTransform.Matrix.Inverse;
                drawing.BeginTransform(new Transform(-matrix * drawing.CurrentTransform.Offset, matrix));
                switch (Variants.Select(_poly))
                {
                    case 0:
                        DrawPolygon(drawing);
                        break;

                    default:
                        DrawBox(drawing);
                        break;
                }
                drawing.EndTransform();
                Bounds = drawing.EndGroup();
            }
        }

        /// <summary>
        /// Draws a simple box around what needs to be annotated.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        private void DrawBox(SvgDrawing drawing)
        {
            // Compute the boxes
            var bounds = new ExpandableBounds();
            foreach (var drawable in _drawables)
                bounds.Expand(drawable.Bounds);

            // Draw the rectangle that encompasses them all
            var total = bounds.Bounds;
            double x = total.Left - MarginLeft;
            double y = total.Top - MarginTop;
            double width = total.Width + MarginLeft + MarginRight;
            double height = total.Height + MarginTop + MarginBottom;
            drawing.Rectangle(x, y, width, height, RoundRadius, RoundRadius);
            double radiusOffset = RoundRadius * 0.29289321881;

            switch (Variants.Select(_left, _center, _right))
            {
                default:
                case 0:
                    // Left
                    switch (Variants.Select(_top, _middle, _bottom))
                    {
                        default:
                        case 0:
                            // Top
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + radiusOffset + 1, y + radiusOffset + 1), new Vector2(1, 1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + RoundRadius, y - 1), new Vector2(1, -1));
                            break;

                        case 1:
                            // Middle
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + 1, y + 0.5 * height), new Vector2(1, 0));
                            else
                                drawing.Text(Labels[0], new Vector2(x - 1, y + 0.5 * height), new Vector2(-1, 0));
                            break;

                        case 2:
                            // Bottom
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + radiusOffset + 1, y + height - radiusOffset - 1), new Vector2(1, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + RoundRadius, y + height + 1), new Vector2(1, 1));
                            break;
                    }
                    break;

                case 1:
                    // Center
                    switch (Variants.Select(_top, _middle, _bottom))
                    {
                        case 0:
                            // Top
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * width, y + 1), new Vector2(0, 1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * width, y - 1), new Vector2(0, -1));
                            break;

                        default:
                        case 1:
                            drawing.Text(Labels[0], new Vector2(x + 0.5 * width, y + 0.5 * height), new Vector2());
                            break;

                        case 2:
                            // Bottom
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * width, y + height - 1), new Vector2(0, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * width, y + height + 1), new Vector2(0, 1));
                            break;

                    }
                    break;

                case 2:
                    // Right
                    switch (Variants.Select(_top, _middle, _bottom))
                    {
                        default:
                        case 0:
                            // Top
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + width - radiusOffset - 1, y + radiusOffset + 1), new Vector2(-1, 1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + width - RoundRadius, y - 1), new Vector2(-1, -1));
                            break;

                        case 1:
                            // Middle
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + width - 1, y + 0.5 * height), new Vector2(-1, 0));
                            else
                                drawing.Text(Labels[0], new Vector2(x + width + 1, y + 0.5 * height), new Vector2(1, 0));
                            break;

                        case 2:
                            // Bottom
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + width - radiusOffset - 1, y + height - radiusOffset - 1), new Vector2(-1, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + width - RoundRadius, y + 1), new Vector2(-1, 1));
                            break;

                    }
                    break;
            }
        }

        /// <summary>
        /// Draws a concave polygon around what needs to be annotated.
        /// </summary>
        /// <param name="drawing"></param>
        private void DrawPolygon(SvgDrawing drawing)
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
                var localBounds = drawable.Bounds;
                AddPoint(new Vector2(localBounds.Left - MarginLeft, localBounds.Top - MarginTop));
                AddPoint(new Vector2(localBounds.Right + MarginRight, localBounds.Top - MarginTop));
                AddPoint(new Vector2(localBounds.Right + MarginRight, localBounds.Bottom + MarginBottom));
                AddPoint(new Vector2(localBounds.Left - MarginLeft, localBounds.Bottom + MarginBottom));
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
                    while (top.Count > 1)
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
            drawing.Path(builder =>
            {
                if (RoundRadius.IsZero())
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

                    Vector2 nu = last - current;
                    double lu = nu.Length;
                    Vector2 nv = node.Next.Value - current;
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
                            double x = RoundRadius / Math.Tan(Math.Acos(dot) * 0.5);
                            if (x > lu * 0.5 || x > lv * 0.5)
                            {
                                // We don't have space for an arc, just straight line
                                builder.MoveTo(current);
                            }
                            else
                            {
                                // Segments
                                builder.MoveTo(current + nu * x);
                                builder.ArcTo(RoundRadius, RoundRadius, 0.0, false, nu.X * nv.Y - nu.Y * nv.X < 0.0, current + nv * x);
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
                                double x = RoundRadius / Math.Tan(Math.Acos(dot) * 0.5);
                                if (x > lu * 0.5 || x > lv * 0.5)
                                {
                                    // We don't have space for an arc, just straight line
                                    builder.LineTo(current);
                                }
                                else
                                {
                                    // Segments
                                    builder.LineTo(current + nu * x);
                                    builder.ArcTo(RoundRadius, RoundRadius, 0.0, false, nu.X * nv.Y - nu.Y * nv.X < 0.0, current + nv * x);
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
            });

            // Draw the label
            if (string.IsNullOrWhiteSpace(Labels[0]))
                return;
            double radiusOffset = RoundRadius * 0.29289321881;
            double x, y, length;


            switch (Variants.Select(_left, _center, _right))
            {
                default:
                case 0:
                    // Left
                    switch (Variants.Select(_top, _middle, _bottom))
                    {
                        default:
                        case 0:
                            // Top
                            FindTop(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + radiusOffset + 1, y + radiusOffset + 1), new Vector2(1, 1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + RoundRadius, y - 1), new Vector2(1, -1));
                            break;

                        case 1:
                            // Middle
                            FindLeft(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + 1, y + 0.5 * length), new Vector2(1, 0));
                            else
                                drawing.Text(Labels[0], new Vector2(x - 1, y + 0.5 * length), new Vector2(-1, 0));
                            break;

                        case 2:
                            // Bottom
                            FindBottom(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + radiusOffset + 1, y - radiusOffset - 1), new Vector2(1, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + RoundRadius, y + 1), new Vector2(1, 1));
                            break;
                    }
                    break;

                case 1:
                    // Center
                    switch (Variants.Select(_top, _middle, _bottom))
                    {
                        case 0:
                            // Top
                            FindTop(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * length, y + 1), new Vector2(0, 1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * length, y - 1), new Vector2(0, -1));
                            break;

                        default:
                        case 1:
                            FindCenter(sortedPoints, out x, out y);
                            drawing.Text(Labels[0], new Vector2(x, y), new Vector2());
                            break;

                        case 2:
                            // Bottom
                            FindBottom(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * length, y - 1), new Vector2(0, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + 0.5 * length, y + 1), new Vector2(0, 1));
                            break;

                    }
                    break;

                case 2:
                    // Right
                    switch (Variants.Select(_top, _middle, _bottom))
                    {
                        default:
                        case 0:
                            // Top
                            FindTop(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + length - radiusOffset - 1, y + radiusOffset + 1), new Vector2(-1, 1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + length - RoundRadius, y - 1), new Vector2(-1, -1));
                            break;

                        case 1:
                            // Middle
                            FindRight(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x - 1, y + 0.5 * length), new Vector2(-1, 0));
                            else
                                drawing.Text(Labels[0], new Vector2(x + 1, y + 0.5 * length), new Vector2(1, 0));
                            break;

                        case 2:
                            // Bottom
                            FindBottom(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                drawing.Text(Labels[0], new Vector2(x + length - radiusOffset - 1, y + length - radiusOffset - 1), new Vector2(-1, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + length - RoundRadius, y + 1), new Vector2(-1, 1));
                            break;

                    }
                    break;
            }
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
        public bool Reset(IResetContext diagnostics)
        {
            _drawables.Clear();
            return true;
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context) { }
    }
}
