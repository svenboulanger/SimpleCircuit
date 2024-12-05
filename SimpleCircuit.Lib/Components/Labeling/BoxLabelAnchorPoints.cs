using SimpleCircuit.Drawing;
using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;

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

        /// <summary>
        /// Calculates the size of a box that fits all the labels inside with given spacing.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="spacing">The spacing.</param>
        /// <returns>Returns the size as a vector.</returns>
        public Vector2 CalculateSize(IBoxDrawable subject, Vector2 spacing)
        {
            double leftWidth = 0.0, centerWidth = 0.0, rightWidth = 0.0;
            double topHeight = 0.0, centerHeight = 0.0, bottomHeight = 0.0;
            void Expand(int index, Bounds bounds)
            {
                switch (index)
                {
                    case 0:
                        centerWidth = Math.Max(bounds.Width, centerWidth);
                        centerHeight = Math.Max(bounds.Height, centerHeight);
                        break;

                    case 13:
                        leftWidth = Math.Max(bounds.Width, leftWidth);
                        topHeight = Math.Max(bounds.Height, topHeight);
                        break;

                    case 14:
                        centerWidth = Math.Max(bounds.Width, centerWidth);
                        topHeight = Math.Max(bounds.Height, topHeight);
                        break;

                    case 15:
                        rightWidth = Math.Max(bounds.Width, rightWidth);
                        topHeight = Math.Max(bounds.Height, topHeight);
                        break;

                    case 16:
                        rightWidth = Math.Max(bounds.Width, rightWidth);
                        centerHeight = Math.Max(bounds.Height, centerHeight);
                        break;

                    case 17:
                        rightWidth = Math.Max(bounds.Width, rightWidth);
                        bottomHeight = Math.Max(bounds.Height, bottomHeight);
                        break;

                    case 18:
                        centerWidth = Math.Max(bounds.Width, centerWidth);
                        bottomHeight = Math.Max(bounds.Height, bottomHeight);
                        break;

                    case 19:
                        leftWidth = Math.Max(bounds.Width, leftWidth);
                        bottomHeight = Math.Max(bounds.Height, bottomHeight);
                        break;

                    case 20:
                        leftWidth = Math.Max(bounds.Width, leftWidth);
                        centerHeight = Math.Max(bounds.Height, centerHeight);
                        break;
                }
            }

            for (int i = 0; i < subject.Labels.Count; i++)
            {
                var label = subject.Labels[i];
                if (label?.Formatted is null)
                    continue;
                string name = label.Location ?? i.ToString();
                var bounds = label.Formatted.Bounds.Bounds;

                switch (name)
                {
                    case "0":
                    case "c":
                    case "ci": Expand(0, bounds); break;

                    case "13":
                    case "nwi": Expand(13, bounds); break;

                    case "14":
                    case "ni": Expand(14, bounds); break;

                    case "15":
                    case "nei": Expand(15, bounds); break;

                    case "16":
                    case "ei": Expand(16, bounds); break;

                    case "17":
                    case "sei": Expand(17, bounds); break;

                    case "18":
                    case "si": Expand(18, bounds); break;

                    case "19":
                    case "swi": Expand(19, bounds); break;

                    case "20":
                    case "wi": Expand(20, bounds); break;

                    default:
                        if (label.Location.All(char.IsDigit))
                        {
                            int index = int.Parse(label.Location);
                            index %= Count;
                            if (index < 0)
                                index += Count;
                            Expand(index, bounds);
                        }
                        break;
                }
            }

            // Calculate the total width and height
            double width = 0.0;
            if (leftWidth > 0)
                width += leftWidth + subject.LabelMargin;
            if (centerWidth > 0)
                width += width.Equals(0.0) ? centerWidth + subject.LabelMargin : centerWidth + spacing.X;
            if (rightWidth > 0)
                width += width.Equals(0.0) ? rightWidth + subject.LabelMargin : rightWidth + spacing.X;
            width += subject.LabelMargin;

            double height = 0.0;
            if (topHeight > 0)
                height += topHeight;
            if (centerHeight > 0)
                height += height.Equals(0.0) ? centerHeight + subject.LabelMargin : centerHeight + spacing.Y;
            if (bottomHeight > 0)
                height += height.Equals(0.0) ? bottomHeight + subject.LabelMargin : bottomHeight + spacing.Y;
            height += subject.LabelMargin;

            return new(width, height);
        }

        /// <inheritdoc />
        public override bool TryCalculate(IBoxDrawable subject, string name, out LabelAnchorPoint value)
        {
            double r = 0.0;
            if (subject is IRoundedBox rb)
                r = rb.CornerRadius;

            switch (name.ToLower())
            {
                case "0":
                case "c":
                case "ci":
                    value = new(0.5 * (subject.TopLeft + subject.BottomRight), new());
                    return true; // Center

                case "1":
                case "nw":
                case "nwo":
                case "nnw":
                case "nnwo":
                    value = new(subject.TopLeft + new Vector2(r, -subject.LabelMargin), new(1, -1));
                    return true; // Top-left above box

                case "2":
                case "n":
                case "no":
                case "u":
                case "up":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y - subject.LabelMargin), new(0, -1));
                    return true; // Top center above box

                case "3":
                case "ne":
                case "neo":
                case "nne":
                case "nneo":
                    value = new(new(subject.BottomRight.X - r, subject.TopLeft.Y - subject.LabelMargin), new(-1, -1));
                    return true; // Top-right above box

                case "4":
                case "ene":
                case "eneo":
                    value = new(new(subject.BottomRight.X + subject.LabelMargin, subject.TopLeft.Y + r), new(1, 1));
                    return true; // Top-right right of box

                case "5":
                case "e":
                case "eo":
                case "r":
                case "right":
                    value = new(new(subject.BottomRight.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0));
                    return true; // Middle-right right of box

                case "6":
                case "ese":
                case "eseo":
                    value = new(subject.BottomRight + new Vector2(subject.LabelMargin, -r), new(1, -1));
                    return true; // Bottom-right right of box

                case "7":
                case "se":
                case "seo":
                case "sse":
                case "sseo":
                    value = new(subject.BottomRight + new Vector2(-r, subject.LabelMargin), new(-1, 1));
                    return true; // Bottom-right below box

                case "8":
                case "s":
                case "so":
                case "d":
                case "down":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y + subject.LabelMargin), new(0, 1));
                    return true; // Bottom-center below box

                case "9":
                case "sw":
                case "swo":
                case "ssw":
                case "sswo":
                    value = new(new(subject.TopLeft.X + r, subject.BottomRight.Y + subject.LabelMargin), new(1, 1));
                    return true; // Bottom-right below box

                case "10":
                case "wsw":
                case "wswo":
                    value = new(new(subject.TopLeft.X - subject.LabelMargin, subject.BottomRight.Y - r), new(-1, -1));
                    return true; // Bottom-left left of box

                case "11":
                case "w":
                case "wo":
                case "l":
                case "left":
                    value = new(new(subject.TopLeft.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0));
                    return true; // Middle-left left of box

                case "12":
                case "wnw":
                case "wnwo":
                    value = new(subject.TopLeft + new Vector2(-subject.LabelMargin, r), new(-1, 1));
                    return true; // Top-left left of box

                case "13":
                case "nwi":
                    double f = r * 0.70710678118;
                    value = new(subject.TopLeft + new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(1, 1));
                    return true; // Top-left inside box

                case "14":
                case "ni":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y + subject.LabelMargin), new(0, 1));
                    return true; // Top-center inside box

                case "15":
                case "nei":
                    f = r * 0.70710678118;
                    value = new(new(subject.BottomRight.X - f - subject.LabelMargin, subject.TopLeft.Y + f + subject.LabelMargin), new(-1, 1));
                    return true; // Top-right inside box

                case "16":
                case "ei":
                    value = new(new(subject.BottomRight.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0));
                    return true; // Middle-right inside box

                case "17":
                case "sei":
                    f = r * 0.70710678118;
                    value = new(subject.BottomRight - new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(-1, -1));
                    return true; // Bottom-right inside box

                case "18":
                case "si":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y - subject.LabelMargin), new(0, -1));
                    return true; // Bottom-center inside box

                case "19":
                case "swi":
                    f = r * 0.70710678118;
                    value = new(new(subject.TopLeft.X + f + subject.LabelMargin, subject.BottomRight.Y - f - subject.LabelMargin), new(1, -1));
                    return true; // Bottom-left inside box

                case "20":
                case "wi":
                    value = new(new(subject.TopLeft.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0));
                    return true; // Middle-left inside box

                default:
                    if (name.All(char.IsDigit))
                    {
                        int index = int.Parse(name);
                        index %= Count;
                        if (index < 0)
                            index += Count;
                        return TryCalculate(subject, index.ToString(), out value);
                    }
                    break;
            }

            // Nothing found
            value = default;
            return false;
        }
    }
}
