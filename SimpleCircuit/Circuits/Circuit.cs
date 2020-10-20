using System;
using System.Collections.Generic;
using System.Xml;
using SimpleCircuit.Circuits;
using SimpleCircuit.Components;
using SimpleCircuit.Functions;

namespace SimpleCircuit
{
    /// <summary>
    /// Represents a circuit of interconnected components.
    /// </summary>
    public class Circuit
    {
        private readonly Dictionary<string, IComponent> _components = new Dictionary<string, IComponent>();
        private readonly HashSet<Function> _constraints = new HashSet<Function>();
        private readonly List<Wire> _wires = new List<Wire>();

        /// <summary>
        /// Occurs when a warning is generated.
        /// </summary>
        public event EventHandler<WarningEventArgs> Warning;

        /// <summary>
        /// Gets or sets the minimum length of a wire.
        /// </summary>
        /// <value>
        /// The minimum length of the wire.
        /// </value>
        public static double MinimumWireLength { get; set; } = 7.5;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Circuit"/> is solved.
        /// </summary>
        /// <value>
        ///   <c>true</c> if solved; otherwise, <c>false</c>.
        /// </value>
        public bool Solved { get; set; }

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
.point circle { fill: black; }
.plane { stroke-width: 1pt; }
text { font-family: Tahoma, Verdana, Segoe, sans-serif; }";

        /// <summary>
        /// Gets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        public IEnumerable<IComponent> Components => _components.Values;

        /// <summary>
        /// Gets the component count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int ComponentCount => _components.Count;

        /// <summary>
        /// Gets the wire count.
        /// </summary>
        /// <value>
        /// The wire count.
        /// </value>
        public int WireCount => _wires.Count;

        /// <summary>
        /// Gets the <see cref="IComponent"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IComponent"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IComponent this[string name] => _components[name];

        /// <summary>
        /// Adds the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Add(IComponent component)
        {
            if (component == null)
                return;
            _components.Add(component.Name, component);
            Solved = false;
        }

        /// <summary>
        /// Adds the specified function as a constraint.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        public void Add(Function constraint)
        {
            if (constraint == null || constraint.IsFixed)
                return;
            _constraints.Add(constraint);
            Solved = false;
        }

        /// <summary>
        /// Adds the specified wire.
        /// </summary>
        /// <param name="wire">The wire.</param>
        public void Add(Wire wire)
        {
            if (wire == null)
                return;
            _wires.Add(wire);
            Solved = false;
        }

        /// <summary>
        /// Removes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        public bool Remove(string name)
        {
            if (_components.ContainsKey(name))
            {
                _components.Remove(name);
                Solved = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the specified components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void Add(IEnumerable<IComponent> components)
        {
            foreach (var component in components)
                Add(component);
        }

        /// <summary>
        /// Adds the specified components.
        /// </summary>
        /// <param name="components">The components.</param>
        public void Add(params IComponent[] components)
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
        public bool TryGetValue(string name, out IComponent value) => _components.TryGetValue(name, out value);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _components.Clear();
            _constraints.Clear();
            _wires.Clear();
            Solved = false;
        }

        /// <summary>
        /// Solves the unknowns in this circuit.
        /// </summary>
        public void Solve()
        {
            var minimizer = new Minimizer();
            minimizer.Warning += Proxy;

            // Build the function that needs to be minimized
            for (var i = 0; i < _wires.Count; i++)
            {
                foreach (var length in _wires[i].Lengths)
                {
                    if (length.IsFixed)
                        continue;
                    var x = length - MinimumWireLength;
                    minimizer.Minimize += 1e3 * (x + new Squared(x) + new Exp(-x));
                    length.Value = MinimumWireLength;
                    minimizer.AddMinimum(length, 0.0);
                }
            }

            foreach (var c in _components)
                c.Value.Apply(minimizer);

            foreach (var c in _constraints)
            {
                if (!c.IsFixed)
                    minimizer.AddConstraint(c);
            }

            minimizer.Solve();
            Solved = true;
            minimizer.Warning -= Proxy;
        }

        private void Proxy(object sender, WarningEventArgs args) => Warning?.Invoke(sender, args);

        /// <summary>
        /// Renders the specified drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        public void Render(SvgDrawing drawing)
        {
            // Draw all components
            foreach (var c in _components)
            {
                drawing.StartGroup(c.Value.Name, c.Value.GetType().Name.ToLower());
                c.Value.Render(drawing);
                drawing.EndGroup();
            }

            // Draw wires
            var drawn = new HashSet<Wire>();
            foreach (var wire in _wires)
            {
                wire.Render(drawn, drawing);
                drawn.Add(wire);
            }
        }

        /// <summary>
        /// Renders the circuit.
        /// </summary>
        /// <param name="style">The style sheet information.</param>
        /// <returns>The circuit.</returns>
        public XmlDocument Render(string style = null)
        {
            if (!Solved)
                Solve();

            // Create our drawing
            var drawing = new SvgDrawing
            {
                Style = style ?? DefaultStyle
            };

            // Draw
            Render(drawing);

            // Return the XML document
            return drawing.GetDocument();
        }
    }
}
