using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;

namespace SimpleCircuit
{
    /// <summary>
    /// Represents a circuit of interconnected components.
    /// </summary>
    public class GraphicalCircuit : IEnumerable<ICircuitPresence>
    {
        private readonly Dictionary<string, ICircuitPresence> _presences = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the style for the graphics.
        /// </summary>
        /// <value>
        /// The cascading stylesheet.
        /// </value>
        public string Style { get; set; } = DefaultStyle;

        /// <summary>
        /// Gets or sets the target length of a wire.
        /// </summary>
        /// <value>
        /// The target length of the wire.
        /// </value>
        public double WireLength { get; set; } = 7.5;

        /// <summary>
        /// Gets or sets the text line height.
        /// </summary>
        /// <value>
        /// The line height.
        /// </value>
        public double LineHeight { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the width of a lower-case character.
        /// </summary>
        /// <value>
        /// The width of a lowercase character.
        /// </value>
        public double LowerCharacterWidth { get; set; } = 3.0;

        /// <summary>
        /// Gets or sets the width of an upper-case character.
        /// </summary>
        /// <value>
        /// The width of an uppercase character.
        /// </value>
        public double UpperCharacterWidth { get; set; } = 4.0;

        /// <summary>
        /// Gets the number of graphical circuit presences.
        /// </summary>
        public int Count => _presences.Count;

        /// <summary>
        /// The default style for drawings.
        /// </summary>
        public static string DefaultStyle =
@"path, polyline, line, circle, polygon {
    stroke: black;
    stroke-width: 0.5pt;
    fill: none;
    stroke-linecap: round;
    stroke-linejoin: round;
}
.super, .sub { font-size: 0.75em; }
.dot, .arrowhead { fill: black; }
.plane { stroke-width: 1pt; }
.battery .neg { stroke-width: 0.75pt; }
text { font-family: Tahoma, Verdana, Segoe, sans-serif; font-size: 4pt; }
.small tspan { font-size: 0.8em; }";

        /// <summary>
        /// Gets a dictionary of metadata key-value pairs that are optional.
        /// </summary>
        public Dictionary<string, string> Metadata { get; } = new();

        /// <summary>
        /// Gets the <see cref="IDrawable"/> with the specified name.
        /// </summary>
        public ICircuitPresence this[string name] => _presences[name];

        /// <summary>
        /// Gets whether the circuit has been solved.
        /// </summary>
        public bool Solved { get; private set; } = false;

        /// <summary>
        /// Adds the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Add(ICircuitPresence component)
        {
            if (component == null)
                return;
            Solved = false;
            _presences.Add(component.Name, component);
        }

        /// <summary>
        /// Removes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        public bool Remove(string name)
        {
            if (_presences.ContainsKey(name))
            {
                _presences.Remove(name);
                Solved = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the specified components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void Add(IEnumerable<ICircuitPresence> components)
        {
            foreach (var component in components)
                Add(component);
        }

        /// <summary>
        /// Adds the specified components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void Add(params ICircuitPresence[] components)
        {
            foreach (var component in components)
                Add(component);
        }

        /// <summary>
        /// Tries to get a value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string name, out ICircuitPresence value) => _presences.TryGetValue(name, out value);

        /// <summary>
        /// Checks whether the circuit contains a component with the specified key.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the component exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string name) => _presences.ContainsKey(name);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _presences.Clear();
            Solved = false;
        }

        /// <summary>
        /// Solves the unknowns in this circuit.
        /// </summary>
        public bool Solve(IDiagnosticHandler diagnostics)
        {
            var context = new CircuitSolverContext();
            bool first = true;
            void Log(object sender, SpiceSharp.WarningEventArgs e)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "OP001", e.Message));
            }
            var presences = _presences.Values.OrderBy(p => p.Order).ToList();

            // Prepare all the presences
            foreach (var c in presences)
                c.Reset();

            // Prepare the circuit
            if (!Prepare(presences, diagnostics))
                return false;

            // Solver presences
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
                c.DiscoverNodeRelationships(context.Nodes, diagnostics);
            foreach (var group in context.Nodes.Relative.Representatives)
                context.Nodes.Shorts.Group(group, "0");

            // Register any solvable presences in the circuit
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
                c.Register(context, diagnostics);

