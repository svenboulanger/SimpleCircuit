using System;

namespace SimpleCircuit.Drawing.Builders
{
    /// <summary>
    /// Enumeration of text orientation types.
    /// </summary>
    [Flags]
    public enum TextOrientationTypes
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
        Transformed = 0x02
    }
}
