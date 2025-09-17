using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of label anchor points that can be used for a box shape.
    /// </summary>
    public class BoxLabelAnchorPoints : LabelAnchorPoints<IBoxDrawable>
    {
        /// <summary>
        /// Gets the default box label anchor points.
        /// </summary>
        public static BoxLabelAnchorPoints Default { get; } = new BoxLabelAnchorPoints();

        /// <inheritdoc />
        public override int Count => 21;

        /// <summary>
        /// Creates a new <see cref="BoxLabelAnchorPoints"/>.
        /// </summary>
        protected BoxLabelAnchorPoints()
        {
        }

        /// <inheritdoc />
        public override LabelAnchorPoint GetAnchorPoint(IBoxDrawable subject, int index, IStyle style)
        {
            // Normalize the index
            index %= Count;
            if (index < 0)
                index += Count;

            double r = 0.0;
            if (subject is IRoundedBox rb)
                r = rb.CornerRadius;

            double m = style?.LineThickness * 0.5 ?? 0.0;
            switch (index)
            {
                case 0: // Center
                    return new(subject.InnerBounds.Shrink(m).Shrink(subject.InnerMargins).Center, Vector2.Zero, Vector2.UX, TextOrientationType.Transformed);

                case 1: // Top-left above box
                    return new(subject.OuterBounds.TopLeft + new Vector2(r, -m - subject.OuterMargin), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomLeft);

                case 2: // Top center above box
                    return new(subject.OuterBounds.TopCenter + new Vector2(0, -m - subject.OuterMargin), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomCenter);

                case 3: // Top-right above box
                    return new(subject.OuterBounds.TopRight + new Vector2(-r, -m - subject.OuterMargin), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomRight);

                case 4: // Top-right right of box
                    return new(subject.OuterBounds.TopRight + new Vector2(m + subject.OuterMargin, r), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopLeft);

                case 5: // Middle-right right of box
                    return new(subject.OuterBounds.MiddleRight + new Vector2(m + subject.OuterMargin, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.MiddleLeft);

                case 6: // Bottom-right right of box
                    return new(subject.OuterBounds.BottomRight + new Vector2(m + subject.OuterMargin, -r), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomLeft);

                case 7: // Bottom-right below box
                    return new(subject.OuterBounds.BottomRight + new Vector2(-r, m + subject.OuterMargin), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopRight);
                     
                case 8: // Bottom-center below box
                    return new(subject.OuterBounds.BottomCenter + new Vector2(0, m + subject.OuterMargin), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopCenter);

                case 9: // Bottom-left below box
                    return new(subject.OuterBounds.BottomLeft + new Vector2(r, m + subject.OuterMargin), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopLeft);

                case 10: // Bottom-left left of box
                    return new(subject.OuterBounds.BottomLeft + new Vector2(-m - subject.OuterMargin, -r), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomRight);

                case 11: // Middle-left left of box
                    return new(subject.OuterBounds.MiddleLeft + new Vector2(-m - subject.OuterMargin, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.MiddleRight);

                case 12: // Top-left left of box
                    return new(subject.OuterBounds.TopLeft + new Vector2(-m - subject.OuterMargin, r), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopRight);

                case 13: // Top-left inside box
                    return new(subject.InnerBounds.TopLeft + new Vector2(m + subject.InnerMargins.Left, m + subject.InnerMargins.Top), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopLeft);

                case 14: // Top-center inside box
                    return new(subject.InnerBounds.TopCenter + new Vector2(0, m + subject.InnerMargins.Top), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopCenter);

                case 15: // Top-right inside box
                    return new(subject.InnerBounds.TopRight + new Vector2(-m - subject.InnerMargins.Right, m + subject.InnerMargins.Top), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.TopRight);

                case 16: // Middle-right inside box
                    return new(subject.InnerBounds.MiddleRight + new Vector2(-m - subject.InnerMargins.Right, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.MiddleRight);

                case 17: // Bottom-right inside box
                    return new(subject.InnerBounds.BottomRight + new Vector2(-m - subject.InnerMargins.Right, -m - subject.InnerMargins.Bottom), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomRight);
                    
                case 18: // Bottom-center inside box
                    return new(subject.InnerBounds.BottomCenter + new Vector2(0, -m - subject.InnerMargins.Bottom), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomCenter);

                case 19: // Bottom-left inside box
                    return new(subject.InnerBounds.BottomLeft + new Vector2(m + subject.InnerMargins.Left, -m - subject.InnerMargins.Bottom), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.BottomLeft);

                case 20: // Middle-left inside box
                    return new(subject.InnerBounds.MiddleLeft + new Vector2(m + subject.InnerMargins.Left, 0), Vector2.NaN, Vector2.UX, TextOrientationType.Transformed, TextAnchor.MiddleLeft);
            }
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool TryGetAnchorIndex(string name, out int index)
        {
            switch (name)
            {
                case "0":
                case "c":
                case "ci": index = 0; return true;

                case "1":
                case "nw":
                case "nwo":
                case "nnw":
                case "nnwo": index = 1; return true;

                case "2":
                case "n":
                case "no":
                case "u":
                case "up": index = 2; return true;

                case "3":
                case "ne":
                case "neo":
                case "nne":
                case "nneo": index = 3; return true;

                case "4":
                case "ene":
                case "eneo": index = 4; return true;

                case "5":
                case "e":
                case "eo":
                case "r":
                case "right": index = 5; return true;

                case "6":
                case "ese":
                case "eseo": index = 6; return true;

                case "7":
                case "se":
                case "seo":
                case "sse":
                case "sseo": index = 7; return true;

                case "8":
                case "s":
                case "so":
                case "d":
                case "down": index = 8; return true;

                case "9":
                case "sw":
                case "swo":
                case "ssw":
                case "sswo": index = 9; return true;

                case "10":
                case "wsw":
                case "wswo": index = 10; return true;

                case "11":
                case "w":
                case "wo":
                case "l":
                case "left": index = 11; return true;

                case "12":
                case "wnw":
                case "wnwo": index = 12; return true;

                case "13":
                case "nwi": index = 13; return true;

                case "14":
                case "ni": index = 14; return true;

                case "15":
                case "nei": index = 15; return true;

                case "16":
                case "ei": index = 16; return true;

                case "17":
                case "sei": index = 17; return true;

                case "18":
                case "si": index = 18; return true;

                case "19":
                case "swi": index = 19; return true;

                case "20":
                case "wi": index = 20; return true;

                default:
                    if (name.All(char.IsDigit))
                    {
                        index = int.Parse(name);
                        index %= Count;
                        if (index < 0)
                            index += Count;
                        return true;
                    }
                    index = -1;
                    return false;
            }
        }

        /// <summary>
        /// Calculates the size of a box that fits all the labels inside with given spacing.
        /// </summary>
        /// <param name="formatter">The text formatter.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="style">The style.</param>
        /// <returns>Returns the size as a vector.</returns>
        public Bounds CalculateSize(ITextFormatter formatter, IBoxDrawable subject, IStyle style)
        {
            double leftWidth = 0.0, centerWidth = 0.0, rightWidth = 0.0;
            double topHeight = 0.0, centerHeight = 0.0, bottomHeight = 0.0;
            var margins = subject.InnerMargins;
            void Expand(int index, Bounds bounds)
            {
                switch (index)
                {
                    case 0:
                        centerWidth = Math.Max(bounds.Width + margins.Horizontal, centerWidth);
                        centerHeight = Math.Max(bounds.Height + margins.Vertical, centerHeight);
                        break;

                    case 13:
                        leftWidth = Math.Max(bounds.Width + margins.Horizontal, leftWidth);
                        topHeight = Math.Max(bounds.Height + margins.Vertical, topHeight);
                        break;

                    case 14:
                        centerWidth = Math.Max(bounds.Width + margins.Horizontal, centerWidth);
                        topHeight = Math.Max(bounds.Height + margins.Vertical, topHeight);
                        break;

                    case 15:
                        rightWidth = Math.Max(bounds.Width + margins.Horizontal, rightWidth);
                        topHeight = Math.Max(bounds.Height + margins.Vertical, topHeight);
                        break;

                    case 16:
                        rightWidth = Math.Max(bounds.Width + margins.Horizontal, rightWidth);
                        centerHeight = Math.Max(bounds.Height + margins.Vertical, centerHeight);
                        break;

                    case 17:
                        rightWidth = Math.Max(bounds.Width + margins.Horizontal, rightWidth);
                        bottomHeight = Math.Max(bounds.Height + margins.Vertical, bottomHeight);
                        break;

                    case 18:
                        centerWidth = Math.Max(bounds.Width + margins.Horizontal, centerWidth);
                        bottomHeight = Math.Max(bounds.Height + margins.Vertical, bottomHeight);
                        break;

                    case 19:
                        leftWidth = Math.Max(bounds.Width + margins.Horizontal, leftWidth);
                        bottomHeight = Math.Max(bounds.Height + margins.Vertical, bottomHeight);
                        break;

                    case 20:
                        leftWidth = Math.Max(bounds.Width + margins.Horizontal, leftWidth);
                        centerHeight = Math.Max(bounds.Height + margins.Vertical, centerHeight);
                        break;
                }
            }

            for (int i = 0; i < subject.Labels.Count; i++)
            {
                var label = subject.Labels[i];
                if (string.IsNullOrWhiteSpace(label.Value))
                    continue;
                if (label?.Formatted is null)
                    label.Format(formatter, style);
                if (!TryGetAnchorIndex(label.Anchor ?? i.ToString(), out int anchorIndex))
                    continue;
                var bounds = label.Formatted.Bounds.Bounds;

                // Expand
                if (anchorIndex == 0 || anchorIndex >= 13 && anchorIndex <= 20)
                    Expand(anchorIndex, bounds);
            }

            return new(0, 0, leftWidth + centerWidth + rightWidth, topHeight + centerHeight + bottomHeight);
        }
    }
}
