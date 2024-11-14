using SimpleCircuit.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// An interface describing a text span.
    /// </summary>
    public interface ISpan
    {
        /// <summary>
        /// Gets the offset of the span.
        /// </summary>
        public Vector2 Offset { get; }

        /// <summary>
        /// Gets the bounds of the span.
        /// </summary>
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Sets the offset of the span.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SetOffset(Vector2 offset);
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
        /// <inheritdoc />
        public SpanBounds Bounds { get; } = bounds;

        /// <inheritdoc />
        public Vector2 Offset { get; private set; }

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
        public void SetOffset(Vector2 offset) => Offset = offset;
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
        public Vector2 Offset { get; private set; }

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
        public void SetOffset(Vector2 offset)
        {
            Offset = offset;
            double x = offset.X;
            foreach (var span in _spans)
            {
                span.SetOffset(new Vector2(x, offset.Y));
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
    public class SubscriptSuperscriptSpan : ISpan
    {
        /// <summary>
        /// Gets the halfway point for the font.
        /// </summary>
        public double Halfway { get; }

        /// <inheritdoc />
        public Vector2 Offset { get; private set; }

        /// <inheritdoc />
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Gets or sets the superscript.
        /// </summary>
        public ISpan Super { get; }

        /// <summary>
        /// Gets or sets the subscript.
        /// </summary>
        public ISpan Sub { get; }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        public ISpan Base { get; }

        /// <summary>
        /// Gets the margin between the spans.
        /// </summary>
        public Vector2 Margin { get; }

        /// <summary>
        /// Creates a new <see cref="SubscriptSuperscriptSpan"/>.
        /// </summary>
        /// <param name="base">The base span.</param>
        /// <param name="sub">The subscript span.</param>
        /// <param name="super">The superscript span.</param>
        /// <param name="halfway">The halfway point.</param>
        /// <param name="margin">The margin to the base.</param>
        public SubscriptSuperscriptSpan(ISpan @base, ISpan sub, ISpan super, double halfway, Vector2 margin)
        {
            Halfway = halfway;
            Margin = margin;
            Base = @base ?? throw new ArgumentNullException(nameof(@base));

            var bounds = new ExpandableBounds();
            double advance = @base.Bounds.Advance;
            bounds.Expand(@base.Bounds.Bounds);
            if (sub is not null)
            {
                var subLocation = new Vector2(
                    @base.Bounds.Bounds.Right + Margin.X - Sub.Bounds.Bounds.Left,
                    -halfway + Margin.Y * 0.5 - Sub.Bounds.Bounds.Top);
                bounds.Expand(subLocation + sub.Bounds.Bounds);
                advance = Math.Max(subLocation.X + sub.Bounds.Advance, advance);
            }
            if (super is not null)
            {
                var superLocation = new Vector2(
                    @base.Bounds.Bounds.Right + Margin.X - Super.Bounds.Bounds.Left,
                    -halfway - Margin.Y * 0.5 - Super.Bounds.Bounds.Bottom);
                bounds.Expand(superLocation + super.Bounds.Bounds);
                advance = Math.Max(superLocation.X + super.Bounds.Advance, advance);
            }
            Bounds = new(bounds.Bounds, advance);
        }

        /// <inheritdoc />
        public void SetOffset(Vector2 offset)
        {
            Offset = offset;
            Base.SetOffset(offset);
            if (Sub is not null)
            {
                var subLocation = new Vector2(
                    offset.X + Base.Bounds.Bounds.Right + Margin.X - Sub.Bounds.Bounds.Left,
                    offset.Y - Halfway + Margin.Y * 0.5 - Sub.Bounds.Bounds.Top);
                Sub.SetOffset(subLocation);
            }
            if (Super is not null)
            {
                var superLocation = new Vector2(
                    offset.X + Base.Bounds.Bounds.Right + Margin.X - Super.Bounds.Bounds.Left,
                    offset.Y - Halfway - Margin.Y * 0.5 - Super.Bounds.Bounds.Bottom);
                Super.SetOffset(superLocation);
            }
        }
    }

    /// <summary>
    /// A span for multiple lines.
    /// </summary>
    public class MultilineSpan : ISpan, IEnumerable<ISpan>
    {
        private readonly List<ISpan> _spans;

        /// <summary>
        /// Gets the line increment.
        /// </summary>
        public double LineIncrement { get; }

        /// <summary>
        /// Gets the alignment.
        /// </summary>
        public double Align { get; }

        /// <inheritdoc />
        public Vector2 Offset { get; private set; }

        /// <inheritdoc />
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Creates a new <see cref="MultilineSpan"/>.
        /// </summary>
        /// <param name="lineIncrement">The increment in y-direction for each span.</param>
        public MultilineSpan(IEnumerable<ISpan> spans, double lineIncrement, double align)
        {
            _spans = spans?.ToList() ?? throw new ArgumentNullException(nameof(spans));
            LineIncrement = lineIncrement;
            Align = align;

            var bounds = new ExpandableBounds();
            double y = 0.0, advance = 0.0;
            foreach (var span in spans)
            {
                bounds.Expand(
                    new Vector2(0, y + span.Bounds.Bounds.Top), 
                    new Vector2(span.Bounds.Bounds.Width, y + span.Bounds.Bounds.Bottom));
                advance = Math.Max(advance, span.Bounds.Advance);
                y += LineIncrement;
            }
            Bounds = new(bounds.Bounds, advance);
        }

        /// <inheritdoc />
        public void SetOffset(Vector2 offset)
        {
            Offset = offset;
            double y = offset.Y;
            foreach (var span in _spans)
            {
                // Deal with X-direction alignment
                double x = offset.X;
                if (Align.IsZero())
                    x += (Bounds.Bounds.Width - span.Bounds.Bounds.Width) * 0.5 - span.Bounds.Bounds.Left;
                else if (Align < 0)
                    x += Bounds.Bounds.Right - span.Bounds.Bounds.Right;
                else
                    x -= span.Bounds.Bounds.Left;
                span.SetOffset(new Vector2(x, y));
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

        /// <inheritdoc />
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
        public void SetOffset(Vector2 offset)
        {
            Base.SetOffset(offset);
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
        public void SetOffset(Vector2 offset)
        {
            Base.SetOffset(offset);
            Offset = offset;

            double y = offset.Y + Base.Bounds.Bounds.Bottom + Margin + Thickness * 0.5;
            double x1 = offset.X + Base.Bounds.Bounds.Left;
            double x2 = offset.X + Base.Bounds.Bounds.Right;
            Start = new(x1, y);
            End = new(x2, y);
        }
    }
}
