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
    /// Applies the options to the given graphical circuit.
    /// </summary>
    /// <param name="circuit">The circuit.</param>
    public void Apply(GraphicalCircuit circuit)
    {
        circuit.Spacing = new(SpacingX, SpacingY);
    }
}
