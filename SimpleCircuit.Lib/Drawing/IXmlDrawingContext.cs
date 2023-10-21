namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Describes a context that can be used when drawing XML.
    /// </summary>
    public interface IXmlDrawingContext
    {
        /// <summary>
        /// Checks whether the a variant is specified.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns>Returns <c>true</c> if the variant is defined; otherwise, <c>false</c>.</returns>
        bool HasVariant(string variant);

        /// <summary>
        /// Transforms a string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The transformed input.</returns>
        string TransformText(string input);
    }
}
