using SimpleCircuit.Drawing;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// Bounds for a text span.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="SpanBounds"/>.
    /// </remarks>
    /// <param name="bounds">The bounds.</param>
    /// <param name="advance">The advance width.</param>
    public readonly struct SpanBounds(Bounds bounds, double advance)
    {
        /// <summary>
        /// Gets the bounds.
        /// </summary>
        public Bounds Bounds { get; } = bounds;

        /// <summary>
        /// Gets the advance width.
        /// </summary>
        public double Advance { get; } = advance;
    }
}
