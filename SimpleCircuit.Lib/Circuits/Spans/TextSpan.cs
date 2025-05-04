using SimpleCircuit.Components.Appearance;
using System.Text;

namespace SimpleCircuit.Circuits.Spans
{
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
    public class TextSpan(string content, string color, double opacity, string fontFamily, bool isBold, double size, SpanBounds bounds) : Span
    {
        private readonly SpanBounds _bounds = bounds;

        /// <summary>
        /// Gets the content of the text span.
        /// </summary>
        public string Content { get; } = content ?? string.Empty;

        /// <summary>
        /// Gets the font family of the text span.
        /// </summary>
        public string FontFamily { get; } = fontFamily;

        /// <summary>
        /// Gets the color.
        /// </summary>
        public string Color { get; } = color;

        /// <summary>
        /// Gets the opacity of the text span.
        /// </summary>
        public double Opacity { get; } = opacity;

        /// <summary>
        /// Gets whether the text span is bold.
        /// </summary>
        public bool Bold { get; } = isBold;

        /// <summary>
        /// Gets the size of the text span.
        /// </summary>
        public double Size { get; } = size;

        /// <summary>
        /// Gets the style of the text span for use in the style-attribute.
        /// </summary>
        public string Style
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append($"font-family: {FontFamily}; ");
                sb.Append($"font-size: {Size.ToSVG()}pt; ");
                if (Opacity.IsZero())
                    sb.Append($"fill: none; ");
                else if ((Opacity - AppearanceOptions.Opaque).IsZero() || Opacity > AppearanceOptions.Opaque)
                    sb.Append($"fill: {Color}; ");
                else
                    sb.Append($"fill: {Color}; fill-opacity: {Opacity}; ");
                if (Bold)
                    sb.Append("font-weight: bold; ");
                sb.Append("stroke: none;");
                return sb.ToString();
            }
        }

        /// <inheritdoc />
        protected override SpanBounds ComputeBounds() => _bounds;

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset) => Offset = offset;
    }
}
