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

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int Order => 100;

        /// <inheritdoc />
        public VariantSet Variants { get; }

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
                var bounds = new ExpandableBounds();
                foreach (var drawable in _drawables)
                    bounds.Expand(drawable.Bounds);

                // Expand the bounds by the margins
                drawing.BeginGroup(new("annotation") { Id = Name });

                var total = bounds.Bounds;
                double x = total.Left - MarginLeft;
                double y = total.Top - MarginTop;
                double width = total.Width + MarginLeft + MarginRight;
                double height = total.Height + MarginTop + MarginBottom;
                drawing.Rectangle(x, y, width, height, RoundRadius, RoundRadius);

                drawing.Text(Labels[0], new(x, y - 1), new(1, -1));
                Bounds = drawing.EndGroup();
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
