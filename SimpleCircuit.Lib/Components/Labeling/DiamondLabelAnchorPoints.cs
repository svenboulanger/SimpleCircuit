using System.Linq;
using static SimpleCircuit.Components.CommonGraphical;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Label anchor points for a diamond shape.
    /// </summary>
    public class DiamondLabelAnchorPoints : LabelAnchorPoints<IBoxLabeled>
    {
        /// <summary>
        /// Gets the diamond label anchor points.
        /// </summary>
        public static DiamondLabelAnchorPoints Default { get; } = new DiamondLabelAnchorPoints();

        /// <inheritdoc />
        public override int Count => 17;

        /// <summary>
        /// Creates a new <see cref="DiamondLabelAnchorPoints"/>.
        /// </summary>
        protected DiamondLabelAnchorPoints()
        {
        }

        /// <inheritdoc />
        public override bool TryCalculate(IBoxLabeled subject, string name, out LabelAnchorPoint value)
        {
            Vector2 c = 0.5 * (subject.TopLeft + subject.BottomRight);
            Vector2 size = subject.BottomRight - subject.TopLeft;
            Vector2 n, ox, oy;
            if (subject is IRoundedDiamond rd)
                RoundedDiamondSize(size.X, size.Y, rd.CornerRadiusX, rd.CornerRadiusY, out n, out ox, out oy);
            else
                RoundedDiamondSize(size.X, size.Y, 0.0, 0.0, out n, out ox, out oy);

            LabelAnchorPoint ComputeMidLocation(Vector2 a, Vector2 b)
            {
                Vector2 p = 0.5 * (a + b);
                Vector2 n = b - a;
                n = new Vector2(n.Y, -n.X);
                n /= n.Length;
                return new(p + subject.LabelMargin * n, n);
            }

            switch (name.ToLower())
            {
                case "0":
                case "c":
                case "ci":
                    value = new(c, new());
                    return true; // Center

                case "1":
                case "nw":
                case "nwo":
                case "nnw":
                case "nnwo":
                case "wnw":
                case "wnwo":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopLeftLeft),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopLeftTop));
                    return true; // Top-left outside

                case "2":
                case "n":
                case "no":
                case "u":
                case "up":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Top) + new Vector2(0, -subject.LabelMargin), new(0, -1));
                    return true; // Top

                case "3":
                case "ne":
                case "neo":
                case "ene":
                case "eneo":
                case "nne":
                case "nneo":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopRightTop),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopRightRight));
                    return true; // Top-right outside

                case "4":
                case "e":
                case "eo":
                case "r":
                case "right":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Right) + new Vector2(subject.LabelMargin, 0), new(1, 0));
                    return true; // Right

                case "5":
                case "se":
                case "seo":
                case "sse":
                case "sseo":
                case "ese":
                case "eseo":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomRightRight),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomRightBottom));
                    return true; // Bottom-right outside

                case "6":
                case "s":
                case "so":
                case "d":
                case "down":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Bottom) + new Vector2(0, subject.LabelMargin), new(0, 1));
                    return true; // Bottom

                case "7":
                case "sw":
                case "swo":
                case "ssw":
                case "sswo":
                case "wsw":
                case "wswo":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomLeftBottom),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomLeftLeft));
                    return true; // Bottom-left outside

                case "8":
                case "w":
                case "wo":
                case "l":
                case "left":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Left) + new Vector2(-subject.LabelMargin, 0), new(-1, 0));
                    return true; // Left

                case "9":
                case "nwi":
                case "nnwi":
                case "wnwi":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopLeftTop),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopLeftLeft));
                    return true; // Top-left inside

                case "10":
                case "ni":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Top) + new Vector2(0, subject.LabelMargin), new(0, 1));
                    return true; // Top inside

                case "11":
                case "nei":
                case "enei":
                case "nnei":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopRightRight),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.TopRightTop));
                    return true; // Top-right inside

                case "12":
                case "ei":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Right) + new Vector2(-subject.LabelMargin, 0), new(-1, 0));
                    return true; // Right inside

                case "13":
                case "sei":
                case "ssei":
                case "esei":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomRightBottom),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomRightRight));
                    return true; // Bottom-right outside

                case "14":
                case "si":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Bottom) + new Vector2(0, -subject.LabelMargin), new(0, -1));
                    return true; // Bottom inside

                case "15":
                case "swi":
                case "sswi":
                case "wswi":
                    value = ComputeMidLocation(
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomLeftLeft),
                        c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.BottomLeftBottom));
                    return true; // Bottom-left outside

                case "16":
                case "wi":
                    value = new(c + GetDiamondOffset(size.X, size.Y, ox, oy, DiamondLocation.Left) + new Vector2(subject.LabelMargin, 0), new(1, 0));
                    return true; // Left inside

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

            value = default;
            return false;
        }
    }
}
