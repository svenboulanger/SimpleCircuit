using SimpleCircuit.Components.Pins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A subcircuit.
    /// </summary>
    public class Subcircuit : ScaledOrientedDrawable
    {
        private readonly GraphicalCircuit _ckt;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Subcircuit(string name, GraphicalCircuit definition, Options options, IEnumerable<IPin> pins)
            : base(name, options)
        {
            _ckt = definition ?? throw new ArgumentNullException(nameof(definition));

            // Find the pins in the subcircuit
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> possibleNames = new();
            foreach (var pin in pins)
            {
                possibleNames.Clear();

                // A pin can have multiple names, and we we want to have the easiest solution!
                string shortName = pin.Owner.Name;
                string key = pin.Owner.GetType().GetCustomAttributes(true).OfType<SimpleKeyAttribute>().FirstOrDefault()?.Key;
                if (shortName.StartsWith(key))
                    shortName = shortName.Substring(key.Length);
                if (!set.Contains(shortName))
                    possibleNames.Add(shortName);
                foreach (string pn in pin.Owner.Pins.NamesOf(pin))
                {
                    string n = $"{shortName}_{pn}";
                    if (!set.Contains(n))
                    {
                        set.Add(n);
                        possibleNames.Add(n);
                    }
                    possibleNames.Add($"{pin.Owner.Name}_{pn}");
                    set.Add($"{pin.Owner.Name}_{pn}");
                }

                if (pin is IOrientedPin op)
                {
                    Pins.Add(new FixedOrientedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location, op.Orientation),
                        possibleNames);
                }
                else
                {
                    Pins.Add(new FixedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location),
                        possibleNames);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            _ckt.Render(drawing);
        }
    }
}
