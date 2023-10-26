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
    public class XmlDrawingContext : IXmlDrawingContext
    {
        private readonly VariantSet _variants;

        /// <inheritdoc />
        public IList<LabelAnchorPoint> Anchors { get; } = new List<LabelAnchorPoint>();

        /// <inheritdoc />
        public Labels Labels { get; }

        /// <summary>
        /// Creates a new <see cref="XmlDrawingContext"/>.
        /// </summary>
        /// <param name="labels">The defined labels.</param>
        /// <param name="variants">The defined variants.</param>
        public XmlDrawingContext(Labels labels, VariantSet variants)
        {
            Labels = labels ?? throw new ArgumentNullException(nameof(labels));
            _variants = variants;
        }
        
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
