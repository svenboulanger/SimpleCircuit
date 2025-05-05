using SimpleCircuit.Components.Styles;
using SimpleCircuit.Drawing;
using System.Drawing;
using System.Text;

namespace SimpleCircuit.Circuits.Spans
{
    /// <summary>
    /// A span of content that is underlined
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="OverlineSpan"/>.
    /// </remarks>
    /// <param name="base">The base.</param>
    /// <param name="margin">The margin.</param>
    /// <param name="thickness">The thickness.</param>
    public class OverlineSpan(Span @base, double margin, IStyle appearance) : Span
    {
        /// <summary>
        /// Gets the content.
        /// </summary>
        public Span Base { get; } = @base;

        /// <summary>
        /// Gets the appearance.
        /// </summary>
        public IStyle Appearance { get; } = appearance;

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
            return new SpanBounds(new Bounds(
                Base.Bounds.Bounds.Left,
                Base.Bounds.Bounds.Top - Margin - Appearance.LineThickness,
                Base.Bounds.Bounds.Right,
                Base.Bounds.Bounds.Bottom), Base.Bounds.Advance);
        }

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset)
        {
            Base.SetOffset(offset);
            Offset = offset;

            double y = offset.Y + Base.Bounds.Bounds.Top - Margin - Appearance.LineThickness * 0.5;
            double x1 = offset.X + Base.Bounds.Bounds.Left;
            double x2 = offset.X + Base.Bounds.Bounds.Right;
            Start = new(x1, y);
            End = new(x2, y);
        }
    }
}
