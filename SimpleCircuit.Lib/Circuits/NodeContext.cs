using SimpleCircuit.Circuits;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// The context used for collecting information about nodes in a
    /// graphical circuit.
    /// </summary>
    public class NodeContext
    {
        /// <summary>
        /// Gets the current mode for discovering node relationships.
        /// </summary>
        public NodeRelationMode Mode { get; set; }

        /// <summary>
        /// Gets the graphical circuit defining the nodes.
        /// </summary>
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Gets the relative fixed offsets.
        /// </summary>
        public NodeOffsetFinder Offsets { get; } = new();

        /// <summary>
        /// Gets the extremes of the nodes.
        /// </summary>
        public Extremes Extremes { get; } = new();

        /// <summary>
        /// Gets a set of X- and Y-coordinate node combinations.
        /// </summary>
        public HashSet<XYNode> XYSets { get; } = new();

        /// <summary>
        /// Linked two nodes together as XY-variables.
        /// </summary>
        /// <remarks>
        /// This is used for spacing graphically distinct blocks.
        /// </remarks>
        /// <param name="x">The X-variable.</param>
        /// <param name="y">The Y-variable.</param>
        public void Link(string x, string y)
        { 
            string repX = Extremes.Linked[Offsets[x].Representative];
            string repY = Extremes.Linked[Offsets[y].Representative];
            XYSets.Add(new(repX, repY));
        }

        /// <summary>
        /// Gets the value from the solver for the specified node.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="node">The node.</param>
        /// <returns>The value.</returns>
        public double GetValue(IBiasingSimulationState state, string node)
        {
            var r = Offsets[node];
            if (state.TryGetValue(r.Representative, out var value))
                return value.Value + r.Offset;
            return r.Offset;
        }

        /// <summary>
        /// Gets the value from the solver for the specified set of nodes.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="nodeX">The node for the X-coordinate.</param>
        /// <param name="nodeY">The node for the Y-coordinate.</param>
        /// <returns>The location.</returns>
        public Vector2 GetValue(IBiasingSimulationState state, string nodeX, string nodeY)
            => new(GetValue(state, nodeX), GetValue(state, nodeY));
    }
}
