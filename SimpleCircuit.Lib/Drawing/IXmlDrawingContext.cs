using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Parser.Variants;
using System.Collections.Generic;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Describes a context that can be used when drawing XML.
    /// </summary>
    public interface IXmlDrawingContext : IVariantContext
    {
        /// <summary>
        /// Gets the label anchor points, if defined.
        /// </summary>
        public IList<LabelAnchorPoint> Anchors { get; }

        /// <summary>
        /// Gets the labels, if defined.
        /// </summary>
        public Labels Labels { get; }

        /// <summary>
        /// Transforms a string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The transformed input.</returns>
        string TransformText(string input);
    }
}
