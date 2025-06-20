﻿using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Drawing.Spans
{
    /// <summary>
    /// A span that simply adds multiple 
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="LineSpan"/>.
    /// </remarks>
    /// <param name="margin">The margin.</param>
    public class LineSpan(double margin = 0.0) : Span, IEnumerable<Span>
    {
        private readonly List<Span> _spans = [];

        /// <summary>
        /// Gets the margin between items.
        /// </summary>
        public double Margin { get; } = margin;

        /// <summary>
        /// Expands the bounds
        /// </summary>
        /// <param name="span"></param>
        public void Add(Span span)
        {
            Invalidated = true;
            _spans.Add(span);
        }

        /// <inheritdoc />
        protected override SpanBounds ComputeBounds()
        {
            var bounds = new ExpandableBounds();
            if (_spans.Count > 0)
            {
                double advance = 0.0;
                foreach (var span in _spans)
                {
                    bounds.Expand(new Vector2(advance, 0) + span.Bounds.Bounds);
                    advance += span.Bounds.Advance;
                }
                return new(bounds.Bounds, advance);
            }
            else
                return new(Drawing.Bounds.Zero, 0.0);
        }

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset)
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
        public IEnumerator<Span> GetEnumerator() => ((IEnumerable<Span>)_spans).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_spans).GetEnumerator();
    }
}
