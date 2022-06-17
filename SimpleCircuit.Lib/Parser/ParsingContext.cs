using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A parsing context.
    /// </summary>
    public class ParsingContext
    {
        private readonly List<Action<ParsingContext>> _postActions = new();

        /// <summary>
        /// Gets the options.
        /// </summary>
        public Options Options { get; } = new();

        /// <summary>
        /// Gets or sets the number of wires.
        /// </summary>
        public int WireCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of virtual coordinates.
        /// </summary>
        public int VirtualCoordinateCount { get; set; } = 0;

        /// <summary>
        /// Gets the factory for components.
        /// </summary>
        public DrawableFactoryDictionary Factory { get; } = new();

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public GraphicalCircuit Circuit { get; } = new GraphicalCircuit();

        /// <summary>
        /// Gets the stack of current sections. This can be used to separate parts of the circuit
        /// from each other.
        /// </summary>
        public Stack<string> Section { get; } = new Stack<string>();

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        public ParsingContext()
        {
            Factory.RegisterAssembly(typeof(ParsingContext).Assembly);
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="path">The path of the component.</param>
        /// <param name="name">The name of the component.</param>
        /// <param name="options">Options that can be used for the component.</param>
        /// <returns>The component.</returns>
        public IDrawable GetOrCreate(string fullname, Options options)
        {
            IDrawable result;
            if (Circuit.TryGetValue(fullname, out var presence) && presence is IDrawable drawable)
                return drawable;
            result = Factory.Create(fullname, options);
            if (result != null)
                Circuit.Add(result);
            return result;
        }

        /// <summary>
        /// Schedules a process to be executed after all components have been created.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void SchedulePostProcess(Action<ParsingContext> action)
        {
            _postActions.Add(action);
        }

        /// <summary>
        /// Executes all processes that need to be executed after all components have been created.
        /// </summary>
        public void FlushActions()
        {
            for (int i = 0; i < _postActions.Count; i++)
                _postActions[i].Invoke(this);
        }
    }
}
