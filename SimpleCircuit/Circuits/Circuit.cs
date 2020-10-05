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

        /// <summary>
        /// The default style for drawings.
        /// </summary>
        public static string DefaultStyle =
@"path, polyline, line, circle, polygon {
    stroke: black;
    stroke-width: 0.5pt;
    fill: transparent;
    stroke-linecap: round;
    stroke-linejoin: round;
}
polygon, .point circle {
    fill: black;
}
.plane {
    stroke-width: 1pt;
}
text {
    font: 4pt Tahoma, Verdana, Segoe, sans-serif;
}";

        /// <summary>
        /// Gets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        public IEnumerable<IComponent> Components => _components.Values;

        /// <summary>
        /// Gets the wires that connect pins together in the order that they are drawn.
        /// </summary>
        /// <value>
        /// The wires.
        /// </value>
        public List<Wire> Wires { get; } = new List<Wire>();

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
            Wires.Clear();
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
                        Console.WriteLine($"Managed to resolve {c}");
                    }
                }
            }

            // Build the function that needs to be minimized
            for (var i = 0; i < Wires.Count; i++)
            {
                if (Wires[i].Length.IsFixed)
                    continue;
                if (minimizer.Minimize == null)
                    minimizer.Minimize = Wires[i].Length / 10.0;
                else
                    minimizer.Minimize *= Wires[i].Length / 10.0;
                minimizer.AddMinimum(Wires[i].Length, 10.0);
            }
            if (minimizer.Minimize == null)
                minimizer.Minimize = 0.0;

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

            // Draw all wires
            var drawn = new List<Wire>();
            foreach (var w in Wires)
            {
                w.Render(drawn, drawing);
                drawn.Add(w);
            }

            // Return the XML document
            return drawing.GetDocument();
        }
    }
}
