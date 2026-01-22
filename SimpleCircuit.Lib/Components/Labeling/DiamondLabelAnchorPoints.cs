using SimpleCircuit.Drawing.Styles;
using System;
using System.Linq;
using static SimpleCircuit.Components.CommonGraphical;

namespace SimpleCircuit.Components.Labeling;

/// <summary>
/// Label anchor points for a diamond shape.
/// </summary>
public class DiamondLabelAnchorPoints : LabelAnchorPoints<IBoxDrawable>
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
    public override bool TryGetAnchorIndex(string name, out int index)
    {
        switch (name.ToLower())
        {
            case "0":
            case "c":
            case "ci":
                index = 0;
                return true; // Center

            case "1":
            case "nw":
            case "nwo":
            case "nnw":
            case "nnwo":
            case "wnw":
            case "wnwo":
                index = 1;
                return true; // Top-left outside

            case "2":
            case "n":
            case "no":
            case "u":
            case "up":
                index = 2;
                return true; // Top

            case "3":
            case "ne":
            case "neo":
            case "ene":
            case "eneo":
            case "nne":
            case "nneo":
                index = 3;
                return true; // Top-right outside

            case "4":
            case "e":
            case "eo":
            case "r":
            case "right":
                index = 4;
                return true; // Right

            case "5":
            case "se":
            case "seo":
            case "sse":
            case "sseo":
            case "ese":
            case "eseo":
                index = 5;
                return true; // Bottom-right outside

            case "6":
            case "s":
            case "so":
            case "d":
            case "down":
                index = 6;
                return true; // Bottom

            case "7":
            case "sw":
            case "swo":
            case "ssw":
            case "sswo":
            case "wsw":
            case "wswo":
                index = 7;
                return true; // Bottom-left outside

            case "8":
            case "w":
            case "wo":
            case "l":
            case "left":
                index = 8;
                return true; // Left

            case "9":
            case "nwi":
            case "nnwi":
            case "wnwi":
                index = 9;
                return true; // Top-left inside

            case "10":
            case "ni":
                index = 10;
                return true; // Top inside

            case "11":
            case "nei":
            case "enei":
            case "nnei":
                index = 11;
                return true; // Top-right inside

            case "12":
            case "ei":
                index = 12;
                return true; // Right inside

            case "13":
            case "sei":
            case "ssei":
            case "esei":
                index = 13;
                return true; // Bottom-right outside

            case "14":
            case "si":
                index = 14;
                return true; // Bottom inside

            case "15":
            case "swi":
            case "sswi":
            case "wswi":
                index = 15;
                return true; // Bottom-left outside

            case "16":
            case "wi":
                index = 16;
                return true; // Left inside

            default:
                if (name.All(char.IsDigit))
                {
                    index = int.Parse(name);
                    index %= Count;
                    if (index < 0)
                        index += Count;
                    return true;
                }
                break;
        }
        index = -1;
        return false;
    }

    /// <inheritdoc />
    public override LabelAnchorPoint GetAnchorPoint(IBoxDrawable subject, int index, IStyle style)
    {
        index %= Count;
        if (index < 0)
            index += Count;

        Vector2 c = subject.InnerBounds.Center;
        Vector2 n, ox, oy;
        if (subject is IRoundedDiamond rd)
            DiamondSize(subject.OuterBounds.Width, subject.OuterBounds.Height, rd.CornerRadiusX, rd.CornerRadiusY, out n, out ox, out oy);
        else
            DiamondSize(subject.OuterBounds.Width, subject.OuterBounds.Height, 0.0, 0.0, out n, out ox, out oy);

        LabelAnchorPoint ComputeMidLocation(Vector2 a, Vector2 b)
        {
            Vector2 p = 0.5 * (a + b);
            Vector2 n = b - a;
            n = new Vector2(n.Y, -n.X);
            n /= n.Length;
            return new(p + subject.OuterMargin * n, new(n.X, n.Y));
        }

        switch (index)
        {
            case 0:
                // Center
                return new(c, new()); 

            case 1:
                // Top-left outside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopLeftLeft),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopLeftTop));

            case 2:
                // Top
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Top) + new Vector2(0, -subject.OuterMargin), new(0, -1));

            case 3:
                // Top-right outside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopRightTop),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopRightRight));

            case 4:
                // Right
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Right) + new Vector2(subject.OuterMargin, 0), new(1, 0));

            case 5:
                // Bottom-right outside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomRightRight),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomRightBottom));

            case 6:
                // Bottom
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Bottom) + new Vector2(0, subject.OuterMargin), new(0, 1));

            case 7:
                // Bottom-left outside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomLeftBottom),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomLeftLeft));

            case 8:
                // Left
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Left) + new Vector2(-subject.OuterMargin, 0), new(-1, 0));

            case 9:
                // Top-left inside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopLeftTop),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopLeftLeft));

            case 10:
                // Top inside
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Top) + new Vector2(0, subject.OuterMargin), new(0, 1));

            case 11:
                // Top-right inside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopRightRight),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.TopRightTop));

            case 12:
                // Right inside
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Right) + new Vector2(-subject.OuterMargin, 0), new(-1, 0));

            case 13:
                // Bottom-right outside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomRightBottom),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomRightRight));

            case 14:
                // Bottom inside
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Bottom) + new Vector2(0, -subject.OuterMargin), new(0, -1));

            case 15:
                // Bottom-left outside
                return ComputeMidLocation(
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomLeftLeft),
                    c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.BottomLeftBottom));

            case 16:
                // Left inside
                return new(c + GetDiamondOffset(subject.OuterBounds.Width, subject.OuterBounds.Height, ox, oy, DiamondLocation.Left) + new Vector2(subject.OuterMargin, 0), new(1, 0));

            default:
                throw new NotImplementedException();
        }
    }
}
