using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Diagrams.EntityRelationDiagram
{
    /// <summary>
    /// An entity for an ERD.
    /// </summary>
    [Drawable("ENT", "An entity-relationship diagram entity. Any label after the first will be added as an attribute.", "ERD", "box rectangle")]
    public class Entity : DrawableFactory
    {
        /// <inheritdoc />
        protected override IDrawable Factory(string key, string name)
            => new Instance(name);

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        private class Instance(string name) : LocatedDrawable(name), IRoundedBox
        {
            private double _width;
            private double _top, _bottom;
            private readonly List<double> _separators = [];
            private CustomLabelAnchorPoints _anchors = null;

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
            public double MinWidth { get; set; }

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

            [Description("The header style.")]
            [Alias("header")]
            public IStyleModifier HeaderStyle { get; set; } = new BoldTextStyleModifier().Color("white", "#666").JustifyCenter();

            [Description("The style for even rows.")]
            [Alias("even")]
            public IStyleModifier EvenStyle { get; set; }

            [Description("The style for odd rows.")]
            [Alias("odd")]
            public IStyleModifier OddStyle { get; set; } = new ColorStyleModifier(null, "#ddd");

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

                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        Pins.Clear();
                        _anchors = new CustomLabelAnchorPoints(Labels.Count);
                        _separators.Clear();

                        // Create the pins (we will deal with offsets later)
                        if (Labels.Count > 0)
                        {
                            // Deal with header pins
                            Pins.Add(new FixedOrientedPin("left", "The left pin", this, default, new(-1, 0)), "l", "w", "left");
                            Pins.Add(new FixedOrientedPin("top", "The top pin", this, default, new(0, -1)), "t", "n", "top");
                            Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, default, new(0, 1)), "b", "s", "bottom");

                            // Deal with entity attributes
                            for (int i = 1; i < Labels.Count; i++)
                            {
                                Pins.Add(new FixedOrientedPin($"attr {i} left", $"The left pin for attribute {i}.", this, default, new(-1, 0)), $"l{i}", $"w{i}", $"left{i}");
                                Pins.Add(new FixedOrientedPin($"attr {i} right", $"The right pin of attribute {i}.", this, default, new(1, 0)), $"r{i}", $"e{i}", $"right{i}");
                            }
                            Pins.Add(new FixedOrientedPin("right", "The right pin", this, default, new(1, 0)), "r", "e", "right");
                        }
                        break;

                    case PreparationMode.Sizes:
                        var style = context.Style.Modify(Style);

                        if (Labels.Count > 0)
                        {
                            // Vertical coordinate computations

                            // Header (vertical)
                            _anchors[0] = new LabelAnchorPoint(new(), new(0, 0));
                            var bounds = Labels[0].Formatted.Bounds.Bounds.Expand(Margin);
                            _width = Math.Max(bounds.Width, MinWidth);
                            _top = -bounds.Height * 0.5;
                            _bottom = bounds.Height * 0.5;

                            // Attributes (vertical)
                            double iY = style.LineSpacing * style.FontSize;
                            for (int i = 1; i < Labels.Count; i++)
                            {
                                // Store the separator
                                _separators.Add(_bottom);

                                // Get the bounds, and keep half a linespacing after and before the baseline of the text
                                bounds = Labels[i].Formatted.Bounds.Bounds;
                                _width = Math.Max(bounds.Width + Margin.Left + Margin.Right, _width);

                                // We will make sure that the locations are in increments of the line spacing.
                                double t = Math.Ceiling(bounds.Top / iY) * iY - iY;
                                double b = Math.Floor(bounds.Bottom / iY) * iY + iY * 0.4;
                                SetPinOffset(i * 2 + 1, new(0, _bottom + (t + b) * 0.5));
                                SetPinOffset(i * 2 + 2, new(0, _bottom + (t + b) * 0.5));

                                _bottom -= t;
                                _anchors[i] = new LabelAnchorPoint(new(0, _bottom), Vector2.NaN, TextOrientation.Normal);
                                _bottom += b;
                            }

                            // Place the header pins horizontally
                            double w = _width * 0.5;
                            SetPinOffset(0, new(-w, 0)); // Left pin
                            SetPinOffset(1, new(0, _top)); // Top pin
                            SetPinOffset(2, new(0, _bottom)); // Bottom pin
                            SetPinOffset(Pins.Count - 1, new(w, 0)); // Right pin

                            // Place the attributes and pins horizontally
                            for (int i = 1; i < Labels.Count; i++)
                            {
                                SetPinOffset(i * 2 + 1, new(-w, ((FixedOrientedPin)Pins[i * 2 + 2]).Offset.Y));
                                SetPinOffset(i * 2 + 2, new(w, ((FixedOrientedPin)Pins[i * 2 + 2]).Offset.Y));
                                _anchors[i] = new LabelAnchorPoint(new(-w + Margin.Left, _anchors[i].Location.Y), Vector2.NaN, TextOrientation.Normal);
                            }
                        }
                        break;
                }

                return result;
            }

            /// <inheritdoc />
            protected override void FormatLabels(IPrepareContext context)
            {
                for (int i = 0; i < Labels.Count; i++)
                {
                    var style = context.Style.Modify(Style);
                    if (i == 0)
                        style = style.Modify(HeaderStyle);
                    else if (i % 2 == 1)
                        style = style.Modify(EvenStyle);
                    else
                        style = style.Modify(OddStyle);
                    Labels[i]?.Format(context.TextFormatter, style);
                }
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                var style = builder.Style.Modify(Style);

                if (Labels.Count > 1)
                {
                    // Draw the header
                    double s = _separators[0];
                    if (CornerRadius.IsZero())
                        builder.Rectangle(-_width * 0.5, _top, _width, s - _top, style.Modify(HeaderStyle).Color(Styles.Style.None, null));
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
                        }, style.Modify(HeaderStyle).Color(Styles.Style.None, null));

                        for (int i = 1; i < Labels.Count; i++)
                        {
                            var rowStyle = i % 2 == 1 ? style.Modify(EvenStyle) : style.Modify(OddStyle);
                            if (i < Labels.Count - 1)
                            {
                                double ns = _separators[i];
                                builder.Rectangle(-_width * 0.5, s, _width, ns - s, rowStyle.Color(Styles.Style.None, null));
                                s = ns;
                            }
                            else if (CornerRadius.IsZero())
                                builder.Rectangle(-_width * 0.5, s, _width, _bottom - s, rowStyle.Color(Styles.Style.None, null));
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
                                }, rowStyle.Color(Styles.Style.None, null));
                            }
                        }
                    }
                }
                else
                {
                    // There is only a header
                    builder.Rectangle(-_width * 0.5, _top, _width, _bottom - _top, style.Modify(HeaderStyle).Color(Styles.Style.None, null), CornerRadius, CornerRadius);
                }

                // The outline
                builder.Rectangle(-_width * 0.5, _top, _width, _bottom - _top, style.Color(null, Styles.Style.None), CornerRadius, CornerRadius);
                
                _anchors.Draw(builder, this, style);
            }
        }
    }
}
