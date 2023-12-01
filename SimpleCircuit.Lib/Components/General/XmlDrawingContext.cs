using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Components.General
{
    /// <summary>
    /// A context for drawing XML drawables.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="XmlDrawingContext"/>.
    /// </remarks>
    /// <param name="labels">The defined labels.</param>
    /// <param name="variants">The defined variants.</param>
    public class XmlDrawingContext(Labels labels, VariantSet variants) : IXmlDrawingContext
    {
        private readonly VariantSet _variants = variants;

        /// <inheritdoc />
        public IList<LabelAnchorPoint> Anchors { get; } = new List<LabelAnchorPoint>();

        /// <inheritdoc />
        public Labels Labels { get; } = labels ?? throw new ArgumentNullException(nameof(labels));

        /// <inheritdoc />
        public string TransformText(string input)
        {
            // Simple substitution of \# with the corresponding label.
            return Regex.Replace(input, @"(?!<\\)\\(\d+)", match =>
            {
                int index = int.Parse(match.Groups[1].Value);
                return Labels[index]?.Value ?? "";
            });
        }

        /// <inheritdoc />
        public bool Contains(string variant) => _variants?.Contains(variant) ?? false;
    }
}
