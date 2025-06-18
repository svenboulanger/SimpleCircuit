using System;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// Enumeration of text orientation type.
    /// </summary>
    [Flags]
    public enum TextOrientationType
    {
        /// <summary>
        /// Nothing special about the orientation type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the text should be kept "upright", i.e. legible without standing on your head.
        /// </summary>
        Upright = 0x01,

        /// <summary>
        /// Indicates that the text is transformed.
        /// </summary>
        Transformed = 0x02,

        /// <summary>
        /// Indicates that the text should be both transformed and upright.
        /// </summary>
        UprightTransformed = Upright | Transformed
    }
}
