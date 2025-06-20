using SimpleCircuit.Components.Analog;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Collections.Generic;

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
            var anchorLabels = new Dictionary<int, List<Label>>();
            for (int i = 0; i < subject.Labels.Count; i++)
            {
                // Get the label
                var label = subject.Labels[i];
                if (label is null || string.IsNullOrWhiteSpace(label.Value))
                    continue;

                // Get the label anchor index
                if (!TryGetAnchorIndex(label.Anchor ?? i.ToString(), out int anchorIndex))
                    continue;
                if (!anchorLabels.TryGetValue(anchorIndex, out var list))
                {
                    list = [];
                    anchorLabels.Add(anchorIndex, list);
                }
                list.Add(label);
            }

            foreach (var pair in anchorLabels)
            {
                var anchor = GetAnchorPoint(subject, pair.Key);
                DrawLabel(builder, anchor, pair.Value);
            }
        }

        /// <summary>
        /// Draws a number of labels for an anchor point.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="anchorPoint">The anchor.</param>
        /// <param name="labels">The labels.</param>
        public static void DrawLabel(IGraphicsBuilder builder, LabelAnchorPoint anchorPoint, List<Label> labels)
        {
            var invMatrix = builder.CurrentTransform.Matrix.Inverse;

            // If the anchor point has an expansion, first determine the global bounds
            Vector2 globalOffset = Vector2.Zero;
            if (!anchorPoint.Expand.IsNaN())
            {
                // Determine what the bounds would be if we were to layout everything
                // without transforming the text, overlaying everything at (0, 0)
                var bounds = new ExpandableBounds();
                foreach (var label in labels)
                {
                    var offset = GetOffsetFromAnchor(anchorPoint.Anchor, label.Formatted);
                    bounds.Expand(offset + label.Formatted.Bounds.Bounds);
                }

                // If the labels are transformed, let's modify the bounds to reflect that
                if ((anchorPoint.Type & TextOrientationType.Transformed) != 0)
                {
                    var orientation = builder.CurrentTransform.ApplyDirection(anchorPoint.Orientation);
                    foreach (var p in bounds.Bounds)
                        bounds.Expand(p.X * orientation + p.Y * orientation.Perpendicular);
                }

                // Determine the offset that we would need to keep to the quadrant
                var expand = builder.CurrentTransform.ApplyDirection(anchorPoint.Expand);
                double x, y;
                if (expand.X.IsZero())
                    x = -0.5 * (bounds.Left + bounds.Right);
                else if (expand.X > 0)
                    x = -bounds.Left;
                else
                    x = -bounds.Right;
                if (expand.Y.IsZero())
                    y = -0.5 * (bounds.Top + bounds.Bottom);
                else if (expand.Y > 0)
                    y = -bounds.Top;
                else
                    y = -bounds.Bottom;
                globalOffset = new Vector2(x, y);
            }

            if ((anchorPoint.Type & TextOrientationType.Transformed) != 0)
            {
                foreach (var label in labels)
                {
                    var offset = GetOffsetFromAnchor(anchorPoint.Anchor, label.Formatted);
                    offset = offset.X * anchorPoint.Orientation + offset.Y * anchorPoint.Orientation.Perpendicular;
                    builder.Text(label.Formatted, anchorPoint.Location + offset + invMatrix * (globalOffset + label.Offset), anchorPoint.Orientation, anchorPoint.Type);
                }
            }
            else
            {
                foreach (var label in labels)
                {
                    var offset = GetOffsetFromAnchor(anchorPoint.Anchor, label.Formatted);
                    offset = offset.X * anchorPoint.Orientation + offset.Y * anchorPoint.Orientation.Perpendicular;
                    builder.Text(label.Formatted, anchorPoint.Location + invMatrix * (offset + globalOffset + label.Offset), anchorPoint.Orientation, anchorPoint.Type);
                }
            }
        }

        private static Vector2 GetOffsetFromAnchor(TextAnchor anchor, Span span)
            => anchor switch
            {
                TextAnchor.Center => -span.Bounds.Bounds.Center,
                TextAnchor.MiddleLeft => -span.Bounds.Bounds.MiddleLeft,
                TextAnchor.TopLeft => -span.Bounds.Bounds.TopLeft,
                TextAnchor.TopCenter => -span.Bounds.Bounds.TopCenter,
                TextAnchor.TopRight => -span.Bounds.Bounds.TopRight,
                TextAnchor.MiddleRight => -span.Bounds.Bounds.MiddleRight,
                TextAnchor.BottomRight => -span.Bounds.Bounds.BottomRight,
                TextAnchor.BottomCenter => -span.Bounds.Bounds.BottomCenter,
                TextAnchor.BottomLeft => -span.Bounds.Bounds.BottomLeft,
                TextAnchor.Origin => Vector2.Zero,
                TextAnchor.BaselineCenter => -new Vector2(span.Bounds.Bounds.Center.X, 0),
                TextAnchor.BaselineEnd => -new Vector2(span.Bounds.Advance, 0),
                TextAnchor.TopBegin => -new Vector2(0, span.Bounds.Bounds.Top),
                TextAnchor.TopEnd => -new Vector2(span.Bounds.Advance, span.Bounds.Bounds.Top),
                TextAnchor.BottomBegin => -new Vector2(0, span.Bounds.Bounds.Bottom),
                TextAnchor.BottomEnd => -new Vector2(span.Bounds.Advance, span.Bounds.Bounds.Bottom),
                _ => throw new NotImplementedException()
            };

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
