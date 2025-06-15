using SimpleCircuit.Circuits;
using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuit.Drawing.Spans
{
    /// <summary>
    /// A superscript span.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="SubscriptSuperscriptSpan"/>.
    /// </remarks>
    /// <param name="base">The base.</param>
    /// <param name="halfway">The height of the halfway point.</param>
    /// <remarks>
    /// Creates a new <see cref="SubscriptSuperscriptSpan"/>.
    /// </remarks>
    /// <param name="base">The base span.</param>
    /// <param name="sub">The subscript span.</param>
    /// <param name="super">The superscript span.</param>
    /// <param name="halfway">The halfway point.</param>
    /// <param name="margin">The margin to the base.</param>
    public class SubscriptSuperscriptSpan(Span @base, Span sub, Span super, double halfway, Vector2 margin) : Span
    {
        /// <summary>
        /// Gets the halfway point for the font.
        /// </summary>
        public double Halfway { get; } = halfway;

        /// <summary>
        /// Gets the margin between the spans.
        /// </summary>
        public Vector2 Margin { get; } = margin;

        /// <summary>
        /// Gets or sets the superscript.
        /// </summary>
        public Span Super { get; } = super;

        /// <summary>
        /// Gets or sets the subscript.
        /// </summary>
        public Span Sub { get; } = sub;

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        public Span Base { get; } = @base ?? throw new ArgumentNullException(nameof(@base));

        /// <inheritdoc />
        protected override SpanBounds ComputeBounds()
        {
            var bounds = new ExpandableBounds();
            double advance = Base.Bounds.Advance;
            bounds.Expand(Base.Bounds.Bounds);
            if (Sub is not null)
            {
                var subLocation = new Vector2(
                    Base.Bounds.Advance + Margin.X - Sub.Bounds.Bounds.Left,
                    -Halfway + Margin.Y * 0.5 - Sub.Bounds.Bounds.Top);
                bounds.Expand(subLocation + Sub.Bounds.Bounds);
                advance = Math.Max(subLocation.X + Sub.Bounds.Advance, advance);
            }
            if (Super is not null)
            {
                var superLocation = new Vector2(
                    Base.Bounds.Advance + Margin.X - Super.Bounds.Bounds.Left,
                    -Halfway - Margin.Y * 0.5 - Super.Bounds.Bounds.Bottom);
                bounds.Expand(superLocation + Super.Bounds.Bounds);
                advance = Math.Max(superLocation.X + Super.Bounds.Advance, advance);
            }
            return new(bounds.Bounds, advance);
        }

        /// <inheritdoc />
        public override void SetOffset(Vector2 offset)
        {
            Offset = offset;
            Base.SetOffset(offset);
            if (Sub is not null)
            {
                var subLocation = new Vector2(
                    offset.X + Base.Bounds.Advance + Margin.X - Sub.Bounds.Bounds.Left,
                    offset.Y - Halfway + Margin.Y * 0.5 - Sub.Bounds.Bounds.Top);
                Sub.SetOffset(subLocation);
            }
            if (Super is not null)
            {
                var superLocation = new Vector2(
                    offset.X + Base.Bounds.Advance + Margin.X - Super.Bounds.Bounds.Left,
                    offset.Y - Halfway - Margin.Y * 0.5 - Super.Bounds.Bounds.Bottom);
                Super.SetOffset(superLocation);
            }
        }
    }
}
