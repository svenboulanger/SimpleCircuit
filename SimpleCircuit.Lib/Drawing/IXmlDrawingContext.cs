using SimpleCircuit.Parser.Variants;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Describes a context that can be used when drawing XML.
    /// </summary>
    public interface IXmlDrawingContext : IVariantContext
    {
        /// <summary>
        /// Transforms a string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The transformed input.</returns>
        string TransformText(string input);
    }
}
