using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// A list of label anchor points that can be used for a box.
    /// </summary>
    public class BoxLabelAnchorPoints : LabelAnchorPoints<IBoxLabeled>
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
        public override bool TryCalculate(IBoxLabeled subject, string name, out LabelAnchorPoint value)
        {
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
                    value = new(subject.TopLeft + new Vector2(subject.CornerRadius, -subject.LabelMargin), new(1, -1));
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
                    value = new(new(subject.BottomRight.X - subject.CornerRadius, subject.TopLeft.Y - subject.LabelMargin), new(-1, -1));
                    return true; // Top-right above box

                case "4":
                case "ene":
                case "eneo":
                    value = new(new(subject.BottomRight.X + subject.LabelMargin, subject.TopLeft.Y + subject.CornerRadius), new(1, 1));
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
                    value = new(subject.BottomRight + new Vector2(subject.LabelMargin, -subject.CornerRadius), new(1, -1));
                    return true; // Bottom-right right of box

                case "7":
                case "se":
                case "seo":
                case "sse":
                case "sseo":
                    value = new(subject.BottomRight + new Vector2(-subject.CornerRadius, subject.LabelMargin), new(-1, 1));
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
                    value = new(new(subject.TopLeft.X + subject.CornerRadius, subject.BottomRight.Y + subject.LabelMargin), new(1, 1));
                    return true; // Bottom-right below box

                case "10":
                case "wsw":
                case "wswo":
                    value = new(new(subject.TopLeft.X - subject.LabelMargin, subject.BottomRight.Y - subject.CornerRadius), new(-1, -1));
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
                    value = new(subject.TopLeft + new Vector2(-subject.LabelMargin, subject.CornerRadius), new(-1, 1));
                    return true; // Top-left left of box

                case "13":
                case "nwi":
                    double f = subject.CornerRadius * 0.70710678118;
                    value = new(subject.TopLeft + new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(1, 1));
                    return true; // Top-left inside box

                case "14":
                case "ni":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.TopLeft.Y + subject.LabelMargin), new(0, 1));
                    return true; // Top-center inside box

                case "15":
                case "nei":
                    f = subject.CornerRadius * 0.70710678118;
                    value = new(new(subject.BottomRight.X - f - subject.LabelMargin, subject.TopLeft.Y + f + subject.LabelMargin), new(-1, 1));
                    return true; // Top-right inside box

                case "16":
                case "ei":
                    value = new(new(subject.BottomRight.X - subject.LabelMargin, 0.5 * (subject.TopLeft.Y + subject.BottomRight.Y)), new(-1, 0));
                    return true; // Middle-right inside box

                case "17":
                case "sei":
                    f = subject.CornerRadius * 0.70710678118;
                    value = new(subject.BottomRight - new Vector2(f + subject.LabelMargin, f + subject.LabelMargin), new(-1, -1));
                    return true; // Bottom-right inside box

                case "18":
                case "si":
                    value = new(new(0.5 * (subject.TopLeft.X + subject.BottomRight.X), subject.BottomRight.Y - subject.LabelMargin), new(0, -1));
                    return true; // Bottom-center inside box

                case "19":
                case "swi":
                    f = subject.CornerRadius * 0.70710678118;
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
