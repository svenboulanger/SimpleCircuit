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
        private readonly Dictionary<Function, string> _constraints = new Dictionary<Function, string>();
        private readonly List<Wire> _wires = new List<Wire>();

        /// <summary>
        /// Occurs when a warning is generated.
        /// </summary>
        public event EventHandler<WarningEventArgs> Warning;

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
        /// <param name="description">The description.</param>
        public void Add(Function constraint, string description = null)
        {
            if (constraint == null || constraint.IsFixed)
                return;
            if (_constraints.ContainsKey(constraint))
                return;
            _constraints.Add(constraint, description);
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
        /// Checks whether the circuit contains a component with the specified key.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the component exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string name) => _components.ContainsKey(name);

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
            minimizer.Warning += Warn;

            // Build the function that needs to be minimized
            for (var i = 0; i < _wires.Count; i++)
            {
                foreach (var length in _wires[i].Lengths)
                {
                    if (length.IsFixed)
                    {
                        if (length.Value < 0.0)
                            Warn(this, new WarningEventArgs($"Wire length '{length}' is smaller than 0."));
                        continue;
                    }
                    var x = length - WireLength;
                    minimizer.Minimize += 1e3 * (x + new Squared(x) + new Exp(-x));
                    length.Value = WireLength;
                    minimizer.AddMinimum(length, 0.0);
                }
            }

            foreach (var c in _components)
                c.Value.Apply(minimizer);

            foreach (var c in _constraints)
            {
                if (!c.Key.IsFixed)
                    minimizer.AddConstraint(c.Key, c.Value);
                else
                {
                    if (!c.Key.Value.IsZero())
                        Warn(this, new WarningEventArgs($"Could not {c.Value}"));
                }
            }

            minimizer.Solve();
            Solved = true;
            minimizer.Warning -= Warn;
        }

        /// <summary>
        /// Warn the user.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The warning arguments.</param>
        protected void Warn(object sender, WarningEventArgs args) => Warning?.Invoke(sender, args);

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
        public XmlDocument Render()
        {
            if (!Solved)
                Solve();

            // Create our drawing
            var drawing = new SvgDrawing
            {
                Style = Style,
                LineHeight = LineHeight,
                UpperCharacterWidth = UpperCharacterWidth,
                LowerCharacterWidth = LowerCharacterWidth
            };

            // Draw
            Render(drawing);

            // Return the XML document
            return drawing.GetDocument();
        }
    }
}
