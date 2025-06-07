namespace SimpleCircuit
{
    /// <summary>
    /// Describes options for parsing SimpleCircuit.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// The identifier for AREI style components.
        /// </summary>
        public const string Arei = "arei";

        /// <summary>
        /// The identifier for American style components.
        /// </summary>
        public const string American = "american";

        /// <summary>
        /// The identifier for European style components.
        /// </summary>
        public const string European = "euro";

        [Description("The spacing in X-direction between two unconnected circuit diagrams. The default is 10.")]
        public double SpacingX { get; set; } = 20.0;

        [Description("The spacing in Y-direction between two unconnected circuit diagrams. The default is 10.")]
        public double SpacingY { get; set; } = 20.0;

        [Description("If true, the graphical bounds are rendered on top of each component. The default is false.")]
        public bool RenderBounds { get; set; }

        /// <summary>
        /// Applies the options to the given graphical circuit.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        public void Apply(GraphicalCircuit circuit)
        {
            circuit.Spacing = new(SpacingX, SpacingY);
            circuit.RenderBounds = RenderBounds;
        }
    }
}
