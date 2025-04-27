using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Builders.Markers;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using SimpleCircuit.Parser.SimpleTexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Evaluator
{
    /// <summary>
    /// A context for evaluating the AST for SimpleCircuit.
    /// </summary>
    public class EvaluationContext
    {
        private readonly Stack<string> _sections = new();
        private readonly Stack<Dictionary<string, int>> _anonymousCounterStack = [];
        private Dictionary<string, int> _anonymousCounters = [];

        /// <summary>
        /// Gets the options.
        /// </summary>
        public Options Options { get; } = new();

        /// <summary>
        /// Gets or sets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; set; }

        /// <summary>
        /// Gets or sets the current scope.
        /// </summary>
        public Scope CurrentScope { get; set; }

        /// <summary>
        /// Gets any local parameter definitions.
        /// </summary>
        public Dictionary<string, SyntaxNode> LocalParameterValues { get; } = [];

        /// <summary>
        /// Gets a set of parameters that are currently being evaluated.
        /// This set is used to find circular dependencies of variable expressions.
        /// </summary>
        public HashSet<string> UsedExpressionParameters { get; } = [];

        /// <summary>
        /// Gets possible markers.
        /// </summary>
        public Dictionary<string, Func<Marker>> Markers { get; } = [];

        /// <summary>
        /// Gets the factory for components.
        /// </summary>
        public DrawableFactoryDictionary Factory { get; } = new();

        /// <summary>
        /// Gets the circuit.
        /// </summary>
        public GraphicalCircuit Circuit { get; }

        /// <summary>
        /// Gets a dictionary of section definitions.
        /// </summary>
        public Dictionary<string, SectionDefinitionNode> SectionDefinitions { get; } = [];

        /// <summary>
        /// Gets or sets whether the evaluator should try to be compatible with SimpleCircuit 2.x.
        /// </summary>
        public bool CompatibilityMode { get; set; } = true;

        /// <summary>
        /// Gets the queue of anonymous points.
        /// </summary>
        public Queue<ILocatedDrawable> QueuedPoints { get; } = [];

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        /// <param name="loadAssembly">If <c>true</c>, the assembly should be searched for components using reflection.</param>
        /// <param name="formatter">The text formatter used for the graphical circuit.</param>
        public EvaluationContext(ParsingContext parsingContext, bool loadAssembly = true, ITextFormatter formatter = null)
        {
            if (loadAssembly)
            {
                Factory.RegisterAssembly(typeof(ParsingContext).Assembly);
                Markers.Add("arrow", () => new Arrow());
                Markers.Add("rarrow", () => new ReverseArrow());
            }
            Circuit = new GraphicalCircuit(formatter ?? new SimpleTextFormatter(new SkiaTextMeasurer()));
            Options = parsingContext.Options;
            Diagnostics = parsingContext.Diagnostics;
            CurrentScope = new();
        }

        /// <summary>
        /// Creates a new <see cref="EvaluationContext"/>.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="circuit">The circuit.</param>
        /// <param name="options">The options.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factories"/>, <paramref name="circuit"/> or <paramref name="options"/> is <c>null</c>.</exception>
        public EvaluationContext(DrawableFactoryDictionary factories, GraphicalCircuit circuit, Options options, IDiagnosticHandler diagnostics, Scope scope = null)
        {
            Factory = factories ?? throw new ArgumentNullException(nameof(factories));
            Circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Diagnostics = diagnostics;
            CurrentScope = scope ?? new();
        }

        /// <summary>
        /// Gets the full name based on the current section stack.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="resolveAnonymous">If <c>true</c>, the name is changed to a unique name if the name is anonymous. If <c>false</c>, the name is simply expanded to the full name without modification.</param>
        /// <returns>The full name.</returns>
        public string GetFullname(string name, bool resolveAnonymous = true)
        {
            if (resolveAnonymous)
            {
                if (Factory.IsAnonymous(name, out string key))
                {
                    if (_anonymousCounters.TryGetValue(key, out int counter))
                    {
                        name = $"{name}{DrawableFactoryDictionary.AnonymousSeparator}{counter}";
                        _anonymousCounters[key] = counter + 1;
                    }
                    else
                    {
                        name = $"{name}{DrawableFactoryDictionary.AnonymousSeparator}1";
                        _anonymousCounters[key] = 2;
                    }

                }
            }
            return string.Join(DrawableFactoryDictionary.Separator.ToString(), _sections.Reverse().Union([name]));
        }

        /// <summary>
        /// Gets the full name for an anonymous point.
        /// </summary>
        /// <returns>Gets the full name of the anonymous point.</returns>
        public string GetAnonymousPointName()
        {
            string name;
            if (_anonymousCounters.TryGetValue(PointFactory.Key, out int counter))
            {
                name = $"{PointFactory.Key}{DrawableFactoryDictionary.AnonymousSeparator}{counter}";
                _anonymousCounters[PointFactory.Key] = counter + 1;
            }
            else
            {
                name = $"{PointFactory.Key}{DrawableFactoryDictionary.AnonymousSeparator}1";
                _anonymousCounters[PointFactory.Key] = 2;
            }
            return string.Join(DrawableFactoryDictionary.Separator.ToString(), _sections.Reverse().Union([name]));
        }

        /// <summary>
        /// Gets a wire name.
        /// </summary>
        /// <returns>The wire name.</returns>
        public string GetWireName()
        {
            string name;
            if (_anonymousCounters.TryGetValue(":wire:", out int counter))
            {
                name = $"wire-{counter}";
                _anonymousCounters[":wire:"] = counter + 1;
            }
            else
            {
                name = "wire-1";
                _anonymousCounters[":wire:"] = 2;
            }
            return string.Join(DrawableFactoryDictionary.Separator.ToString(), _sections.Reverse().Union([name]));
        }

        /// <summary>
        /// Gets the name of a virtual chain item.
        /// </summary>
        /// <returns>The virtual item name.</returns>
        public string GetVirtualName()
        {
            string name;
            if (_anonymousCounters.TryGetValue(":virtual:", out int counter))
            {
                name = $"virtual-{counter}";
                _anonymousCounters[":virtual:"] = counter + 1;
            }
            else
            {
                name = "virtual-1";
                _anonymousCounters[":virtual:"] = 2;
            }
            return string.Join(DrawableFactoryDictionary.Separator.ToString(), _sections.Reverse().Union([name]));
        }

        /// <summary>
        /// Pushes/starts a new section.
        /// </summary>
        /// <param name="sectionName">The section.</param>
        public void StartSection(string sectionName)
        {
            _sections.Push(sectionName);
            StartScope();
            _anonymousCounterStack.Push(_anonymousCounters);
            _anonymousCounters = [];
        }

        /// <summary>
        /// Starts a new scope.
        /// </summary>
        public void StartScope() => CurrentScope = new Scope(CurrentScope);

        /// <summary>
        /// Pops/ends the last started section.
        /// </summary>
        /// <returns>The section name that was closed.</returns>
        public string EndSection()
        {
            CurrentScope = CurrentScope.ParentScope;
            _anonymousCounters = _anonymousCounterStack.Pop();
            return _sections.Pop();
        }

        /// <summary>
        /// Ends a previously started scope.
        /// </summary>
        public void EndScope() => CurrentScope = CurrentScope.ParentScope;
    }
}
