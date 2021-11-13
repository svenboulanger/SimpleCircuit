namespace SimpleCircuit
{
    public class Options
    {
        [Description("Whether to use the packaged variant of transistors. I.e. default value for the 'Packaged' property.")]
        public bool PackagedTransistors { get; set; } = false;

        [Description("The minimum wire length used for wires when nothing is specified.")]
        public double MinimumWireLength { get; set; } = 10.0;

        [Description("Horizontal inter-pin spacing for black-box components.")]
        public double HorizontalPinSpacing { get; set; } = 30.0;

        [Description("Vertical inter-pin spacing for black-box components.")]
        public double VerticalPinSpacing { get; set; } = 20.0;

        [Description("The default scale for any amplifier created.")]
        public double Scale { get; set; } = 1.0;

        [Description("Uses polarized capacitors by default.")]
        public bool PolarCapacitors { get; set; } = false;
    }
}
