using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SimpleCircuit.Components.Constraints.MinimumConstraints
{
    /// <summary>
    /// Parameters for a <see cref="MinimumConstraint"/>.
    /// </summary>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the weight when higher than the minimum.
        /// </summary>
        [ParameterName("w"), ParameterName("weight")]
        private double _weight;

        /// <summary>
        /// Gets or sets the minimum between the two coordinates.
        /// </summary>
        [ParameterName("m"), ParameterName("minimum")]
        private double _minimum;
    }
}
