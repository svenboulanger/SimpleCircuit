using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;

namespace SimpleCircuit.Components.Constraints.SlopedMinimumConstraints
{
    /// <summary>
    /// Parameters for a <see cref="SlopedMinimumConstraint"/>.
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

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        [ParameterName("o"), ParameterName("offset")]
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Gets or sets the normal.
        /// </summary>
        [ParameterName("n"), ParameterName("normal")]
        public Vector2 Normal { get; set; }

        /// <summary>
        /// Gets or sets whether the coordinate ordering along the X-axis is inverted.
        /// </summary>
        [ParameterName("invertx")]
        public bool InvertedX { get; set; }

        /// <summary>
        /// Gets or sets whether the coordinate ordering along the Y-axis is inverted.
        /// </summary>
        [ParameterName("inverty")]
        public bool InvertedY { get; set; }
    }
}
