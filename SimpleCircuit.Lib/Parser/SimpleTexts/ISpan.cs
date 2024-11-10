using SimpleCircuit.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// Represents a span of similar styling.
    /// </summary>
    public interface ISpan
    {
        /// <summary>
        /// Gets the bounds of the span.
        /// </summary>
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Updates the span with the new line start information.
        /// </summary>
        /// <param name="offset">The location of the start of the line.</param>
        public void Update(Vector2 offset);
    }

    /// <summary>
    /// A span representing a simple text element.
    /// </summary>
    /// <remarks>
    /// Creates a new text span from a measured XML element.
    /// </remarks>
    /// <param name="content">The contents of the text span.</param>
    /// <param name="bounds">The bounds of the text span.</param>
    /// <param name="fontFamily">The font family.</param>
    /// <param name="isBold">If <c>true</c>, the font weight is bold.</param>
    /// <param name="size">The size of the text.</param>
    public class TextSpan(string content, string fontFamily, bool isBold, double size, SpanBounds bounds) : ISpan
    {
        /// <summary>
        /// Gets the content of the text span.
        /// </summary>
        public string Content { get; } = content ?? string.Empty;

        /// <summary>
        /// Gets the font family of the text span.
        /// </summary>
        public string FontFamily { get; } = fontFamily;

        /// <summary>
        /// Gets whether the text span is bold.
        /// </summary>
        public bool Bold { get; } = isBold;

        /// <summary>
        /// Gets the size of the text span.
        /// </summary>
        public double Size { get; } = size;

        /// <inheritdoc />
        public SpanBounds Bounds { get; } = bounds;

        /// <summary>
        /// Gets the offset of the text.
        /// </summary>
        public Vector2 Offset { get; private set; }

        /// <inheritdoc />
        public void Update(Vector2 offset) => Offset = offset;
    }

    /// <summary>
    /// A span that simply adds multiple 
    /// </summary>
    public class LineSpan : ISpan, IEnumerable<ISpan>
    {
        private readonly List<ISpan> _spans = [];
        private readonly ExpandableBounds _bounds = new();
        private double _x = 0.0;

        /// <inheritdoc />
        public SpanBounds Bounds => new(_bounds.Bounds, _x);

        /// <summary>
        /// Gets the margin between items.
        /// </summary>
        public double Margin { get; }

        /// <summary>
        /// Creates a new <see cref="LineSpan"/>.
        /// </summary>
        /// <param name="flow">The flow direction.</param>
        public LineSpan(double margin = 0.0)
        {
            Margin = margin;
            _bounds.Expand(new Vector2());
        }

        /// <summary>
        /// Expands the bounds
        /// </summary>
        /// <param name="span"></param>
        public void Add(ISpan span)
        {
            _spans.Add(span);

            // Expand the bounds assuming X-direction
            _bounds.Expand(new Vector2(_x, 0) + span.Bounds.Bounds);
            _x += span.Bounds.Advance + Margin;
        }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            double x = 0.0;
            foreach (var span in _spans)
            {
                span.Update(new Vector2(offset.X + x, offset.Y));
                x += span.Bounds.Advance + Margin;
            }
        }

        /// <inheritdoc />
        public IEnumerator<ISpan> GetEnumerator() => ((IEnumerable<ISpan>)_spans).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_spans).GetEnumerator();
    }

    /// <summary>
    /// A superscript span.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="SubscriptSuperscriptSpan"/>.
    /// </remarks>
    /// <param name="base">The base.</param>
    /// <param name="halfway">The height of the halfway point.</param>
    public class SubscriptSuperscriptSpan(ISpan @base, double halfway) : ISpan
    {
        private ISpan _super = null, _sub = null;

        /// <summary>
        /// Gets or sets the superscript.
        /// </summary>
        public ISpan Super { get => _super; set { _super = value; UpdateBounds(); } }

        /// <summary>
        /// Gets or sets the subscript.
        /// </summary>
        public ISpan Sub { get => _sub; set { _sub = value; UpdateBounds(); } }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        public ISpan Base { get; } = @base;

        /// <summary>
        /// Gets the margin between the spans.
        /// </summary>
        public Vector2 Margin { get; set; }

        /// <summary>
        /// Gets the half-way point.
        /// </summary>
        public double Halfway { get; } = halfway;

        /// <inheritdoc />
        public SpanBounds Bounds { get; private set; } = @base.Bounds;

        /// <summary>
        /// Gets the location for the superscript.
        /// </summary>
        protected Vector2 SuperLocation => new(Base.Bounds.Bounds.Right + Margin.X - Super.Bounds.Bounds.Left, -Halfway - Margin.Y * 0.5 - Super.Bounds.Bounds.Bottom);

        /// <summary>
        /// Gets the location for the subscript.
        /// </summary>
        protected Vector2 SubLocation => new(Base.Bounds.Bounds.Right + Margin.X - Sub.Bounds.Bounds.Left, -Halfway + Margin.Y * 0.5 - Sub.Bounds.Bounds.Top);

        private void UpdateBounds()
        {
            ExpandableBounds bounds = new();

            // Account for the space that the base takes up
            // The base is always at the origin
            bounds.Expand(Base.Bounds.Bounds);
            if (Super != null)
                bounds.Expand(SuperLocation + Super.Bounds.Bounds);
            if (Sub != null)
                bounds.Expand(SubLocation + Sub.Bounds.Bounds);
            Bounds = new(bounds.Bounds, bounds.Bounds.Right);
        }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            Base.Update(offset);
            Super?.Update(SuperLocation + offset);
            Sub?.Update(SubLocation + offset);
        }
    }

    /// <summary>
    /// A span for multiple lines.
    /// </summary>
    public class MultilineSpan : ISpan, IEnumerable<ISpan>
    {
        private readonly List<ISpan> _spans = [];
        private readonly ExpandableBounds _bounds = new();

        /// <summary>
        /// Gets or sets the line spacing.
        /// </summary>
        public double LineIncrement { get; }

        /// <summary>
        /// Alignment. If less than 0, the text is left-aligned. If it is 0 it is centered, otherwise it is right-aligned.
        /// </summary>
        public double Align { get; }

        /// <inheritdoc />
        public SpanBounds Bounds => new(_bounds.Bounds, _bounds.Bounds.Right);

        /// <summary>
        /// Creates a new <see cref="MultilineSpan"/>.
        /// </summary>
        /// <param name="lineIncrement">The increment in y-direction for each span.</param>
        public MultilineSpan(double lineIncrement, double align)
        {
            LineIncrement = lineIncrement;
            Align = align;
            _bounds.Expand(new Vector2());
        }

        /// <summary>
        /// Adds a new span line to the span.
        /// </summary>
        /// <param name="span">The span.</param>
        public void Add(ISpan span)
        {
            double y = LineIncrement * _spans.Count;
            _spans.Add(span);
            _bounds.Expand(new Vector2(0, y + span.Bounds.Bounds.Top), new Vector2(span.Bounds.Bounds.Width, y + span.Bounds.Bounds.Bottom));
        }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            double y = offset.Y;
            foreach (var span in _spans)
            {
                // Deal with X-direction alignment
                double x = offset.X;
                if (Align.IsZero())
                    x += (_bounds.Bounds.Width - span.Bounds.Bounds.Width) * 0.5 - span.Bounds.Bounds.Left;
                else if (Align < 0)
                    x += _bounds.Bounds.Right - span.Bounds.Bounds.Right;
                else
                    x -= span.Bounds.Bounds.Left;
                span.Update(new Vector2(x, y));
                y += LineIncrement;
            }
        }

        /// <inheritdoc />
        public IEnumerator<ISpan> GetEnumerator() => ((IEnumerable<ISpan>)_spans).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_spans).GetEnumerator();
    }

    /// <summary>
    /// A span of content that is underlined
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="OverlineSpan"/>.
    /// </remarks>
    /// <param name="base">The base.</param>
    /// <param name="margin">The margin.</param>
    /// <param name="thickness">The thickness.</param>
    public class OverlineSpan(ISpan @base, double margin, double thickness) : ISpan
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        public ISpan Base { get; } = @base;

        /// <inheritdoc />
        public SpanBounds Bounds { get; } = new SpanBounds(new Bounds(
                @base.Bounds.Bounds.Left,
                @base.Bounds.Bounds.Top - margin - thickness,
                @base.Bounds.Bounds.Right,
                @base.Bounds.Bounds.Bottom), @base.Bounds.Advance);

        /// <summary>
        /// Gets the margin.
        /// </summary>
        public double Margin { get; } = margin;

        /// <summary>
        /// Gets the thickness.
        /// </summary>
        public double Thickness { get; } = thickness;

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public Vector2 Offset { get; private set; }

        /// <summary>
        /// Gets the starting point of the overline.
        /// </summary>
        public Vector2 Start { get; private set; }

        /// <summary>
        /// Gets the ending point of the overline.
        /// </summary>
        public Vector2 End { get; private set; }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            Base.Update(offset);
            Offset = offset;

            double y = offset.Y + Base.Bounds.Bounds.Top - Margin - Thickness * 0.5;
            double x1 = offset.X + Base.Bounds.Bounds.Left;
            double x2 = offset.X + Base.Bounds.Bounds.Right;
            Start = new(x1, y);
            End = new(x2, y);
        }
    }

    /// <summary>
    /// A span of content that is underlined
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="UnderlineSpan"/>.
    /// </remarks>
    /// <param name="base">The base.</param>
    /// <param name="margin">The margin.</param>
    /// <param name="thickness">The thickness.</param>
    public class UnderlineSpan(ISpan @base, double margin, double thickness) : ISpan
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        public ISpan Base { get; } = @base;

        /// <inheritdoc />
        public SpanBounds Bounds { get; } = new SpanBounds(new Bounds(
                @base.Bounds.Bounds.Left,
                @base.Bounds.Bounds.Top,
                @base.Bounds.Bounds.Right,
                @base.Bounds.Bounds.Bottom + margin + thickness), @base.Bounds.Advance);

        /// <summary>
        /// Gets the margin.
        /// </summary>
        public double Margin { get; } = margin;

        /// <summary>
        /// Gets the thickness.
        /// </summary>
        public double Thickness { get; } = thickness;

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public Vector2 Offset { get; private set; }

        /// <summary>
        /// Gets the starting point of the overline.
        /// </summary>
        public Vector2 Start { get; private set; }

        /// <summary>
        /// Gets the ending point of the overline.
        /// </summary>
        public Vector2 End { get; private set; }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            Base.Update(offset);
            Offset = offset;

            double y = offset.Y + Base.Bounds.Bounds.Bottom + Margin + Thickness * 0.5;
            double x1 = offset.X + Base.Bounds.Bounds.Left;
            double x2 = offset.X + Base.Bounds.Bounds.Right;
            Start = new(x1, y);
            End = new(x2, y);
        }
    }
}
