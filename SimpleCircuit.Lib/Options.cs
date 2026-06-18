namespace SimpleCircuit;

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

    /// <summary>
    /// Gets or sets the spacing along the X-axis for unconnected circuit diagrams.
    /// </summary>
    [Description("The spacing in X-direction between two unconnected circuit diagrams. The default is 10.")]
    public double SpacingX { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets the spacing along the Y-axis for unconnected circuit diagrams.
    /// </summary>
    [Description("The spacing in Y-direction between two unconnected circuit diagrams. The default is 10.")]
    public double SpacingY { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets whether components should be kept upright.
    /// </summary>
    [Description("Decides whether components should be kept upright when possible. The default is false.")]
    public bool KeepUpright { get; set; } = false;

    /// <summary>
    /// Gets or sets whether overlapping components inside a connected group are automatically pushed apart.
    /// </summary>
    [Description("Decides whether overlapping components inside a connected group are pushed apart automatically. The default is true.")]
    public bool ResolveOverlaps { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of re-solves used to resolve overlapping components.
    /// </summary>
    [Description("The maximum number of re-solves used to push overlapping components apart. The default is 5.")]
    public int MaxOverlapIterations { get; set; } = 5;

    /// <summary>
    /// Gets or sets the gap (X-direction) that overlapping components are pushed apart to. This is
    /// also the detection threshold.
    /// </summary>
    [Description("The gap in X-direction that overlapping components are pushed apart to (and the detection threshold). The default is 5.")]
    public double OverlapMarginX { get; set; } = 5.0;

    /// <summary>
    /// Gets or sets the gap (Y-direction) that overlapping components are pushed apart to. This is
    /// also the detection threshold.
    /// </summary>
    [Description("The gap in Y-direction that overlapping components are pushed apart to (and the detection threshold). The default is 5.")]
    public double OverlapMarginY { get; set; } = 5.0;

    /// <summary>
    /// Applies the options to the given graphical circuit.
    /// </summary>
    /// <param name="circuit">The circuit.</param>
    public void Apply(GraphicalCircuit circuit)
    {
        circuit.Spacing = new(SpacingX, SpacingY);
        circuit.ResolveOverlaps = ResolveOverlaps;
        circuit.MaxOverlapIterations = MaxOverlapIterations;
        circuit.OverlapMargin = new(OverlapMarginX, OverlapMarginY);
    }
}
