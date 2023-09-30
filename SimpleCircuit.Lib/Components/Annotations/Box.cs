using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Annotations
{
    /// <summary>
    /// An annotation box.
    /// </summary>
    public class Box : IAnnotation, ILabeled
    {
        private readonly HashSet<ComponentInfo> _componentInfos = new();
        private readonly HashSet<WireInfo> _wireInfos = new();
        private readonly HashSet<IDrawable> _components = new();
        private readonly HashSet<Wire> _wires = new();

        public static readonly string Poly = "poly";

        private static readonly string _top = "top";
        private static readonly string _middle = "middle";
        private static readonly string _bottom = "bottom";
        private static readonly string _left = "left";
        private static readonly string _center = "center";
        private static readonly string _right = "right";
        private static readonly string _inside = "inside";
        private static readonly string _over = "over";

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
        /// Gets or sets the margin for wires.
        /// </summary>
        public double WireMargin { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the margin at the start of a wire.
        /// </summary>
        public double WireStartMargin { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the margin at the end of a wire.
        /// </summary>
        public double WireEndMargin { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the radius of the corners.
        /// </summary>
        public double RoundRadius { get; set; }

        /// <summary>
        /// Gets or sets the tolerance for detering an edge of the annotation box.
        /// </summary>
        public double Tolerance { get; set; } = 4.0;

        /// <summary>
        /// The offset of the label.
        /// </summary>
        public Vector2 Offset { get; set; }

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
            _componentInfos.Add(info);
        }

        /// <inheritdoc />
        public void Add(WireInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            _wireInfos.Add(info);
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
                    WireStartMargin = WireEndMargin = WireMargin = margin;
                    break;

                case "ml":
                case "leftmargin":
                case "marginleft": MarginLeft = (double)value; break;
                case "mt":
                case "topmargin":
                case "margintop": MarginTop = (double)value; break;
                case "mr":
                case "rightmargin":
                case "marginright": MarginRight = (double)value; break;
                case "mb":
                case "bottommargin":
                case "marginbottom": MarginBottom = (double)value; break;
                case "mw":
                case "marginwire":
                case "wiremargin": WireMargin = (double)value; break;
                case "mws":
                case "marginwirestart":
                case "wiremarginstart": WireStartMargin = (double)value; break;
                case "mwe":
                case "marginwireend":
                case "wiremarginend": WireEndMargin = (double)value; break;

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
            foreach (var info in _componentInfos)
            {
                var drawable = info.Get(context);
                if (drawable == null)
                    return PresenceResult.GiveUp;
                _components.Add(drawable);
            }
            foreach (var info in _wireInfos)
            {
                var wire = info.Get(context);
                if (wire == null)
                    return PresenceResult.GiveUp;
                _wires.Add(wire);
            }
            return PresenceResult.Success;
        }

        /// <inheritdoc />
        public void Register(IRegisterContext context) { }

        /// <inheritdoc />
        public void Render(SvgDrawing drawing)
        {
            // All components should have been rendered by now
            if (_components.Count + _wires.Count > 0)
            {
                // Expand the bounds by the margins
                drawing.BeginGroup(new("annotation") { Id = Name }, !Variants.Contains(_over));
                var matrix = drawing.CurrentTransform.Matrix.Inverse;
                drawing.BeginTransform(new Transform(-matrix * drawing.CurrentTransform.Offset, matrix));
                switch (Variants.Select(Poly))
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
            foreach (var drawable in _components)
            {
                bounds.Expand(drawable.Bounds.Left - MarginLeft, drawable.Bounds.Top - MarginTop);
                bounds.Expand(drawable.Bounds.Right + MarginRight, drawable.Bounds.Bottom + MarginBottom);
            }
            foreach (var wire in _wires)
            {
                bounds.Expand(wire.Bounds.Left - WireMargin, wire.Bounds.Top - WireMargin);
                bounds.Expand(wire.Bounds.Right + WireMargin, wire.Bounds.Bottom + WireMargin);
            }

            // Draw the rectangle that encompasses them all
            var total = bounds.Bounds;
            double x = total.Left;
            double y = total.Top;
            double width = total.Width;
            double height = total.Height;
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
                                Labels.SetDefaultPin(-1, location: new(x + radiusOffset + 1, y + radiusOffset + 1), expand: new(1, 1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + RoundRadius, y - 1), expand: new(1, -1));
                            break;

                        case 1:
                            // Middle
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + 1, y + 0.5 * height), expand: new(1, 0));
                            else
                                Labels.SetDefaultPin(-1, location: new(x - 1, y + 0.5 * height), expand: new(-1, 0));
                            break;

                        case 2:
                            // Bottom
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + radiusOffset + 1, y + height - radiusOffset - 1), expand: new(1, -1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + RoundRadius, y + height + 1), expand: new(1, 1));
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
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * width, y + 1), expand: new(0, 1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * width, y - 1), expand: new(0, -1));
                            break;

                        default:
                        case 1:
                            Labels.SetDefaultPin(-1, location: new(x + 0.5 * width, y + 0.5 * height), expand: new());
                            break;

                        case 2:
                            // Bottom
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * width, y + height - 1), expand: new(0, -1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * width, y + height + 1), expand: new(0, 1));
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
                                Labels.SetDefaultPin(-1, location: new(x + width - radiusOffset - 1, y + radiusOffset + 1), expand: new(-1, 1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + width - RoundRadius, y - 1), expand: new(-1, -1));
                            break;

                        case 1:
                            // Middle
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + width - 1, y + 0.5 * height), expand: new(-1, 0));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + width + 1, y + 0.5 * height), expand: new(1, 0));
                            break;

                        case 2:
                            // Bottom
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + width - radiusOffset - 1, y + height - radiusOffset - 1), expand: new(-1, -1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + width - RoundRadius, y + 1), expand: new(-1, 1));
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
            foreach (var drawable in _components)
            {
                var localBounds = drawable.Bounds;
                AddPoint(new Vector2(localBounds.Left - MarginLeft, localBounds.Top - MarginTop));
                AddPoint(new Vector2(localBounds.Right + MarginRight, localBounds.Top - MarginTop));
                AddPoint(new Vector2(localBounds.Right + MarginRight, localBounds.Bottom + MarginBottom));
                AddPoint(new Vector2(localBounds.Left - MarginLeft, localBounds.Bottom + MarginBottom));
            }
            foreach (var drawable in _wires)
            {
                if (drawable.Points.Count > 1)
                {
                    bool isFirst = true;
                    var last = drawable.Points[0];
                    Vector2 lastNormal = default, lastPerpendicular = default, normal;
                    for (int i = 1; i < drawable.Points.Count; i++)
                    {
                        var current = drawable.Points[i];
                        if ((current - last).IsZero())
                            continue;
                        normal = current - last;
                        normal /= normal.Length;
                        Vector2 perpendicular = normal.Perpendicular;
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
                else if (drawable.Points.Count == 1)
                {
                    var pt = drawable.Points[0];
                    AddPoint(new Vector2(pt.X - WireMargin, pt.Y - WireMargin));
                    AddPoint(new Vector2(pt.X + WireMargin, pt.Y - WireMargin));
                    AddPoint(new Vector2(pt.X + WireMargin, pt.Y + WireMargin));
                    AddPoint(new Vector2(pt.X - WireMargin, pt.Y + WireMargin));
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
                                Labels.SetDefaultPin(-1, location: new(x + radiusOffset + 1, y + radiusOffset + 1), expand: new(1, 1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + RoundRadius, y - 1), expand: new(1, -1));
                            break;

                        case 1:
                            // Middle
                            FindLeft(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + 1, y + 0.5 * length), expand: new(1, 0));
                            else
                                Labels.SetDefaultPin(-1, location: new(x - 1, y + 0.5 * length), expand: new(-1, 0));
                            break;

                        case 2:
                            // Bottom
                            FindBottom(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + radiusOffset + 1, y - radiusOffset - 1), expand: new(1, -1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + RoundRadius, y + 1), expand: new(1, 1));
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
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * length, y + 1), expand: new(0, 1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * length, y - 1), expand: new(0, -1));
                            break;

                        default:
                        case 1:
                            FindCenter(sortedPoints, out x, out y);
                            Labels.SetDefaultPin(-1, location: new(x, y), expand: new());
                            break;

                        case 2:
                            // Bottom
                            FindBottom(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * length, y - 1), expand: new(0, -1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + 0.5 * length, y + 1), expand: new(0, 1));
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
                                Labels.SetDefaultPin(-1, location: new(x + length - radiusOffset - 1, y + radiusOffset + 1), expand: new(-1, 1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + length - RoundRadius, y - 1), expand: new(-1, -1));
                            break;

                        case 1:
                            // Middle
                            FindRight(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x - 1, y + 0.5 * length), expand: new(-1, 0));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + 1, y + 0.5 * length), expand: new(1, 0));
                            break;

                        case 2:
                            // Bottom
                            FindBottom(sortedPoints, bounds, out x, out y, out length);
                            if (Variants.Contains(_inside))
                                Labels.SetDefaultPin(-1, location: new(x + length - radiusOffset - 1, y + length - radiusOffset - 1), expand: new(-1, -1));
                            else
                                Labels.SetDefaultPin(-1, location: new(x + length - RoundRadius, y + 1), expand: new(-1, 1));
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
            _components.Clear();
            _wires.Clear();
            return true;
        }

        /// <inheritdoc />
        public void Update(IUpdateContext context) { }
    }
}
