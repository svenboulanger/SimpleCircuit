namespace SimpleCircuit
{
    /// <summary>
    /// Describes options for parsing SimpleCircuit.
    /// </summary>
    public class Options
    {
        private enum Styles
        {
            ANSI,
            EIC
        }
        private Styles _style = Styles.ANSI;

        [Description("Use ANSI style symbols.")]
        public bool ANSI { get => _style == Styles.ANSI; set => _style = Styles.ANSI; }

        [Description("Use IEC style symbols.")]
        public bool IEC { get => _style == Styles.EIC; set => _style = Styles.EIC; }

        [Description("Use symbols for electrical installations in Belgium (Algemeen Reglement op Elektrische Installaties).")]
        public bool AREI { get; set; }

        [Description("Whether to use the packaged variant of transistors. The default is false.")]
        public bool PackagedTransistors { get; set; } = false;

        [Description("The minimum wire length used for wires when no minimum or fixed length is specified. The default is 10.")]
        public double MinimumWireLength { get; set; } = 10.0;

        [Description("Horizontal inter-pin spacing for black-box components. The default is 30.")]
        public double HorizontalPinSpacing { get; set; } = 30.0;

        [Description("Vertical inter-pin spacing for black-box components. The default is 20.")]
        public double VerticalPinSpacing { get; set; } = 20.0;

        [Description("The default scale for any amplifier created. The default is 1.")]
        public double Scale { get; set; } = 1.0;

        [Description("Uses polarized capacitor symbols. False by default.")]
        public bool PolarCapacitors { get; set; } = false;

        [Description("Use small-signal model related symbols.")]
        public bool SmallSignal { get; set; }

        [Description("The margin along the border of the drawing.")]
        public double Margin { get; set; } = 1.0;

        [Description("Removes SVG groups that are empty.")]
        public bool RemoveEmptyGroups { get; set; } = true;

        [Description("The spacing between lines of text.")]
        public double LineSpacing { get; set; } = 1.0;
    }
}
