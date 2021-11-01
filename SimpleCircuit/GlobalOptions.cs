namespace SimpleCircuit
{
    /// <summary>
    /// Options that can affect all components.
    /// </summary>
    public static class GlobalOptions
    {
        [Description("Whether to use the packaged variant of transistors. I.e. default value for the 'Packaged' property.")]
        public static bool PackagedTransistors { get; set; } = false;

        [Description("The minimum wire length used for wires when nothing is specified.")]
        public static double MinimumWireLength { get; set; } = 10.0;

        [Description("Horizontal inter-pin spacing for black-box components.")]
        public static double HorizontalPinSpacing { get; set; } = 30.0;

        [Description("Vertical inter-pin spacing for black-box components.")]
        public static double VerticalPinSpacing { get; set; } = 20.0;

        [Description("The default scale for any amplifier created.")]
        public static double Scale { get; set; } = 1.0;

        /// <summary>
        /// Resets all global options.
        /// </summary>
        public static void Reset()
        {
            PackagedTransistors = false;
            MinimumWireLength = 10.0;
            HorizontalPinSpacing = 30.0;
            VerticalPinSpacing = 20.0;
            Scale = 1.0;
        }
    }
}
