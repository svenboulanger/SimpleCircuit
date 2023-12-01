using SimpleCircuit.Drawing;
using System.Collections.Generic;
using System.Xml;

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
    public class TextSpan : ISpan
    {
        /// <summary>
        /// Gets the span element.
        /// </summary>
        public XmlElement Element { get; }

        /// <inheritdoc />
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Creates a new text span from a measured XML element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="bounds">The bounds of the element on its own.</param>
        public TextSpan(XmlElement element, SpanBounds bounds)
        {
            Element = element;
            Bounds = bounds;
        }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            Element.SetAttribute("x", offset.X.ToCoordinate());
            Element.SetAttribute("y", offset.Y.ToCoordinate());
        }
    }

    /// <summary>
    /// A span that simply adds multiple 
    /// </summary>
    public class LineSpan : ISpan
    {
        private readonly List<ISpan> _spans = new();
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
    }

    /// <summary>
    /// A superscript span.
    /// </summary>
    public class SubscriptSuperscriptSpan : ISpan
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
        public ISpan Base { get; }

        /// <summary>
        /// Gets the margin between the spans.
        /// </summary>
        public Vector2 Margin { get; set; }

        /// <summary>
        /// Gets the half-way point.
        /// </summary>
        public double Halfway { get; }

        /// <inheritdoc />
        public SpanBounds Bounds { get; private set; }

        /// <summary>
        /// Gets the location for the superscript.
        /// </summary>
        protected Vector2 SuperLocation => new(Base.Bounds.Bounds.Right + Margin.X - Super.Bounds.Bounds.Left, -Halfway - Margin.Y * 0.5 - Super.Bounds.Bounds.Bottom);

        /// <summary>
        /// Gets the location for the subscript.
        /// </summary>
        protected Vector2 SubLocation => new(Base.Bounds.Bounds.Right + Margin.X - Sub.Bounds.Bounds.Left, -Halfway + Margin.Y * 0.5 - Sub.Bounds.Bounds.Top);

        /// <summary>
        /// Creates a new <see cref="SubscriptSuperscriptSpan"/>.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="halfway">The height of the halfway point.</param>
        public SubscriptSuperscriptSpan(ISpan @base, double halfway)
        {
            Base = @base;
            Halfway = halfway;
            Bounds = @base.Bounds;
        }

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
    public class MultilineSpan : ISpan
    {
        private readonly List<ISpan> _spans = new();
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
    }

    /// <summary>
    /// A span of content that is underlined
    /// </summary>
    public class OverlineSpan : ISpan
    {
        /// <summary>
        /// Gets the XML element for the overline
        /// </summary>
        public XmlElement Element { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public ISpan Base { get; }

        /// <inheritdoc />
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Gets the margin.
        /// </summary>
        public double Margin { get; }

        /// <summary>
        /// Gets the thickness.
        /// </summary>
        public double Thickness { get; }

        /// <summary>
        /// Creates a new <see cref="OverlineSpan"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="base">The base.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="thickness">The thickness.</param>
        public OverlineSpan(XmlElement element, ISpan @base, double margin, double thickness)
        {
            Element = element;
            Base = @base;
            Margin = margin;
            Thickness = thickness;
            Bounds = new SpanBounds(new Bounds(
                @base.Bounds.Bounds.Left,
                @base.Bounds.Bounds.Top - margin - thickness,
                @base.Bounds.Bounds.Right,
                @base.Bounds.Bounds.Bottom), @base.Bounds.Advance);
        }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            Base.Update(offset);

            string y = (offset.Y + Base.Bounds.Bounds.Top - Margin - Thickness * 0.5).ToCoordinate();
            string x1 = (offset.X + Base.Bounds.Bounds.Left).ToCoordinate();
            string x2 = (offset.X + Base.Bounds.Bounds.Right).ToCoordinate();
            Element.SetAttribute("d", $"M{x1} {y} L{x2} {y}");
        }
    }


    /// <summary>
    /// A span of content that is underlined
    /// </summary>
    public class UnderlineSpan : ISpan
    {
        /// <summary>
        /// Gets the XML element for the overline
        /// </summary>
        public XmlElement Element { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public ISpan Base { get; }

        /// <inheritdoc />
        public SpanBounds Bounds { get; }

        /// <summary>
        /// Gets the margin.
        /// </summary>
        public double Margin { get; }

        /// <summary>
        /// Gets the thickness.
        /// </summary>
        public double Thickness { get; }

        /// <summary>
        /// Creates a new <see cref="UnderlineSpan"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="base">The base.</param>
        /// <param name="margin">The margin.</param>
        /// <param name="thickness">The thickness.</param>
        public UnderlineSpan(XmlElement element, ISpan @base, double margin, double thickness)
        {
            Element = element;
            Base = @base;
            Margin = margin;
            Thickness = thickness;
            Bounds = new SpanBounds(new Bounds(
                @base.Bounds.Bounds.Left,
                @base.Bounds.Bounds.Top,
                @base.Bounds.Bounds.Right,
                @base.Bounds.Bounds.Bottom + margin + thickness), @base.Bounds.Advance);
        }

        /// <inheritdoc />
        public void Update(Vector2 offset)
        {
            Base.Update(offset);

            string y = (offset.Y + Base.Bounds.Bounds.Bottom + Margin + Thickness * 0.5).ToCoordinate();
            string x1 = (offset.X + Base.Bounds.Bounds.Left).ToCoordinate();
            string x2 = (offset.X + Base.Bounds.Bounds.Right).ToCoordinate();
            Element.SetAttribute("d", $"M{x1} {y} L{x2} {y}");
        }
    }
}
