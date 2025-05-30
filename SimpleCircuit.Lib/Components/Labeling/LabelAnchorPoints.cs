using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Xml.Schema;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of default placements for labels.
    /// </summary>
    public abstract class LabelAnchorPoints<T> : ILabelAnchorPoints<T> where T : IDrawable
    {
        /// <inheritdoc />
        public abstract int Count { get; }

        /// <inheritdoc />
        public abstract bool TryGetAnchorIndex(string name, out int index);

        /// <inheritdoc />
        public abstract LabelAnchorPoint GetAnchorPoint(T subject, int index);

        /// <inheritdoc />
        public virtual Bounds GetBounds(T drawable, int index, ITextFormatter formatter, IStyle parentStyle)
        {
            // Start tracking bounds
            var bounds = new ExpandableBounds();
            bounds.Expand(new Vector2());

            // Go through each label and figure out the width
            var anchor = GetAnchorPoint(drawable, index);
            for (int i = 0; i < drawable.Labels.Count; i++)
            {
                // Get the location
                var label = drawable.Labels[i];
                if (!TryGetAnchorIndex(label.Anchor ?? i.ToString(), out int anchorIndex) || anchorIndex != index)
                    continue;

                // Get the bounds and expand
                if (label.Formatted is null)
                    label.Format(formatter, label.Style?.Apply(parentStyle) ?? parentStyle);
                bounds.Expand(label.Formatted.Bounds.Bounds);
            }
            return bounds.Bounds;
        }

        /// <inheritdoc />
        public void Draw(IGraphicsBuilder builder, T subject, IStyle parentStyle)
        {
            // Calculate the bounds and indices
            var anchorBounds = new Dictionary<int, (ExpandableBounds Bounds, List<Label> Labels)>();
            for (int i = 0; i < subject.Labels.Count; i++)
            {
                // Get the label
                var label = subject.Labels[i];
                if (label is null || string.IsNullOrWhiteSpace(label.Value))
                    continue;

                // Get the label anchor index
                if (!TryGetAnchorIndex(label.Anchor ?? i.ToString(), out int anchorIndex))
                    continue;

                // Compute the bounds
                var anchor = GetAnchorPoint(subject, anchorIndex);
                if (label.Formatted is null)
                    label.Format(builder.TextFormatter, parentStyle);

                // Expand the bounds and register for drawing later
                if (!anchorBounds.TryGetValue(anchorIndex, out var bounds))
                {
                    bounds = (new(), []);
                    bounds.Bounds.Expand(new Vector2());
                    anchorBounds.Add(anchorIndex, bounds);
                }
                bounds.Bounds.Expand(anchor.Orientation.TransformTextBounds(label.Formatted.Bounds.Bounds, builder.CurrentTransform));
                bounds.Labels.Add(label);

            }

            // Draw the labels
            var invMatrix = builder.CurrentTransform.Matrix.Inverse;
            foreach (var pair in anchorBounds)
            {
                // Get the anchor
                var anchor = GetAnchorPoint(subject, pair.Key);
                var bounds = pair.Value.Bounds.Bounds;

                // Determine the location based on the found bounds
                Vector2 offset;
                if (anchor.Expand.IsNaN()) // No keeping to a quadrant
                    offset = default;
                else
                {
                    // Stick to a quadrant
                    var expand = builder.CurrentTransform.ApplyDirection(anchor.Expand);
                    double x, y;
                    if (expand.X.IsZero())
                        x = -bounds.Left - 0.5 * bounds.Width;
                    else if (expand.X > 0)
                        x = -bounds.Left;
                    else
                        x = -bounds.Right;
                    if (expand.Y.IsZero())
                        y = -bounds.Top - 0.5 * bounds.Height;
                    else if (expand.Y > 0)
                        y = -bounds.Top;
                    else
                        y = -bounds.Bottom;
                    // offset = invMatrix * new Vector2(x, y);
                    offset = invMatrix * new Vector2(x, y);
                }
                foreach (var label in pair.Value.Labels)
                {
                    var loc = anchor.Location + offset + invMatrix * label.Offset;
                    builder.Text(label.Formatted, loc, anchor.Orientation);
                    // builder.Rectangle(loc.X + bounds.Left, loc.Y + bounds.Top, bounds.Width, bounds.Height, new Style { Background = Style.None, Color = "red", LineThickness = 0.1 });
                }
            }
        }

        /// <summary>
        /// Calculates the bounds of a given anchor index for a given <see cref="ILabelAnchorPoints{T}"/>.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="labels">The labels.</param>
        /// <param name="anchorIndex">The anchor index for which the bounds need to be computed.</param>
        /// <param name="anchors">The anchors.</param>
        /// <param name="parentStyle">The parent style.</param>
        /// <returns>Returns the bounds for all labels on the given anchor index.</returns>
        public static Bounds CalculateBounds(ITextFormatter formatter, Labels labels, int anchorIndex, ILabelAnchorPoints<T> anchors, IStyle parentStyle)
        {
            var bounds = new ExpandableBounds();
            bounds.Expand(new Vector2());
            for (int i = 0; i < labels.Count; i++)
            {
                // Get the label index
                var label = labels[i];
                if (!anchors.TryGetAnchorIndex(label.Anchor ?? i.ToString(), out int index) || index != anchorIndex)
                    continue;

                if (label.Formatted is null)
                    label.Format(formatter, parentStyle);
                bounds.Expand(label.Formatted.Bounds.Bounds);
            }
            return bounds.Bounds;
        }
    }
}
