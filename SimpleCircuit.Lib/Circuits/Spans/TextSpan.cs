﻿namespace SimpleCircuit.Circuits.Spans
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
    public class TextSpan(string content, string fontFamily, bool isBold, double size, SpanBounds bounds) : Span
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
        /// Gets whether the text span is bold.
        /// </summary>
        public bool Bold { get; } = isBold;

        /// <summary>
        /// Gets the size of the text span.
        /// </summary>
        public double Size { get; } = size;

        /// <inheritdoc />
        protected override SpanBounds ComputeBounds() => _bounds;

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset) => Offset = offset;
    }
}