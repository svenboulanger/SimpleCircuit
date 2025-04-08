using System;

namespace SimpleCircuit.Parser.Nodes
{
    /// <summary>
    /// An enumeration of type of constraints.
    /// </summary>
    [Flags]
    public enum VirtualChainConstraints
    {
        /// <summary>
        /// No constraint.
        /// </summary>
        None = 0,

        /// <summary>
        /// Constrains along the X-axis.
        /// </summary>
        X = 0x01,

        /// <summary>
        /// Constrains along the Y-axis.
        /// </summary>
        Y = 0x02,

        /// <summary>
        /// Constrains along both axis.
        /// </summary>
        XY = X | Y
    }
}
