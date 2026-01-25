using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram;

/// <summary>
/// An entity for an Entity-Relationship Diagram.
/// </summary>
[Drawable("ENT", "An entity-relationship diagram entity. Any label after the first will be added as an attribute.", "ERD", "box rectangle", labelCount: 3)]
public partial class Entity : DrawableFactory
{
    /// <inheritdoc />
    protected override IDrawable Factory(string key, string name)
        => new Instance(name);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    private class Instance : BoundedDrawable, IRoundedBox
    {
        private double _width;
        private double _top, _bottom;
        private readonly List<double> _separators = [];
        private CustomLabelAnchorPoints _anchors = null;
        private readonly PinCollection _pins;
        private bool _extendsBeyondBounds = false;

        /// <param name="name">The name of the entity.</param>
        public Instance(string name)
            : base(name)
        {
            _pins = new PinCollection(this);
            Pins = _pins;
        }

        /// <inheritdoc />
        public override string Type => "entity";

        /// <inheritdoc />
        protected override IEnumerable<string> GroupClasses => ["diagram"];

        /// <summary>
        /// Gets or sets the width of the entity block.
        /// </summary>
        [Description("The width of the entity block. If 0, the content is used to size the entity. The default is 0.")]
        [Alias("w")]
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of the block.
        /// </summary>
        [Description("The minimum width of the entity block. Only used if the content is used to size the block.")]
        public double MinWidth { get; set; } = 20.0;

        /// <summary>
        /// Gets or sets the minimum height of the block.
        /// </summary>
        [Description("The minimum height of the entity block. Only used if the content is used to size the block.")]
        public double MinHeight { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the height of an attribute line.
        /// </summary>
        [Description("The height of a line for attributes, relative to the font size. If 0, the content of each attribute is used to size a line. The default is 2.")]
        [Alias("lh")]
        public double LineHeight { get; set; } = 2;

        /// <summary>
        /// Gets or sets the baseline for text within the line height, relative to the font size.
        /// </summary>
        [Description("The text baseline within a line, relative to the line height. The default is 0.25.")]
        public double BaseLine { get; set; } = 0.25;

        /// <summary>
        /// Gets or sets the corner radius.
        /// </summary>
        [Description("The round-off corner radius.")]
        [Alias("r")]
        [Alias("radius")]
        public double CornerRadius { get; set; }

        /// <summary>
        /// Gets or sets the minimum space.
        /// </summary>
        [Description("The minimum space between (anonymous) pins.")]
        [Alias("ms")]
        public double MinimumSpace { get; set; } = 5.0;

        [Description("The header style.")]
        [Alias("header")]
        public IStyleModifier HeaderStyle { get; set; }

        [Description("The style for even rows.")]
        [Alias("even")]
        public IStyleModifier EvenStyle { get; set; }

        [Description("The style for odd rows.")]
        [Alias("odd")]
        public IStyleModifier OddStyle { get; set; }

        /// <summary>
        /// Gets the bounds relative to the origin of the component.
        /// </summary>
        /// <remarks>This is used by the pins to calculate where they should be.</remarks>
        public Bounds RelativeBounds => new(-0.5 * _width, _top, 0.5 * _width, _bottom);

        /// <summary>
        /// Gets or sets the margin.
        /// </summary>
        [Description("The margin of the header or attributes when sizing based on content.")]
        public Margins Margin { get; set; } = new(2, 2, 2, 2);

        /// <inheritdoc />
        public override PresenceResult Prepare(IPrepareContext context)
        {
            var result = base.Prepare(context);
            if (result == PresenceResult.GiveUp)
                return result;

            // Pass preparation to our pins
            var r = _pins.Prepare(context);
            if (r == PresenceResult.GiveUp)
                return PresenceResult.GiveUp;
            else if (r == PresenceResult.Incomplete)
                result = r;

            switch (context.Mode)
            {
                case PreparationMode.Reset:
                    Pins.Clear();
                    _extendsBeyondBounds = false;
                    _anchors = new CustomLabelAnchorPoints(Labels.Count);
                    _separators.Clear();

                    // Create the pins (we will deal with offsets later)
                    if (Labels.Count > 0)
                    {
                        // Deal with header pins
                        _pins.Add(new FixedOrientedPin("left", "The left pin", this, default, new(-1, 0)), "l", "w", "left");
                        _pins.Add(new FixedOrientedPin("top", "The top pin", this, default, new(0, -1)), "t", "n", "top");
                        _pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, default, new(0, 1)), "b", "s", "bottom");

                        // Deal with entity attributes
                        for (int i = 1; i < Labels.Count; i++)
                        {
                            _pins.Add(new FixedOrientedPin($"attr {i} left", $"The left pin for attribute {i}.", this, default, new(-1, 0)), $"l{i}", $"w{i}", $"left{i}");
                            _pins.Add(new FixedOrientedPin($"attr {i} right", $"The right pin of attribute {i}.", this, default, new(1, 0)), $"r{i}", $"e{i}", $"right{i}");
                        }
                        _pins.Add(new FixedOrientedPin("right", "The right pin", this, default, new(1, 0)), "r", "e", "right");
                    }
                    break;

                case PreparationMode.Sizes:
                    var style = context.Style.ModifyDashedDotted(this);

                    if (Labels.Count > 0)
                    {
                        // Vertical coordinate computations

                        // Header (vertical)
                        _anchors[0] = new LabelAnchorPoint(new(), new(0, 0));
                        var bounds = Labels[0].Formatted.Bounds.Bounds.Expand(Margin).Expand(style.LineThickness * 0.5);
                        _width = Math.Max(bounds.Width, MinWidth);
                        _top = -bounds.Height * 0.5;
                        _bottom = bounds.Height * 0.5;

                        // Attributes (vertical)
                        for (int i = 1; i < Labels.Count; i++)
                        {
                            IStyle cstyle;
                            if (i % 2 == 1)
                                cstyle = style.Modify(EvenStyle);
                            else
                                cstyle = style.Modify(OddStyle);

                            // Store the separator
                            _separators.Add(_bottom);

                            // Get the bounds, and keep half a linespacing after and before the baseline of the text
                            bounds = Labels[i].Formatted.Bounds.Bounds;
                            _width = Math.Max(bounds.Width + Margin.Horizontal + style.LineThickness, _width);

                            // We will make sure that the locations are in increments of the line spacing.
                            double iY = cstyle.LineSpacing * cstyle.FontSize;
                            double t = bounds.Top - 0.25 * iY;
                            double b = (Math.Floor(bounds.Bottom / iY + 0.4) + 0.4) * iY;
                            _pins.SetPinOffset(i * 2 + 1, new(0, _bottom + (b - t) * 0.5));
                            _pins.SetPinOffset(i * 2 + 2, new(0, _bottom + (b - t) * 0.5));

                            _bottom -= t;
                            _anchors[i] = new LabelAnchorPoint(new(-bounds.Left + Margin.Left, _bottom), Vector2.NaN, Vector2.UX, TextOrientationType.None);
                            _bottom += b;
                        }

                        // Expand depending on the number of pins
                        _bottom = Math.Max(_bottom, _top + MinimumSpace * Math.Max(_pins.LeftCount, _pins.RightCount) + 2 * CornerRadius);
                        _bottom = Math.Max(_bottom, _top + MinHeight);
                        _width = Math.Max(_width, MinimumSpace * Math.Max(_pins.TopCount, _pins.BottomCount) + 2 * CornerRadius);
                        if (Width > 0.0)
                        {
                            _extendsBeyondBounds = _width > Width;
                            _width = Width;
                        }

                        // Place the header pins horizontally
                        double w = _width * 0.5;
                        _pins.SetPinOffset(0, new(-w, 0)); // Left pin
                        _pins.SetPinOffset(1, new(0, _top)); // Top pin
                        _pins.SetPinOffset(2, new(0, _bottom)); // Bottom pin
                        _pins.SetPinOffset(1 + Labels.Count * 2, new(w, 0)); // Right pin

                        // Place the attributes and pins horizontally
                        for (int i = 1; i < Labels.Count; i++)
                        {
                            var offset = ((FixedOrientedPin)Pins[i * 2 + 2]).Offset;
                            _pins.SetPinOffset(i * 2 + 1, new(-w, offset.Y));
                            _pins.SetPinOffset(i * 2 + 2, new(w, offset.Y));
                            _anchors[i] = new LabelAnchorPoint(new(_anchors[i].Location.X - w, _anchors[i].Location.Y), Vector2.NaN, Vector2.UX, TextOrientationType.None);
                        }
                    }
                    else
                    {
                        _width = MinWidth;
                    }
                    break;
            }
            return result;
        }

