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
        /// Gets or sets the minimum length of a wire.
        /// </summary>
        /// <value>
        /// The minimum length of the wire.
        /// </value>
        public static double MinimumWireLength { get; set; } = 7.5;

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
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _components.Count;

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
        }

        /// <summary>
        /// Adds the specified function as a constraint.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        public void Add(Function constraint)
        {
            if (constraint == null || constraint.IsConstant)
                return;
            _constraints.Add(constraint);
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
        }

        /// <summary>
        /// Renders the circuit.
        /// </summary>
        /// <param name="style">The style sheet information.</param>
        /// <returns>The circuit.</returns>
        public XmlDocument Render(string style = null)
        {
            var minimizer = new Minimizer();

            // First try to precompute as many constraints as possible
            bool resolvedConstraints = true;
            while (resolvedConstraints)
            {
                resolvedConstraints = false;
                foreach (var c in _constraints)
                {
                    if (c.Resolve(0.0))
                    {
                        resolvedConstraints = true;
                        Console.WriteLine($"Resolved {c}");
                    }
                }
            }

            // Build the function that needs to be minimized
            for (var i = 0; i < _wires.Count; i++)
            {
                foreach (var length in _wires[i].Lengths)
                {
                    if (length.IsFixed)
                        continue;
                    var x = length - MinimumWireLength;
                    minimizer.Minimize += 1e3 * (x + new Exp(-x));
                    length.Value = MinimumWireLength;
                    minimizer.AddMinimum(length, 0.0);
                }
            }

            foreach (var c in _components)
                c.Value.Apply(minimizer);

            foreach (var c in _constraints)
            {
                if (!c.IsConstant)
                    minimizer.AddConstraint(c);
            }

            minimizer.Solve();

            // Create our drawing
            var drawing = new SvgDrawing();
            drawing.Style = style ?? DefaultStyle;

            // Draw all components
            foreach (var c in _components)
            {
                drawing.StartGroup(c.Value.Name, c.Value.GetType().Name.ToLower());
                c.Value.Render(drawing);
                drawing.EndGroup();
            }

            // Draw wires
            drawing.TF = Transform.Identity;
            var drawn = new HashSet<Wire>();
            foreach (var wire in _wires)
            {
                wire.Render(drawn, drawing);
                drawn.Add(wire);
            }

            // Return the XML document
            return drawing.GetDocument();
        }
    }
}
