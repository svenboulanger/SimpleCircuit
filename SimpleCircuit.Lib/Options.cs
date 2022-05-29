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

        [Description("If true, use ANSI style symbols.")]
        public bool ANSI { get => _style == Styles.ANSI; set => _style = Styles.ANSI; }

        [Description("If true, use IEC style symbols.")]
        public bool IEC { get => _style == Styles.EIC; set => _style = Styles.EIC; }

        [Description("If true, some components will use symbols for electrical installations in Belgium (Algemeen Reglement op Elektrische Installaties).")]
        public bool AREI { get; set; }

        [Description("If true, transistors will by default show as packaged transistors.")]
        public bool PackagedTransistors { get; set; } = false;

        [Description("The minimum wire length used for wires when no minimum or fixed length is specified.")]
        public double MinimumWireLength { get; set; } = 10.0;

        [Description("Horizontal inter-pin spacing for black-box components.")]
        public double HorizontalPinSpacing { get; set; } = 30.0;

        [Description("Vertical inter-pin spacing for black-box components.")]
        public double VerticalPinSpacing { get; set; } = 20.0;

        [Description("The default scale for any amplifier created. The default is 1.")]
        public double Scale { get; set; } = 1.0;

        [Description("If true, capacitors will show polarized capacitor symbols.")]
        public bool PolarCapacitors { get; set; } = false;

        [Description("If true, some components will use small-signal model related symbols.")]
        public bool SmallSignal { get; set; }

        [Description("The margin along the border of the drawing.")]
        public double Margin { get; set; } = 1.0;

        [Description("If true, removes any empty groups in the SVG output.")] 
        public bool RemoveEmptyGroups { get; set; } = true;

        [Description("The spacing between lines of text.")]
        public double LineSpacing { get; set; } = 1.0;

        [Description("If true, wires will draw small arcs indicating jumping over another wire.")]
        public bool JumpOverWires { get; set; } = false;
    }
}
