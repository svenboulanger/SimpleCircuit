using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A parsing context.
    /// </summary>
    public class ParsingContext
    {
        private readonly struct SectionInfo
        {
            public string Name { get; }
            public int WireCount { get; }
            public SectionInfo(string name, int wireCount)
            {
                Name = name;
                WireCount = wireCount;
            }
        }
        private readonly Stack<SectionInfo> _sections = new();
        private readonly Queue<ComponentInfo> _queuedPoints = new();

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
        /// Gets the queued point count.
        /// </summary>
        public int QueuedPointCount { get; private set; } = 0;

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
        /// Gets the defined sections until now.
        /// </summary>
        public Dictionary<string, Token> SectionTemplates { get; } = new();

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        public ParsingContext()
        {
            Factory.RegisterAssembly(typeof(ParsingContext).Assembly);

            // Link the circuit options to the actual circuit
            Options.SpacingXChanged += (sender, args) => Circuit.SpacingX = Options.SpacingX;
            Options.SpacingYChanged += (sender, args) => Circuit.SpacingY = Options.SpacingY;
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="fullname">The full name of the drawable.</param>
        /// <param name="options">Options that can be used for the component.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <returns>The component, or <c>null</c> if no drawable could be created.</returns>
        public IDrawable GetOrCreate(string fullname, Options options, IDiagnosticHandler diagnostics)
        {
            IDrawable result;
            if (Circuit.TryGetValue(fullname, out var presence) && presence is IDrawable drawable)
                return drawable;
            result = Factory.Create(fullname, options, diagnostics);
            if (result != null)
                Circuit.Add(result);
            return result;
        }

        /// <summary>
        /// Pushes a new section.
        /// </summary>
        /// <param name="name"></param>
        public void PushSection(string name)
        {
            _sections.Push(new(name, WireCount));
            WireCount = 0;
        }

        /// <summary>
        /// Pops the last section.
        /// </summary>
        public string PopSection()
        {
            if (_sections.Count > 0)
            {
                var section = _sections.Pop();
                WireCount = section.WireCount;
                return section.Name;
            }
            return null;
        }

        /// <summary>
        /// Gets the full name based on the current section stack.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The full name.</returns>
        public string GetFullname(string name)
            => string.Join(DrawableFactoryDictionary.Separator, _sections.Select(s => s.Name).Reverse().Union(new[] { name }));

        /// <summary>
        /// Gets the full name of a wire.
        /// </summary>
        /// <returns>The wire name.</returns>
        public string GetWireFullname()
            => GetFullname($"w{DrawableFactoryDictionary.AnonymousSeparator}{++WireCount}");

        /// <summary>
        /// Creats a queued anonymous point for later reuse.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The created queued point.</returns>
        public ComponentInfo CreateQueuedPoint(Token source)
        {
            var component = new ComponentInfo(source, GetFullname($"X{DrawableFactoryDictionary.AnonymousSeparator}Q{++QueuedPointCount}"));
            _queuedPoints.Enqueue(component);
            return component;
        }

        /// <summary>
        /// Gets a queued anonymous point or creates a new one.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The point component.</returns>
        public ComponentInfo GetOrCreateAnonymousPoint(Token source)
        {
            if (_queuedPoints.Count > 0)
            {
                var c = _queuedPoints.Dequeue();
                return new ComponentInfo(source, c.Fullname);
            }
            return new ComponentInfo(source, GetFullname("X"));
        }

        /// <summary>
        /// Generates diagnostic messages for left-over queued points.
        /// </summary>
        /// <param name="diagnostics">The diagnostic handler.</param>
        public void CheckQueuedPoints(IDiagnosticHandler diagnostics)
        {
            foreach (var pt in _queuedPoints)
                diagnostics?.Post(pt.Name, ErrorCodes.LeftOverAnonymousPoints);
        }
    }
}
