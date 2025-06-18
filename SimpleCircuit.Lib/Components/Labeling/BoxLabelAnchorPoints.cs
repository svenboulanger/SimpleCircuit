using SimpleCircuit.Drawing;
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
        public override LabelAnchorPoint GetAnchorPoint(IBoxDrawable subject, int index)
        {
            // Normalize the index
            index %= Count;
            if (index < 0)
                index += Count;

            double r = 0.0;
            if (subject is IRoundedBox rb)
                r = rb.CornerRadius;

            switch (index)
            {
                case 0: // Center
                    return new(subject.Center, new());

                case 1: // Top-left above box
                    return new(subject.TopLeft + new Vector2(r, -subject.LabelMargin), new(1, -1));

                case 2: // Top center above box
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y - subject.LabelMargin), new(0, -1));

                case 3: // Top-right above box
                    return new(new(subject.BottomRight.X - r, subject.TopLeft.Y - subject.LabelMargin), new(-1, -1));

                case 4: // Top-right right of box
                    return new(new(subject.BottomRight.X + subject.LabelMargin, subject.TopLeft.Y + r), new(1, 1));

                case 5: // Middle-right right of box
                    return new(new(subject.BottomRight.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0));

                case 6: // Bottom-right right of box
                    return new(subject.BottomRight + new Vector2(subject.LabelMargin, -r), new(1, -1));

                case 7: // Bottom-right below box
                    return new(subject.BottomRight + new Vector2(-r, subject.LabelMargin), new(-1, 1));
                     
                case 8: // Bottom-center below box
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y + subject.LabelMargin), new(0, 1));

                case 9: // Bottom-right below box
                    return new(new(subject.TopLeft.X + r, subject.BottomRight.Y + subject.LabelMargin), new(1, 1));

                case 10: // Bottom-left left of box
                    return new(new(subject.TopLeft.X - subject.LabelMargin, subject.BottomRight.Y - r), new(-1, -1));

                case 11: // Middle-left left of box
                    return new(new(subject.TopLeft.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0));

                case 12: // Top-left left of box
                    return new(subject.TopLeft + new Vector2(-subject.LabelMargin, r), new(-1, 1));

                case 13: // Top-left inside box
                    double f = r * 0.70710678118;
                    return new(subject.TopLeft + new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(1, 1));

                case 14: // Top-center inside box
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y + subject.LabelMargin), new(0, 1));

                case 15: // Top-right inside box
                    f = r * 0.70710678118;
                    return new(new(subject.BottomRight.X - f - subject.LabelMargin, subject.TopLeft.Y + f + subject.LabelMargin), new(-1, 1));

                case 16: // Middle-right inside box
                    return new(new(subject.BottomRight.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0));

                case 17: // Bottom-right inside box
                    f = r * 0.70710678118;
                    return new(subject.BottomRight - new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(-1, -1));
                    
                case 18: // Bottom-center inside box
                    return new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y - subject.LabelMargin), new(0, -1));

                case 19: // Bottom-left inside box
                    f = r * 0.70710678118;
                    return new(new(subject.TopLeft.X + f + subject.LabelMargin, subject.BottomRight.Y - f - subject.LabelMargin), new(1, -1));

                case 20: // Middle-left inside box
                    return new(new(subject.TopLeft.X + subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(1, 0));
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
                if (!TryGetAnchorIndex(label.Anchor ?? i.ToString(), out int anchorIndex))
                    continue;
                var bounds = label.Formatted.Bounds.Bounds;

                // Expand
                if (anchorIndex == 0 || anchorIndex >= 13 && anchorIndex <= 19)
                    Expand(anchorIndex, bounds); break;
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
    }
}
