using SimpleCircuit.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A subcircuit definition.
    /// </summary>
    public class SubcircuitDefinition
    {
        /// <summary>
        /// Gets the circuit definition.
        /// </summary>
        public Circuit Definition { get; }

        /// <summary>
        /// Gets the ports of the circuit.
        /// </summary>
        public List<IPin> Ports { get; } = new List<IPin>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinition"/> class.
        /// </summary>
        /// <param name="ckt"></param>
        public SubcircuitDefinition(Circuit ckt = null)
        {
            Definition = ckt ?? new Circuit();
        }
    }
}
