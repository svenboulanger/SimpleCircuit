using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser.SimpleTexts;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SimpleCircuit
{
    /// <summary>
    /// Represents a circuit of interconnected components.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="formatter">A text formatter.</param>
    public class GraphicalCircuit(IStyle style = null, ITextFormatter formatter = null) : IEnumerable<ICircuitPresence>
    {
        private readonly Dictionary<string, ICircuitPresence> _presences = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the number of graphical circuit presences.
        /// </summary>
        public int Count => _presences.Count;

        /// <summary>
        /// The default style for drawings.
        /// </summary>
        public static bool DefaultStyle { get; set; } = true;

        /// <summary>
        /// Gets a dictionary of metadata key-value pairs that are optional.
        /// </summary>
        public Dictionary<string, string> Metadata { get; } = [];

        /// <summary>
        /// Gets the <see cref="IDrawable"/> with the specified name.
        /// </summary>
        public ICircuitPresence this[string name] => _presences[name];

        /// <summary>
        /// Gets whether the circuit has been solved.
        /// </summary>
        public bool Solved { get; private set; } = false;

        /// <summary>
        /// Gets or sets the minimum spacing in X- and Y-direction.
        /// </summary>
        public Vector2 Spacing { get; set; } = new(10.0, 10.0);

        /// <summary>
        /// Gets or sets a flag that determines whether bounds are rendered.
        /// </summary>
        public bool RenderBounds { get; set; }

        /// <summary>
        /// Gets the text formatter.
        /// </summary>
        public ITextFormatter TextFormatter => formatter ?? new SimpleTextFormatter(new SkiaTextMeasurer());

        /// <summary>
        /// Gets the style.
        /// </summary>
        public IStyle Style { get; } = style ?? Drawing.Styles.Style.Light;

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

            // Prepare the circuit (first constrain orientations, then prepare offsets)
            var prepareContext = new PrepareContext(this, diagnostics);
            if (!Prepare(presences, prepareContext))
                return false;

            // Space loose blocks next to each other to avoid overlaps
            var registerContext = new RegisterContext(diagnostics, prepareContext);
            void Log(object sender, SpiceSharp.WarningEventArgs e)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "OP001", e.Message));
            }

            // Register any solvable presences in the circuit
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
                c.Register(registerContext);

            // Make sure we have a solution by grounding floating nodes
            foreach (string group in prepareContext.Groups.Representatives)
                registerContext.Circuit.Add(new Resistor($"R{group}", group, "0", 1e6));

            // If there are no circuit components to solve, let's stop here
            if (registerContext.Circuit.Count == 0)
            {
                diagnostics?.Post(ErrorCodes.NoUnknownsToSolve);
                var updateContext = new UpdateContext(diagnostics, null, prepareContext);

                // Apply spacing
                if (!Update(prepareContext, updateContext, presences))
                    return false;
                return true;
            }

            // Solve the circuit
            SpiceSharp.SpiceSharpWarning.WarningGenerated += Log;
            try
            {
                // Solve
                var op = new OP("op");
                op.BiasingParameters.Validate = false; // We should have constructed a valid circuit
                foreach (var _ in op.Run(registerContext.Circuit)) { }

                // Extract the information
                var state = op.GetState<IBiasingSimulationState>();
                var updateContext = new UpdateContext(diagnostics, state, prepareContext);

                // Apply spacing
                if (!Update(prepareContext, updateContext, presences))
                    return false;
            }
            finally
            {
                SpiceSharp.SpiceSharpWarning.WarningGenerated -= Log;
            }

            return true;
        }

        private bool Prepare(IEnumerable<ICircuitPresence> presences, PrepareContext context)
        {
            // Reset all presences
            context.Mode = PreparationMode.Reset;
            foreach (var presence in presences)
                presence.Prepare(context);

            // Resolve links and references to other elements
            context.Mode = PreparationMode.Find;
            if (!PrepareCycle(presences, context))
                return false;

            // Prepare orientations
            context.Mode = PreparationMode.Orientation;
            if (!PrepareCycle(presences, context))
                return false;

            // Prepare measurements
            context.Mode = PreparationMode.Sizes;
            if (!PrepareCycle(presences, context))
                return false;

            // Prepare offsets
            context.Mode = PreparationMode.Offsets;
            if (!PrepareCycle(presences, context))
                return false;

            // Prepare linked groups
            context.Mode = PreparationMode.Groups;
            if (!PrepareCycle(presences, context))
                return false;

            // Prepare grouped drawables
            context.Mode = PreparationMode.DrawableGroups;
            if (!PrepareCycle(presences, context))
                return false;

            return true;
        }

        private bool PrepareCycle(IEnumerable<ICircuitPresence> presences, PrepareContext context)
        {
            context.Desparateness = DesperatenessLevel.Normal;
            bool success = true;

            // Preparation presences
            List<ICircuitPresence> _todo = [];
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

        private bool Update(PrepareContext prepareContext, UpdateContext updateContext, List<ICircuitPresence> presences)
        {
            // Initial update used for determining the bounds
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
                c.Update(updateContext);

            if (prepareContext.DrawnGroups.Count <= 1)
                return true; // No need to apply spacing

            // Prepare information for figuring out the locations of the groups
            HashSet<string> groupsX = [], groupsY = [];
            Dictionary<string, double> sizes = [], offset = [];
            var builder = new BoundsBuilder(TextFormatter, prepareContext.Style, updateContext.Diagnostics);
            foreach (var pair in prepareContext.DrawnGroups.Groups)
            {
                // Initialize
                groupsX.Add(pair.Key.GroupX);
                groupsY.Add(pair.Key.GroupY);

                // Calculate the bounds of this block
                builder.BeginBounds();
                foreach (var c in pair.Value.Drawables)
                    c.Render(builder);
                builder.EndBounds(out var bounds);

                // Track the total width/height of the group track
                if (sizes.TryGetValue(pair.Key.GroupX, out double existing))
                    sizes[pair.Key.GroupX] = Math.Max(existing, bounds.Width);
                else
                    sizes.Add(pair.Key.GroupX, bounds.Width);
                if (sizes.TryGetValue(pair.Key.GroupY, out existing))
                    sizes[pair.Key.GroupY] = Math.Max(existing, bounds.Height);
                else
                    sizes.Add(pair.Key.GroupY, bounds.Height);

                // Track the top-left corner
                if (offset.TryGetValue(pair.Key.GroupX, out existing))
                    offset[pair.Key.GroupX] = Math.Min(existing, bounds.Left);
                else
                    offset[pair.Key.GroupX] = bounds.Left;
                if (offset.TryGetValue(pair.Key.GroupY, out existing))
                    offset[pair.Key.GroupY] = Math.Min(existing, bounds.Top);
                else
                    offset[pair.Key.GroupY] = bounds.Top;
            }

            // Determine the group locations based on the sizes of each group track
            Dictionary<string, double> location = [];
            double offsetY = 0.0;
            foreach (string groupY in groupsY)
            {
                double offsetX = 0.0;
                foreach (string groupX in groupsX)
                {
                    // Get the bounds (skip if there is none)
                    var key = new DrawableGrouper.Key(groupX, groupY);
                    if (!prepareContext.DrawnGroups.ContainsKey(key))
                        continue;

                    // We will allocate space for each group in a zig-zag pattern
                    location[groupX] = offsetX - offset[groupX];
                    offsetX += sizes[groupX] + Spacing.X;
                }
                location[groupY] = offsetY - offset[groupY];
                offsetY += sizes[groupY] + Spacing.Y;
            }

            foreach (string representative in prepareContext.Offsets.Representatives)
            {
                string group = prepareContext.Groups.GetRepresentative(representative);
                updateContext.AddOffset(representative, location[group]);
            }

            // Extract the information
            updateContext.WireSegments.Clear();
            foreach (var c in presences.OfType<ICircuitSolverPresence>())
                c.Update(updateContext);

            return true;
        }

        /// <summary>
        /// Renders the specified drawing.
        /// </summary>
        /// <param name="builder">The drawing.</param>
        public void Render(IGraphicsBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            // Draw all components
            foreach (var c in _presences.Values.OfType<IDrawable>().OrderBy(d => d.Order))
                c.Render(builder);
        }

        /// <summary>
        /// Renders the graphical circuit to an SVG XML document.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <returns>The XML document, or <c>null</c> if the process failed.</returns>
        public XmlDocument Render(IDiagnosticHandler diagnostics)
        {
            if (!Solved)
            {
                if (!Solve(diagnostics))
                    return null;
            }

            // Create our drawing
            var drawing = new SvgBuilder(TextFormatter, Style, diagnostics);

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
