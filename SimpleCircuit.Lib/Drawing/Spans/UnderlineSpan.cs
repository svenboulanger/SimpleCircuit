using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Drawing.Spans
{
    /// <summary>
    /// A span of content that is underlined
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="UnderlineSpan"/>.
    /// </remarks>
    /// <param name="base">The base.</param>
    /// <param name="margin">The margin.</param>
    /// <param name="style">The style.</param>
    public class UnderlineSpan(Span @base, double margin, IStyle style) : Span
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        public Span Base { get; } = @base;

        /// <summary>
        /// Gets the appearance.
        /// </summary>
        public IStyle Style => style;

        /// <summary>
        /// Gets the margin.
        /// </summary>
        public double Margin { get; } = margin;

        /// <summary>
        /// Gets the starting point of the overline.
        /// </summary>
        public Vector2 Start { get; private set; }

        /// <summary>
        /// Gets the ending point of the overline.
        /// </summary>
        public Vector2 End { get; private set; }

        /// <inheritdoc />
        protected override SpanBounds ComputeBounds()
        {
            return new(new Bounds(
                Base.Bounds.Bounds.Left,
                Base.Bounds.Bounds.Top,
                Base.Bounds.Bounds.Right,
                Base.Bounds.Bounds.Bottom + Margin + style.LineThickness), Base.Bounds.Advance);
        }

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset)
        {
            Base.SetOffset(offset);
            Offset = offset;

            double y = offset.Y + Base.Bounds.Bounds.Bottom + Margin + style.LineThickness * 0.5;
            double x1 = offset.X + Base.Bounds.Bounds.Left;
            double x2 = offset.X + Base.Bounds.Bounds.Right;
            Start = new(x1, y);
            End = new(x2, y);
        }
    }
}
