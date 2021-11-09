using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
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
.dot { fill: black; }
.plane { stroke-width: 1pt; }
.battery line.negative { stroke-width: 0.75pt; }
text { font-family: Tahoma, Verdana, Segoe, sans-serif; }";

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
        public void Solve(IDiagnosticHandler diagnostics)
        {
            var context = new CircuitContext();
            bool first = true;

            // First discover any node maps
            foreach (var c in _presences.Values)
            {
                c.DiscoverNodeRelationships(context.Nodes, diagnostics);
                if (first && c is ILocatedPresence lp)
                {
                    context.Nodes.Shorts.Group("0", lp.X);
                    context.Nodes.Shorts.Group("0", lp.Y);
                    first = false;
                }
            }

            foreach (var c in _presences.Values)
                c.Register(context, diagnostics);

            if (context.Circuit.Count == 0)
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "SOL001",
                    $"No elements to solve for."));
                return;
            }

            var op = new OP("op");
            if (diagnostics != null)
                SpiceSharp.SpiceSharpWarning.WarningGenerated += (sender, args) => diagnostics.Post(new DiagnosticMessage(SeverityLevel.Warning, "OP001", args.Message));
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
                        context.Circuit.Add(new SpiceSharp.Components.Resistor($"fix.R{++fixResistors}", violation.FloatingVariable.Name, "0", 1e-3));
                    else
                        throw ex;
                }
            }
            while (fixResistors > previousFixResistors);

            var state = op.GetState<IBiasingSimulationState>();
            foreach (var c in _presences.Values)
                c.Update(state, context, diagnostics);
        }

        /// <summary>
        /// Renders the specified drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing)
        {
            if (drawing == null)
                throw new ArgumentNullException(nameof(drawing));

            // Draw all components
            foreach (var c in _presences.Values.OfType<IDrawable>())
                c.Render(drawing);
        }

        /// <summary>
        /// Renders the graphical circuit to an SVG XML document.
        /// </summary>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <returns>The XML document.</returns>
        public XmlDocument Render(IDiagnosticHandler diagnostics)
        {
            if (!Solved)
                Solve(diagnostics);

            // Create our drawing
            var drawing = new SvgDrawing
            {
                Style = Style
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
