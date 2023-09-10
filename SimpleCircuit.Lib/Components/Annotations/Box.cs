using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
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
    public class Box : IDrawable, ILabeled, IAnnotation
    {
        private readonly HashSet<ComponentInfo> _components = new();
        private readonly HashSet<WireInfo> _wires = new();
        private readonly HashSet<IDrawable> _drawables = new();

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
                case "radius":
                    RoundRadius = (double)value;
                    break;

                case "margin":
                    double margin = (double)value;
                    MarginLeft = MarginTop = MarginRight = MarginBottom = margin;
                    break;

                case "marginleft":
                case "margin-left":
                    MarginLeft = (double)value;
                    break;

                case "margintop":
                case "margin-top":
                    MarginTop = (double)value;
                    break;

                case "marginright":
                case "margin-right":
                    MarginRight = (double)value;
                    break;

                case "marginbottom":
                case "margin-bottom":
                    MarginBottom = (double)value;
                    break;

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
                switch (Variants.Select())
                {
                    default:
                        DrawBox(drawing);
                        break;
                }
                Bounds = drawing.EndGroup();
            }
        }

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
            double radius_offset = RoundRadius * 0.29289321881;

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
                                drawing.Text(Labels[0], new Vector2(x + radius_offset + 1, y + radius_offset + 1), new Vector2(1, 1));
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
                                drawing.Text(Labels[0], new Vector2(x + radius_offset + 1, y + height - radius_offset - 1), new Vector2(1, -1));
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
                                drawing.Text(Labels[0], new Vector2(x + width - radius_offset - 1, y + radius_offset + 1), new Vector2(-1, 1));
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
                                drawing.Text(Labels[0], new Vector2(x + width - radius_offset - 1, y + height - radius_offset - 1), new Vector2(-1, -1));
                            else
                                drawing.Text(Labels[0], new Vector2(x + width - RoundRadius, y + 1), new Vector2(-1, 1));
                            break;

                    }
                    break;
            }
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
