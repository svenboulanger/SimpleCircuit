﻿using SimpleCircuit.Drawing;
using System;
using System.Collections;
using System.Linq;

namespace SimpleCircuit.Components.Labeling
{
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
        public Vector2 CalculateSize(IEllipseDrawable subject, Vector2 spacing)
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
                string name = label.Location ?? i.ToString();
                var bounds = label.Formatted.Bounds.Bounds;
                
                switch (name)
                {
                    case "0":
                    case "c":
                    case "ci": Expand(0, bounds); break;

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

            if (width > 0)
                width += subject.LabelMargin * 2;
            if (height > 0)
                height += subject.LabelMargin * 2;

            // Calculate the ellipse size
            return new Vector2(width, height) * Math.Sqrt(2.0);
        }

        /// <inheritdoc />
        public override bool TryCalculate(IEllipseDrawable subject, string name, out LabelAnchorPoint value)
        {
            switch (name)
            {
                case "0":
                case "c":
                case "ci":
                    value = new(subject.Center, new(), subject.Appearance);
                    return true; // Center

                case "1":
                case "nw":
                case "nwo":
                case "nnw":
                case "nnwo":
                case "wnw":
                case "wnwo":
                    Vector2 pt = new(-subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                    Vector2 n = new(pt.Y, pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n, subject.Appearance);
                    return true; // Top-left

                case "2":
                case "n":
                case "no":
                case "u":
                case "up":
                    value = new(subject.Center + new Vector2(0, -subject.RadiusY - subject.LabelMargin), new(0, -1), subject.Appearance);
                    return true; // Top

                case "3":
                case "ne":
                case "neo":
                case "ene":
                case "eneo":
                case "nne":
                case "nneo":
                    pt = new(subject.RadiusX * 0.70710678118, -subject.RadiusY * 0.70710678118);
                    n = new(-pt.Y, -pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n, subject.Appearance);
                    return true; // Top-right

                case "4":
                case "e":
                case "eo":
                case "r":
                case "right":
                    value = new(subject.Center + new Vector2(subject.RadiusX + subject.LabelMargin, 0), new(1, 0), subject.Appearance);
                    return true; // Right

                case "5":
                case "se":
                case "seo":
                case "sse":
                case "sseo":
                case "ese":
                case "eseo":
                    pt = new(subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                    n = new(pt.Y, pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n, subject.Appearance);
                    return true; // Bottom-right

                case "6":
                case "s":
                case "so":
                case "d":
                case "down":
                    value = new(subject.Center + new Vector2(0, subject.RadiusY + subject.LabelMargin), new(0, 1), subject.Appearance);
                    return true; // Bottom

                case "7":
                case "sw":
                case "swo":
                case "ssw":
                case "sswo":
                case "wsw":
                case "wswo":
                    pt = new(-subject.RadiusX * 0.70710678118, subject.RadiusY * 0.70710678118);
                    n = new(-pt.Y, -pt.X);
                    n /= n.Length;
                    value = new(subject.Center + pt + subject.LabelMargin * n, n, subject.Appearance);
                    return true; // Bottom-left

                case "8":
                case "w":
                case "wo":
                case "l":
                case "left":
                    value = new(subject.Center + new Vector2(-subject.RadiusX - subject.LabelMargin, 0), new(-1, 0), subject.Appearance);
                    return true; // Left

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
