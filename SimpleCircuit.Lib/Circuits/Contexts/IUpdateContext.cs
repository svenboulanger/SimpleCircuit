using SimpleCircuit.Components.Wires;
using SimpleCircuit.Diagnostics;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A context for updating the component with the solved information.
    /// </summary>
    public interface IUpdateContext
    {
        /// <summary>
        /// Gets the diagnostic handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// Gets the biasing simulation state that contains the solved unknowns.
        /// </summary>
        public IBiasingSimulationState State { get; }

        /// <summary>
        /// The wire segments defined until now.
        /// </summary>
        public List<WireSegment> WireSegments { get; }

        /// <summary>
        /// Gets the solved value.
        /// </summary>
        /// <param name="node">The node name.</param>
        /// <returns>The value.</returns>
        public double GetValue(string node);

        /// <summary>
        /// Gets the solved 2D vector value.
        /// </summary>
        /// <param name="nodeX">The x-coordinate node name.</param>
        /// <param name="nodeY">The y-coordinate node name.</param>
        /// <returns>The point.</returns>
        public Vector2 GetValue(string nodeX, string nodeY);
    }
}
