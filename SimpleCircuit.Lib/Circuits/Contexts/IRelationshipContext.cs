using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System.Collections.Generic;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// Describes a context for discovering relationships.
    /// </summary>
    public interface IRelationshipContext
    {
        /// <summary>
        /// Gets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// Gets the current mode for discovering node relationships.
        /// </summary>
        public NodeRelationMode Mode { get; }

        /// <summary>
        /// Gets the relative fixed offsets.
        /// </summary>
        public NodeOffsetFinder Offsets { get; }

        /// <summary>
        /// Gets the extremes of the nodes.
        /// </summary>
        public Extremes Extremes { get; }

        /// <summary>
        /// Gets a set of X- and Y-coordinate node combinations.
        /// </summary>
        public HashSet<XYNode> XYSets { get; }

        /// <summary>
        /// Linked two nodes together as XY-variables.
        /// </summary>
        /// <remarks>
        /// This is used for spacing graphically distinct blocks.
        /// </remarks>
        /// <param name="x">The X-variable.</param>
        /// <param name="y">The Y-variable.</param>
        public void Link(string x, string y);
    }
}
