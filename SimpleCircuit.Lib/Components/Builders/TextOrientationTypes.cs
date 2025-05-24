namespace SimpleCircuit.Components.Builders
{
    /// <summary>
    /// Enumeration of text orientation types.
    /// </summary>
    public enum TextOrientationTypes
    {
        /// <summary>
        /// Regular left-to-right text within a given quadrant.
        /// </summary>
        Normal,

        /// <summary>
        /// A top-to-bottom text within a given quadrant.
        /// </summary>
        VerticalDown,

        /// <summary>
        /// A bottom-to-top text within a given quadrant.
        /// </summary>
        VertialUp,

        /// <summary>
        /// Text that transforms with the orientation.
        /// </summary>
        Transformed
    }
}
