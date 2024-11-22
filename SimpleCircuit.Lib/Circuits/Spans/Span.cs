using SimpleCircuit.Drawing;

namespace SimpleCircuit.Circuits.Spans
{
    /// <summary>
    /// Describes an abstract text span.
    /// </summary>
    public abstract class Span
    {
        private SpanBounds _bounds;
        private bool _invalidated = true;

        /// <summary>
        /// Gets or sets whether the span is invalidated.
        /// </summary>
        protected bool Invalidated { get => _invalidated; set => _invalidated |= value; }

        /// <summary>
        /// Gets the bounds of the span.
        /// </summary>
        public SpanBounds Bounds
        {
            get
            {
                if (Invalidated)
                {
                    _bounds = ComputeBounds();
                    _invalidated = false;
                }
                return _bounds;
            }
        }

        /// <summary>
        /// Gets the offset of the span.
        /// </summary>
        public Vector2 Offset { get; protected set; }

        /// <summary>
        /// Compute the bounds of the span.
        /// </summary>
        /// <returns>The bounds.</returns>
        protected abstract SpanBounds ComputeBounds();

        /// <summary>
        /// Sets the offset of the span (and any child spans inside).
        /// </summary>
        /// <param name="offset">The offset.</param>
        public abstract void SetOffset(Vector2 offset);
    }
}
