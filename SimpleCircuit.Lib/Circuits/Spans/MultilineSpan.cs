using SimpleCircuit.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Spans
{
    /// <summary>
    /// A span for multiple lines.
    /// </summary>
    public class MultilineSpan(double lineIncrement, double align) : Span, IEnumerable<Span>
    {
        private readonly List<Span> _spans = [];

        /// <summary>
        /// Gets the line increment.
        /// </summary>
        public double LineIncrement { get; } = lineIncrement;

        /// <summary>
        /// Gets the alignment.
        /// </summary>
        public double Align { get; } = align;

        /// <summary>
        /// Adds a new span to the multiline.
        /// </summary>
        /// <param name="span">The span.</param>
        public void Add(Span span)
        {
            Invalidated = true;
            _spans.Add(span);
        }

        /// <inheritdoc />
        protected override SpanBounds ComputeBounds()
        {
            var bounds = new ExpandableBounds();
            double y = 0.0, advance = 0.0;
            foreach (var span in _spans)
            {
                bounds.Expand(
                    new Vector2(0, y + span.Bounds.Bounds.Top),
                    new Vector2(span.Bounds.Bounds.Width, y + span.Bounds.Bounds.Bottom));
                advance = Math.Max(advance, span.Bounds.Advance);
                y += LineIncrement;
            }
            return new(bounds.Bounds, advance);
        }

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset)
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
        public IEnumerator<Span> GetEnumerator() => ((IEnumerable<Span>)_spans).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_spans).GetEnumerator();
    }
}
