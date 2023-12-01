using SimpleCircuit.Drawing;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// Bounds for a text span.
    /// </summary>
    public struct SpanBounds
    {
        /// <summary>
        /// Gets the bounds.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Gets the advance width.
        /// </summary>
        public double Advance { get; }

        /// <summary>
        /// Creates a new <see cref="SpanBounds"/>.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="advance">The advance width.</param>
        public SpanBounds(Bounds bounds, double advance)
        {
            Bounds = bounds;
            Advance = advance;
        }
    }
}
