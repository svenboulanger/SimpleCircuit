using SimpleCircuit.Components;
using SimpleCircuit.Components.Markers;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser;
using SimpleCircuit.Parser.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private readonly Stack<HashSet<IDrawable>> _trackedDrawables = [];

        public const string WireKey = ":wire:";
        public const string VirtualKey = ":virtual:";
        public const string AnnotationKey = ":annotation:";

        public const string BoxName = "box";
        public const string WireName = "wire";

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
        /// Gets the already parsed files.
        /// </summary>
        public Dictionary<string, SyntaxNode> IncludeDefinitions { get; } = [];

        /// <summary>
        /// Gets or sets whether the evaluator should try to be compatible with SimpleCircuit 2.x.
        /// </summary>
        public bool CompatibilityMode { get; set; } = true;

        /// <summary>
        /// Gets the queue of anonymous points.
        /// </summary>
        public Queue<ILocatedDrawable> QueuedPoints { get; } = [];

        /// <summary>
        /// Gets or sets the base path of the evaluation context.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets all the requested themes.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Themes { get; } = [];

        /// <summary>
        /// Create a new parsing context with the default stuff in it.
        /// </summary>
        /// <param name="loadAssembly">If <c>true</c>, the assembly should be searched for components using reflection.</param>
        /// <param name="style">The style.</param>
        /// <param name="formatter">The text formatter used for the graphical circuit.</param>
        /// <param name="options">The options.</param>
        public EvaluationContext(bool loadAssembly = true, IStyle style = null, ITextFormatter formatter = null, Options options = null)
        {
            if (loadAssembly)
            {
                Factory.RegisterAssembly(typeof(ParsingContext).Assembly);

                // Search for markers in the assembly
                foreach (var t in typeof(ParsingContext).Assembly.GetTypes())
                {
                    if (t.IsAbstract || t.IsInterface || t.IsGenericType)
                        continue;
                    var nt = t;
                    while (nt is not null)
                    {
                        nt = nt.BaseType;
                        if (nt == typeof(Marker))
                        {
                            // Version where location and orientation is given
                            var ctor = t.GetConstructor([typeof(Vector2), typeof(Vector2)]);
                            if (ctor is not null)
                            {
                                Marker factory() => (Marker)Activator.CreateInstance(t, [new Vector2(), new Vector2()]);
                                foreach (var attribute in t.GetCustomAttributes<DrawableAttribute>())
                                    Markers.Add(attribute.Key, factory);
                            }
                        }
                    }
                }
            }
            Circuit = new GraphicalCircuit(style, formatter);
            Options = options ?? new Options();
            CurrentScope = new();
        }

        /// <summary>
        /// Creates a new <see cref="EvaluationContext"/>.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="circuit">The circuit.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factories"/>, <paramref name="circuit"/> or <paramref name="options"/> is <c>null</c>.</exception>
        public EvaluationContext(DrawableFactoryDictionary factories, GraphicalCircuit circuit, Options options, Scope scope = null)
        {
            Factory = factories ?? throw new ArgumentNullException(nameof(factories));
            Circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));
            Options = options ?? throw new ArgumentNullException(nameof(options));
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
        /// Tries to get an anonymous component backtracking a number of steps.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="name">The name.</param>
        /// <param name="backtrack">The number of steps to backtrack.</param>
        /// <param name="presence">The found result.</param>
        /// <returns>Returns <c>true</c> if the component could be found; otherwise, <c>false</c>.</returns>
        public bool TryGetBacktrackedAnonymousComponent(TextLocation location, string name, int backtrack, out ICircuitPresence presence)
        {
            // Check whether this is referring to other sections
            if (name.Contains(DrawableFactoryDictionary.Separator))
            {
                Diagnostics?.Post(location, ErrorCodes.CouldNotBacktrackToSections, name, backtrack);
                presence = null;
                return false;
            }

            // Check that the name is a key
            if (!Factory.IsAnonymous(name, out string key))
            {
                Diagnostics?.Post(location, ErrorCodes.ExpectedAnonymousKey, name);
                presence = null;
                return false;
            }

            // Get the counter for the given anonymous key
            if (backtrack <= 0 || !_anonymousCounters.TryGetValue(key, out int counter) || backtrack >= counter)
            {
                Diagnostics?.Post(location, ErrorCodes.CouldNotFindBacktrackedAnonymousComponent, name, backtrack);
                presence = null;
                return false;
            }

            // Expand the name
            name = $"{name}{DrawableFactoryDictionary.AnonymousSeparator}{counter - backtrack}";
            name = string.Join(DrawableFactoryDictionary.Separator.ToString(), _sections.Reverse().Union([name]));
            if (!Circuit.TryGetValue(name, out presence))
                throw new ArgumentException();
            return true;
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
        /// Gets an annotation name.
        /// </summary>
        /// <returns>The annotation name.</returns>
        public string GetAnnotationBoxName()
        {
            string name;
            if (_anonymousCounters.TryGetValue(AnnotationKey, out int counter))
            {
                name = $"{BoxName}{DrawableFactoryDictionary.AnonymousSeparator}{counter}";
                _anonymousCounters[AnnotationKey] = counter + 1;
            }
            else
            {
                name = $"{BoxName}{DrawableFactoryDictionary.AnonymousSeparator}1";
                _anonymousCounters[AnnotationKey] = 2;
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
            if (_anonymousCounters.TryGetValue(WireKey, out int counter))
            {
                name = $"{WireName}{DrawableFactoryDictionary.AnonymousSeparator}{counter}";
                _anonymousCounters[WireKey] = counter + 1;
            }
            else
            {
                name = $"{WireName}{DrawableFactoryDictionary.AnonymousSeparator}1";
                _anonymousCounters[WireKey] = 2;
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
            if (_anonymousCounters.TryGetValue(VirtualKey, out int counter))
            {
                name = $"virtual-{counter}";
                _anonymousCounters[VirtualKey] = counter + 1;
            }
            else
            {
                name = "virtual-1";
                _anonymousCounters[VirtualKey] = 2;
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
        /// Starts tracking drawables.
        /// </summary>
        public void StartTrackingDrawables() => _trackedDrawables.Push([]);

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

        /// <summary>
        /// Stop tracking drawables.
        /// </summary>
        /// <returns>The found drawables.</returns>
        public IEnumerable<IDrawable> StopTrackingDrawables()
        {
            var set = _trackedDrawables.Pop();
            if (_trackedDrawables.Count > 0)
                _trackedDrawables.Peek().UnionWith(set);
            return set;
        }

        /// <summary>
        /// Notify the context of a newly created drawable.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public void NotifyDrawable(IDrawable drawable)
        {
            if (_trackedDrawables.Count > 0 && drawable is not null)
                _trackedDrawables.Peek().Add(drawable);
        }
    }
}