        /// <inheritdoc />
        protected override void FormatLabels(IPrepareContext context)
        {
            var style = context.Style.ModifyDashedDotted(this);
            for (int i = 0; i < Labels.Count; i++)
            {
                IStyle cstyle;
                if (i == 0)
                    cstyle = style.Modify(HeaderStyle);
                else if (i % 2 == 1)
                    cstyle = style.Modify(EvenStyle);
                else
                    cstyle = style.Modify(OddStyle);
                Labels[i]?.Format(context.TextFormatter, cstyle);
            }
        }

        /// <inheritdoc />
        protected override void Draw(IGraphicsBuilder builder)
        {
            var style = builder.Style.ModifyDashedDotted(this);

            if (Labels.Count > 1)
            {
                double s = _separators[0];
                if (CornerRadius.IsZero())
                {
                    builder.Rectangle(-_width * 0.5, _top, _width, s - _top, style.Modify(HeaderStyle).Color(Style.None, null)); // Header background

                    for (int i = 1; i < Labels.Count; i++)
                    {
                        var rowStyle = i % 2 == 1 ? style.Modify(EvenStyle) : style.Modify(OddStyle);
                        if (i < Labels.Count - 1)
                        {
                            double ns = _separators[i];
                            builder.Rectangle(-_width * 0.5, s, _width, ns - s, rowStyle.Color(Style.None, null));
                            s = ns; 
                        }
                        else
                            builder.Rectangle(-_width * 0.5, s, _width, _bottom - s, rowStyle.Color(Style.None, null));
                    }
                }
                else
                {
                    builder.Path(b =>
                    {
                        b.MoveTo(new(_width * 0.5 - CornerRadius, _top))
                            .Arc(CornerRadius, CornerRadius, 0, false, true, new(CornerRadius, CornerRadius))
                            .VerticalTo(s)
                            .HorizontalTo(-_width * 0.5)
                            .VerticalTo(_top + CornerRadius)
                            .Arc(CornerRadius, CornerRadius, 0, false, true, new(CornerRadius, -CornerRadius))
                            .Close();
                    }, style.Modify(HeaderStyle).Color(Style.None, null)); // Header background

                    for (int i = 1; i < Labels.Count; i++)
                    {
                        var rowStyle = i % 2 == 1 ? style.Modify(EvenStyle) : style.Modify(OddStyle);
                        if (i < Labels.Count - 1)
                        {
                            double ns = _separators[i];
                            builder.Rectangle(-_width * 0.5, s, _width, ns - s, rowStyle.Color(Style.None, null));
                            s = ns;
                        }
                        else if (CornerRadius.IsZero())
                            builder.Rectangle(-_width * 0.5, s, _width, _bottom - s, rowStyle.Color(Style.None, null));
                        else
                        {
                            builder.Path(b =>
                            {
                                b.MoveTo(new(-_width * 0.5, s))
                                    .Horizontal(_width)
                                    .VerticalTo(_bottom - CornerRadius)
                                    .Arc(CornerRadius, CornerRadius, 0, false, true, new(-CornerRadius, CornerRadius))
                                    .Horizontal(-_width + 2 * CornerRadius)
                                    .Arc(CornerRadius, CornerRadius, 0, false, true, new(-CornerRadius, -CornerRadius))
                                    .Close();
                            }, rowStyle.Color(Style.None, null));
                        }
                    }
                }
            }
            else
            {
                // There is only a header
                builder.Rectangle(-_width * 0.5, _top, _width, _bottom - _top, style.Modify(HeaderStyle).Color(Style.None, null), CornerRadius, CornerRadius);
            }

            // The outline
            builder.Rectangle(-_width * 0.5, _top, _width, _bottom - _top, style.Color(null, Style.None), CornerRadius, CornerRadius);
            
            _anchors.Draw(builder, this, style);
        }

        /// <inheritdoc />
        protected override PresenceResult RegisterBoundOffsets(IPrepareContext context)
        {
            if (!UsedBounds)
                return PresenceResult.Success;

            if (Width.Equals(0.0) || !_extendsBeyondBounds)
            {
                if (!context.Offsets.Group(X, Left, -0.5 * _width) ||
                    !context.Offsets.Group(Y, Top, _top) ||
                    !context.Offsets.Group(X, Right, 0.5 * _width) ||
                    !context.Offsets.Group(Y, Bottom, _bottom))
                    return PresenceResult.GiveUp;
                return PresenceResult.Success;
            }
            else
            {
                // The contents go over the bounds, so let's fall back to the general case
                return base.RegisterBoundOffsets(context);
            }
        }
    }
}
