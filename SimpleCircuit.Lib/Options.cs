namespace SimpleCircuit
{
    public class Options
    {
        [Description("Whether to use the packaged variant of transistors. The default is false.")]
        public bool PackagedTransistors { get; set; } = false;

        [Description("The minimum wire length used for wires when nothing is specified. The default is 10.")]
        public double MinimumWireLength { get; set; } = 10.0;

        [Description("Horizontal inter-pin spacing for black-box components. The default is 30.")]
        public double HorizontalPinSpacing { get; set; } = 30.0;

        [Description("Vertical inter-pin spacing for black-box components. The default is 20.")]
        public double VerticalPinSpacing { get; set; } = 20.0;

        [Description("The default scale for any amplifier created. The default is 1.")]
        public double Scale { get; set; } = 1.0;

        [Description("Uses polarized capacitors. False by default.")]
        public bool PolarCapacitors { get; set; } = false;

        [Description("Makes the crossing wires on direction (DIR) components slanted. True by default.")]
        public bool Slanted { get; set; } = true;

        [Description("Sets the number of crossings on direction (DIR) components. The default is 0.")]
        public int DirCrossings { get; set; } = 0;

        [Description("Use symbols for electrical installation diagrams.")]
        public bool ElectricalInstallation { get; set; }

        [Description("Use small-signal model related symbols.")]
        public bool SmallSignal { get; set; }

        [Description("The margin along the border of the drawing.")]
        public double Margin { get; set; } = 1.0;

        [Description("Removes SVG groups that are empty.")]
        public bool RemoveEmptyGroups { get; set; } = true;
    }
}
