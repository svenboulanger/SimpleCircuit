using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Styles;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Labeling;

/// <summary>
/// A list of label anchor points that can be used for a circle.
/// </summary>
public class EllipseLabelAnchorPoints : LabelAnchorPoints<IEllipseDrawable>
{
    /// <summary>
    /// Gets the ellipse label anchor points.
    /// </summary>
    public static EllipseLabelAnchorPoints Default { get; } = new EllipseLabelAnchorPoints();

    /// <inheritdoc />
    public override int Count => 9;

    /// <summary>
    /// Creates a new <see cref="EllipseLabelAnchorPoints"/>.
    /// </summary>
    /// <param name="circle">The circle.</param>
    protected EllipseLabelAnchorPoints()
    {
    }

    /// <summary>
    /// Calculates the size of an ellipse that fits all the labels with given spacing.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="spacing">The spacing.</param>
    /// <returns>Returns the size as a vector.</returns>
    public Vector2 CalculateSize(IEllipseDrawable subject, Vector2 spacing, Margins margins)
    {
        double width = 0.0;
        double height = 0.0;

        void Expand(int index, Bounds bounds)
        {
            switch (index)
            {
                case 0:
                    width = Math.Max(bounds.Width, width);
                    height = Math.Max(bounds.Height, height);
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
            if (anchorIndex == 0)
                Expand(anchorIndex, bounds);
        }

        // Calculate the total width and height
        if (width > 0)
            width += margins.Horizontal;
        if (height > 0)
            height += margins.Vertical;

        // Calculate the ellipse size
        return new Vector2(width, height) * Math.Sqrt(2.0);
    }

    /// <inheritdoc />
    public override bool TryGetAnchorIndex(string name, out int index)
    {
        switch (name)
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
                return true; // Top-left

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
                return true; // Top-right

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
                return true; // Bottom-right

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
                return true; // Bottom-left

            case "8":
            case "w":
            case "wo":
            case "l":
            case "left":
                index = 8;
                return true; // Left

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

    public override LabelAnchorPoint GetAnchorPoint(IEllipseDrawable subject, int index, IStyle style)
    {
        index %= Count;
        if (index < 0)
            index += Count;

        double m = style?.LineThickness * 0.5 ?? 0.0;
        switch (index)
        {
            case 0:
                // Center
                return new(subject.Center, new());

            case 1:
                // Top-left
                Vector2 pt = new(-subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                Vector2 n = new(pt.Y, pt.X);
                n /= n.Length;
                return new(subject.Center + pt + (m + subject.OuterMargin) * n, new(n.X, n.Y));

            case 2:
                // Top
                return new(subject.Center + new Vector2(0, -subject.RadiusY - m - subject.OuterMargin), new(0, -1));

            case 3:
                // Top-right
                pt = new(subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                n = new(-pt.Y, -pt.X);
                n /= n.Length;
                return new(subject.Center + pt + (m + subject.OuterMargin) * n, new(n.X, n.Y));

            case 4:
                // Right
                return new(subject.Center + new Vector2(subject.RadiusX + m + subject.OuterMargin, 0), new(1, 0));

            case 5:
                // Bottom-right
                pt = new(subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                n = new(pt.Y, pt.X);
                n /= n.Length;
                return new(subject.Center + pt + (m + subject.OuterMargin) * n, new(n.X, n.Y));

            case 6:
                // Bottom
                return new(subject.Center + new Vector2(0, subject.RadiusY + m + subject.OuterMargin), new(0, 1));

            case 7:
                // Bottom-left
                pt = new(-subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                n = new(-pt.Y, -pt.X);
                n /= n.Length;
                return new(subject.Center + pt + (m + subject.OuterMargin) * n, new(n.X, n.Y));

            case 8:
                // Left
                return new(subject.Center + new Vector2(-subject.RadiusX - m - subject.OuterMargin, 0), new(-1, 0));

            default:
                throw new NotImplementedException();
        }
    }
}