            // If there are no circuit components to solve, let's stop here
            if (context.Circuit.Count == 0)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "SOL001", $"No elements to solve for."));
                return false;
            }

            // Solve the circuit
            var op = new OP("op");
            SpiceSharp.SpiceSharpWarning.WarningGenerated += Log;
            try
            {
                do
                {
                    context.Recalculate = false;
                    int fixResistors = 0, previousFixResistors;
                    do
                    {
                        previousFixResistors = fixResistors;
                        try
                        {
                            op.Run(context.Circuit);
                        }
                        catch (ValidationFailedException ex)
                        {
                            // Let's fix floating nodes
                            var violation = ex.Rules.Violations.OfType<FloatingNodeRuleViolation>().FirstOrDefault();
                            if (violation != null)
                                context.Circuit.Add(new SpiceSharp.Components.Resistor($"fix.R{++fixResistors}", violation.FloatingVariable.Name, "0", 1));
                            else
                                throw ex;
                        }
                    }
                    while (fixResistors > previousFixResistors);

                    // Extract the information
                    var state = op.GetState<IBiasingSimulationState>();
                    context.WireSegments.Clear();
                    foreach (var c in presences.OfType<ICircuitSolverPresence>())
                        c.Update(state, context, diagnostics);
                }
                while (context.Recalculate);
            }
            finally
            {
                SpiceSharp.SpiceSharpWarning.WarningGenerated -= Log;
            }
            return true;
        }

        private bool Prepare(IEnumerable<ICircuitPresence> presences, IDiagnosticHandler diagnostics)
        {
            bool success = true;

            // Preparation presences
            List<ICircuitPresence> _todo = new();
            foreach (var c in presences)
            {
                var result = c.Prepare(this, PresenceMode.Normal, diagnostics);
                if (result == PresenceResult.Incomplete)
                    _todo.Add(c);
                if (result == PresenceResult.GiveUp)
                    success = false;
            }

            // Resolve presences
            while (_todo.Count > 0)
            {
                int oldCount = _todo.Count;
                int index = 0;

                // Redo the todo-list after doing everything else
                while (index < _todo.Count)
                {
                    switch (_todo[index].Prepare(this, PresenceMode.Normal, diagnostics))
                    {
                        case PresenceResult.Success:
                            _todo.RemoveAt(index);
                            break;

                        case PresenceResult.GiveUp:
                            _todo.RemoveAt(index);
                            success = false;
                            break;

                        default:
                            index++;
                            break;
                    }
                }

                if (oldCount == _todo.Count)
                {
                    // We did not manage to reduce the number of todo-items on the list...
                    // Tell all the remaining presences that we are becoming desparate
                    index = 0;
                    while (index < _todo.Count)
                    {
                        switch (_todo[index].Prepare(this, PresenceMode.Fix, diagnostics))
                        {
                            case PresenceResult.Success:
                                _todo.RemoveAt(index);
                                break;

                            case PresenceResult.GiveUp:
                                _todo.RemoveAt(index);
                                success = false;
                                break;

                            default:
                                index++;
                                success = false;
                                break;
                        }
                    }

                    // Stop trying to go forward if we already failed
                    if (!success)
                    {
                        // Give a chance to all presences left to raise some errors
                        for (int i = 0; i < _todo.Count; i++)
                            _todo[i].Prepare(this, PresenceMode.GiveUp, diagnostics);
                        break;
                    }
                    else
                        continue; // We managed to solve some stuff trying to fix, so let's go again
                }
            }
            return success;
        }

        /// <summary>
        /// Renders the specified drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing)
        {
            if (drawing == null)
                throw new ArgumentNullException(nameof(drawing));

            drawing.Style = Style;

            // Draw all components
            foreach (var c in _presences.Values.OfType<IDrawable>().OrderBy(d => d.Order))
                c.Render(drawing);
        }

        /// <summary>
        /// Renders the graphical circuit to an SVG XML document.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <returns>The XML document, or <c>null</c> if the process failed.</returns>
        public XmlDocument Render(IDiagnosticHandler diagnostics, IElementFormatter formatter = null)
        {
            if (!Solved)
            {
                if (!Solve(diagnostics))
                    return null;
            }

            // Create our drawing
            var drawing = new SvgDrawing
            {
                Style = Style,
                ElementFormatter = formatter
            };

            // Draw
            Render(drawing);

            // Add metadata
            foreach (var pair in Metadata)
                drawing.AddMetadata(pair.Key, pair.Value);

            // Return the XML document
            return drawing.GetDocument();
        }

        /// <inheritdoc />
        public IEnumerator<ICircuitPresence> GetEnumerator() => _presences.Values.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
