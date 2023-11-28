using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SimpleCircuit.Circuits.Contexts;
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
        private readonly List<ICircuitPresence> _extra = new();

        /// <summary>
        /// Gets or sets the style for the graphics.
        /// </summary>
        /// <value>
        /// The cascading stylesheet.
        /// </value>
        public string Style { get; set; } = DefaultStyle;

        /// <summary>
        /// Gets the number of graphical circuit presences.
        /// </summary>
        public int Count => _presences.Count;

        /// <summary>
        /// The default style for drawings.
        /// </summary>
        public static string DefaultStyle { get; set; } = Properties.Resources.DefaultStyle;

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
        /// Gets or sets the minimum spacing in X-direction between blocks.
        /// </summary>
        public double SpacingX { get; set; } = 20.0;

        /// <summary>
        /// Gets or sets the minimum spacing in Y-directionb between blocks.
        /// </summary>
        public double SpacingY { get; set; } = 20.0;

        /// <summary>
        /// Gets or sets a flag that determines whether bounds are rendered.
        /// </summary>
        public bool RenderBounds { get; set; }

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
            var presences = _presences.Values.OrderBy(p => p.Order).ToList();
            _extra.Clear();

            // Prepare all the presences
            var resetContext = new ResetContext(diagnostics);
            if (!Reset(presences, resetContext))
                return false;

            // Prepare the circuit (first constrain orientations, then prepare offsets)
            var prepareContext = new PrepareContext(this, diagnostics);
            if (!Prepare(presences, prepareContext))
                return false;

            // Solver presences
            var relationshipContext = new NodeContext(diagnostics);
            if (!DiscoverNodeRelationships(presences.OfType<ICircuitSolverPresence>(), relationshipContext))
                return false;

            // Space loose blocks next to each other to avoid overlaps
            var registerContext = new RegisterContext(diagnostics, relationshipContext);
            void Log(object sender, SpiceSharp.WarningEventArgs e)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "OP001", e.Message));
            }
            if (!Space(registerContext))
                return false;
            presences.AddRange(_extra);

            // Register any solvable presences in the circuit
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
                c.Register(registerContext);

            // If there are no circuit components to solve, let's stop here
            if (registerContext.Circuit.Count == 0)
            {
                diagnostics?.Post(ErrorCodes.NoUnknownsToSolve);
                return false;
            }

            // Solve the circuit
            var op = new OP("op");
            SpiceSharp.SpiceSharpWarning.WarningGenerated += Log;
            try
            {
                do
                {
                    registerContext.Recalculate = false;
                    int fixResistors = 0, previousFixResistors;
                    do
                    {
                        previousFixResistors = fixResistors;
                        try
                        {
                            op.Run(registerContext.Circuit);
                        }
                        catch (ValidationFailedException ex)
                        {
                            // Let's fix floating nodes
                            var violation = ex.Rules.Violations.OfType<FloatingNodeRuleViolation>().FirstOrDefault();
                            if (violation != null)
                                registerContext.Circuit.Add(new SpiceSharp.Components.Resistor($"fix.R{++fixResistors}", violation.FloatingVariable.Name, "0", 1));
                            else
                                throw ex;
                        }
                    }
                    while (fixResistors > previousFixResistors);

                    // Extract the information
                    var state = op.GetState<IBiasingSimulationState>();
                    var updateContext = new UpdateContext(diagnostics, state, relationshipContext);
                    foreach (var c in presences.OfType<ICircuitSolverPresence>())
                        c.Update(updateContext);
                }
                while (registerContext.Recalculate);
            }
            finally
            {
                SpiceSharp.SpiceSharpWarning.WarningGenerated -= Log;
            }
            return true;
        }

        private bool Reset(IEnumerable<ICircuitPresence> presences, ResetContext context)
        {
            foreach (var c in presences)
            {
                if (!c.Reset(context))
                    return false;
            }
            return true;
        }

        private bool Prepare(IEnumerable<ICircuitPresence> presences, PrepareContext context)
        {
            // Prepare orientations
            context.Mode = PreparationMode.Orientation;
            if (!PrepareCycle(presences, context))
                return false;

            // Prepare offsets
            context.Mode = PreparationMode.Offsets;
            if (!PrepareCycle(presences, context))
                return false;

            return true;
        }
        private bool PrepareCycle(IEnumerable<ICircuitPresence> presences, PrepareContext context)
        {
            context.Desparateness = DesperatenessLevel.Normal;
            bool success = true;

            // Preparation presences
            List<ICircuitPresence> _todo = new();
            foreach (var c in presences)
            {
                var result = c.Prepare(context);
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
                context.Desparateness = DesperatenessLevel.Normal;
                while (index < _todo.Count)
                {
                    switch (_todo[index].Prepare(context))
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
                        context.Desparateness = DesperatenessLevel.Fix;
                        switch (_todo[index].Prepare(context))
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
                        context.Desparateness = DesperatenessLevel.GiveUp;
                        for (int i = 0; i < _todo.Count; i++)
                            _todo[i].Prepare(context);
                        break;
                    }
                    else
                        continue; // We managed to solve some stuff trying to fix, so let's go again
                }
            }
            return success;
        }

        private bool DiscoverNodeRelationships(IEnumerable<ICircuitSolverPresence> presences, NodeContext context)
        {
            // First deal with shorts to reduce the number of variables as much as possible
            context.Mode = NodeRelationMode.Offsets;
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
            {
                if (!c.DiscoverNodeRelationships(context))
                    return false;
            }
            context.Offsets.ComputeBounds();

            // Order coordinates to discover the bounds on blocks
            context.Mode = NodeRelationMode.Links;
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
            {
                if (!c.DiscoverNodeRelationships(context))
                    return false;
            }

            // Group nodes together that belong to the same "drawn block"
            context.Mode = NodeRelationMode.Groups;
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
            {
                if (!c.DiscoverNodeRelationships(context))
                    return false;
            }
            return true;
        }

        private bool Space(RegisterContext context)
        {
            // Make a map of representatives and their extremes
            var dict = new Dictionary<string, (List<string> Minima, List<string> Maxima)>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in context.Relationships.Extremes.Linked.Representatives)
                dict.Add(node, (new List<string>(), new List<string>()));

            // Map all minima
            foreach (var node in context.Relationships.Extremes.Minimum.Extremes)
            {
                string representative = context.Relationships.Extremes.Linked[node];
                if (dict.TryGetValue(representative, out var extremes))
                    extremes.Minima.Add(node);
            }

            // Map all maxima
            foreach (var node in context.Relationships.Extremes.Maximum.Extremes)
            {
                string representative = context.Relationships.Extremes.Linked[node];
                if (dict.TryGetValue(representative, out var extremes))
                    extremes.Maxima.Add(node);
            }

            // Use the XY sets to make a vertical stack of graphical blocks
            Dictionary<string, HashSet<string>> stacked = new();
            foreach (var set in context.Relationships.XYSets)
            {
                if (!stacked.TryGetValue(set.NodeY, out var horiz))
                {
                    horiz = new HashSet<string>();
                    stacked.Add(set.NodeY, horiz);
                }
                horiz.Add(set.NodeX);
            }

            // Fix the positions
            int constraint = 0;
            IEnumerable<string> lastMaxY = null;
            foreach (var blocks in stacked)
            {
                // The Y-coordinate is all related to each other, so we can simply use the minima for the y-node
                if (!dict.TryGetValue(blocks.Key, out var minMaxY))
                {
                    var list = new[] { blocks.Key };
                    AddMinimumSpacing(lastMaxY, list, SpacingY, ref constraint);
                    lastMaxY = list;
                }
                else
                {
                    // Add the minima for y-coordinates
                    AddMinimumSpacing(lastMaxY, minMaxY.Minima, SpacingY, ref constraint);
                    lastMaxY = minMaxY.Maxima;
                    dict.Remove(blocks.Key);
                }

                // Deal with the X-coordinates
                IEnumerable<string> lastMaxX = null;
                foreach (var nodeX in blocks.Value)
                {
                    if (!dict.TryGetValue(nodeX, out var minMaxX))
                    {
                        var list = new[] { nodeX };
                        AddMinimumSpacing(lastMaxX, new[] { nodeX }, SpacingX, ref constraint);
                        lastMaxX = list;
                    }
                    else
                    {
                        // Add the minima for x-coordinates
                        AddMinimumSpacing(lastMaxX, minMaxX.Minima, SpacingX, ref constraint);
                        lastMaxX = minMaxX.Maxima;
                        dict.Remove(nodeX);
                    }
                }
            }

            // Deal with loose ends
            foreach (var pair in dict)
            {
                if (pair.Value.Minima.Count == 0 || pair.Value.Maxima.Count == 0)
                    continue;
                foreach (var min in pair.Value.Minima)
                    _extra.Add(new MinimumConstraint($"constraint.{constraint++}", "0", min, 0.0) { Weight = 1e-6 });
            }
            return true;
        }

        private void AddMinimumSpacing(IEnumerable<string> lastMax, IEnumerable<string> nextMin, double spacing, ref int constraint)
        {
            if (lastMax == null)
            {
                foreach (var min in nextMin)
                    _extra.Add(new MinimumConstraint($"constraint.{constraint++}", "0", min, 0.0) { Weight = 1e-6 });
            }
            else
            {
                foreach (var min in nextMin)
                {
                    foreach (var max in lastMax)
                        _extra.Add(new MinimumConstraint($"constraint.{constraint++}", max, min, spacing) { Weight = 1e-6 });
                }
            }
        }

        /// <summary>
        /// Renders the specified drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing)
        {
            if (drawing == null)
                throw new ArgumentNullException(nameof(drawing));

            bool oldRenderBounds = drawing.RenderBounds;
            drawing.RenderBounds = RenderBounds;

            // Draw all components
            foreach (var c in _presences.Values.OfType<IDrawable>().OrderBy(d => d.Order))
                c.Render(drawing);

            // Restore original state
            drawing.RenderBounds = oldRenderBounds;
        }

        /// <summary>
        /// Renders the graphical circuit to an SVG XML document.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <returns>The XML document, or <c>null</c> if the process failed.</returns>
        public XmlDocument Render(IDiagnosticHandler diagnostics, ITextMeasurer measurer = null)
        {
            if (!Solved)
            {
                if (!Solve(diagnostics))
                    return null;
            }

            // Create our drawing
            var drawing = new SvgDrawing(measurer, diagnostics)
            {
                RenderBounds = RenderBounds
            };

            // Draw
            Render(drawing);

            // Add metadata
            foreach (var pair in Metadata)
                drawing.AddMetadata(pair.Key, pair.Value);

            // Return the XML document
            return drawing.GetDocument(Style);
        }

        /// <inheritdoc />
        public IEnumerator<ICircuitPresence> GetEnumerator() => _presences.Values.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
