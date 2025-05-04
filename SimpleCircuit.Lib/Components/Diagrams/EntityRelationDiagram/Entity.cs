using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Appearance;
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
            private double _width, _height;
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

            /// <summary>
            /// Gets or sets the margin.
            /// </summary>
            [Description("The margin of the header or attributes when sizing based on content.")]
            public Margins Margin { get; set; } = new(2, 2, 2, 2);

            /// <inheritdoc />
            public override PresenceResult Prepare(IPrepareContext context)
            {
                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        Pins.Clear();
                        _anchors = null;

                        // Create the pins (we will deal with offsets later)
                        if (Labels.Count > 0)
                        {
                            Pins.Add(new FixedOrientedPin("left", "The left pin", this, default, new(-1, 0)), "l", "w", "left");
                            Pins.Add(new FixedOrientedPin("top", "The top pin", this, default, new(0, -1)), "t", "n", "top");
                            Pins.Add(new FixedOrientedPin("bottom", "The bottom pin", this, default, new(0, 1)), "b", "s", "bottom");
                            for (int i = 1; i < Labels.Count; i++)
                            {
                                Pins.Add(new FixedOrientedPin($"attr {i} left", $"The left pin for attribute {i}.", this, default, new(-1, 0)), $"l{i}", $"w{i}", $"left{i}");
                                Pins.Add(new FixedOrientedPin($"attr {i} right", $"The right pin of attribute {i}.", this, default, new(1, 0)), $"r{i}", $"e{i}", $"right{i}");
                            }
                            Pins.Add(new FixedOrientedPin("right", "The right pin", this, default, new(1, 0)), "r", "e", "right");
                        }
                        break;

                    case PreparationMode.Offsets:

                        // Place the header
                        _anchors[0] = new LabelAnchorPoint(_anchors[0].Location + new Vector2(_width * 0.5, 0), _anchors[0].Expand, Appearance);

                        ((FixedOrientedPin)Pins[1]).Offset = new(_width * 0.5, 0);
                        ((FixedOrientedPin)Pins[2]).Offset = new(_width * 0.5, _height);
                        ((FixedOrientedPin)Pins[^1]).Offset = new(_width, ((FixedOrientedPin)Pins[0]).Offset.Y);
                        for (int i = 1; i < Labels.Count; i++)
                            ((FixedOrientedPin)Pins[i * 2 + 2]).Offset = new(_width, ((FixedOrientedPin)Pins[i * 2 + 1]).Offset.Y);
                        break;
                }

                var result = base.Prepare(context);
                if (result == PresenceResult.GiveUp)
                    return result;

                switch (context.Mode)
                {
                    case PreparationMode.Sizes:
                        // Calculate the size of the total entity, update accordingly
                        _anchors = new CustomLabelAnchorPoints(new LabelAnchorPoint[Labels.Count]);
                        _height = Margin.Top;
                        _width = 0.0;
                        for (int i = 0; i < Labels.Count; i++)
                        {
                            var bounds = Labels[i].Formatted.Bounds.Bounds;

                            if (i == 0)
                            {
                                // This is the header, we will center it according to the line height
                                _anchors[0] = new LabelAnchorPoint(
                                    new(0, _height + LineHeight * (1 - BaseLine) * Labels[0].Size + bounds.Top),
                                    new(0, 1), Appearance);
                                ((FixedOrientedPin)Pins[0]).Offset = new(0, _height + LineHeight * Labels[0].Size * 0.5);
                                _height += Margin.Bottom;
                            }
                            else
                            {
                                // These are attributes, we will simply follow the line height given
                                _anchors[i] = new LabelAnchorPoint(
                                    new(Margin.Left + bounds.Left, _height + LineHeight * (1 - BaseLine) * Labels[i].Size + bounds.Top),
                                    new(1, 1), Appearance);

                                // We can already update the left-side pins (we will u
                                ((FixedOrientedPin)Pins[i * 2 + 1]).Offset = new(0, _height + LineHeight * Labels[i].Size * 0.5);
                            }    
                            _height += LineHeight * Labels[i].Size;
                            _width = Math.Max(_width, bounds.Width + Margin.Left + Margin.Right);
                        }
                        break;
                }

                return result;
            }

            /// <inheritdoc />
            protected override void Draw(IGraphicsBuilder builder)
            {
                if (Labels.Count > 1)
                {
                    var options = Appearance.CreatePathOptions(this);

                    // The header
                    double y = Margin.Top + Margin.Bottom + LineHeight * Labels[0].Size;
                    builder.Path(b =>
                    {
                        b.MoveTo(new(_width - CornerRadius, 0));
                        if (!CornerRadius.Equals(0.0))
                            b.ArcTo(CornerRadius, CornerRadius, 0.0, false, true, new(_width, CornerRadius));
                        b.VerticalTo(y);
                        b.HorizontalTo(0);
                        b.VerticalTo(CornerRadius);
                        if (!CornerRadius.Equals(0.0))
                            b.ArcTo(CornerRadius, CornerRadius, 0.0, false, true, new(CornerRadius, 0));
                        b.Close();
                    }, options);

                    // The attributes
                    for (int i = 1; i < Labels.Count - 1; i++)
                    {
                        builder.Rectangle(0.0, y, _width, LineHeight * Labels[i].Size, Appearance, options: options);
                        y += LineHeight * Labels[i].Size;
                    }

                    // Last attribute
                    builder.Path(b =>
                    {
                        b.MoveTo(new(0, y));
                        b.HorizontalTo(_width);
                        b.LineTo(new(_width, _height - CornerRadius));
                        if (!CornerRadius.Equals(0.0))
                            b.ArcTo(CornerRadius, CornerRadius, 0.0, false, true, new(_width - CornerRadius, _height));
                        b.HorizontalTo(CornerRadius);
                        if (!CornerRadius.Equals(0.0))
                            b.ArcTo(CornerRadius, CornerRadius, 0.0, false, true, new(0, _height - CornerRadius));
                        b.Close();
                    }, options);

                    // All attributes encapsulated
                    y = Margin.Top + Margin.Bottom + LineHeight * Labels[0].Size;
                    builder.Path(b =>
                    {
                        b.MoveTo(new(0, y));
                        b.HorizontalTo(_width);
                        b.VerticalTo(_height - CornerRadius);
                        if (!CornerRadius.Equals(0.0))
                            b.ArcTo(CornerRadius, CornerRadius, 0.0, false, true, new(_width - CornerRadius, _height));
                        b.HorizontalTo(CornerRadius);
                        if (!CornerRadius.Equals(0.0))
                            b.ArcTo(CornerRadius, CornerRadius, 0.0, false, true, new(0, _height - CornerRadius));
                        b.Close();
                    }, options);
                }
                else
                {
                    builder.Rectangle(0, 0, _width, _height, Appearance, CornerRadius, CornerRadius, Appearance.CreatePathOptions(this));
                }    
                _anchors.Draw(builder, this);
            }
        }
    }
}
