﻿using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Annotations;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.Nodes;
using SimpleCircuit.Parser.SimpleTexts;
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
        private readonly struct SectionInfo(string name, int wireCount)
        {
            public string Name { get; } = name;
            public int WireCount { get; } = wireCount;
        }
        private readonly Stack<SectionInfo> _sections = new();
        // private readonly Queue<ComponentInfo> _queuedPoints = new();
        private readonly Stack<IAnnotation> _annotations = new();

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
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Gets the defined sections until now.
        /// </summary>
        public Dictionary<string, Token> SectionTemplates { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the currently active annotations.
        /// </summary>
        public IEnumerable<IAnnotation> Annotations => _annotations.AsEnumerable();

        /// <summary>
        /// Currently included files.
        /// </summary>
        public HashSet<string> Included { get; } = [];

        /// <summary>
        /// Gets extra CSS.
        /// </summary>
        public IList<string> ExtraCss { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the defined parameters in the current scope.
        /// </summary>
        public Dictionary<string, SyntaxNode> Parameters { get; set; }

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        /// <param name="loadAssembly">If <c>true</c>, the assembly should be searched for components using reflection.</param>
        /// <param name="formatter">The text formatter used for the graphical circuit.</param>
        public ParsingContext(bool loadAssembly = true, ITextFormatter formatter = null)
        {
            if (loadAssembly)
                Factory.RegisterAssembly(typeof(ParsingContext).Assembly);
            Circuit = new GraphicalCircuit(formatter ?? new SimpleTextFormatter(new SkiaTextMeasurer()));
        }

        /// <summary>
        /// Creates a new parsing context based on an existing parsing context.
        /// </summary>
        /// <remarks>
        /// The new context shares the drawable factories and the diagnostics.
        /// </remarks>
        /// <param name="context">The existing context.</param>
        public ParsingContext(ParsingContext context)
        {
            // Things we can reuse
            Factory = context.Factory;
            Diagnostics = context.Diagnostics;
            Circuit = new GraphicalCircuit(context.Circuit.TextFormatter);
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
        /// Pushes a new annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        public void PushAnnotation(IAnnotation annotation)
            => _annotations.Push(annotation);

        /// <summary>
        /// Pops the last annotation.
        /// </summary>
        /// <returns>The top annotation that got popped; or <c>null</c> if there was no annotation.</returns>
        public IAnnotation PopAnnotation()
        {
            if (_annotations.Count == 0)
                return null;
            return _annotations.Pop();
        }

        /// <summary>
        /// Gets the full name based on the current section stack.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The full name.</returns>
        public string GetFullname(string name)
            => string.Join(DrawableFactoryDictionary.Separator.ToString(), _sections.Select(s => s.Name).Reverse().Union(new[] { name }));

        /// <summary>
        /// Gets the full name of a wire.
        /// </summary>
        /// <returns>The wire name.</returns>
        public string GetWireFullname()
            => GetFullname($"w{DrawableFactoryDictionary.AnonymousSeparator}{++WireCount}");
    }
}
