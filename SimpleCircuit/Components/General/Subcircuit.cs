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
        public Subcircuit(string name, GraphicalCircuit definition, IEnumerable<IPin> pins)
            : base(name)
        {
            _ckt = definition ?? throw new ArgumentNullException(nameof(definition));

            // Find the pins in the subcircuit
            foreach (var pin in pins)
            {
                if (pin is IOrientedPin op)
                {
                    Pins.Add(new FixedOrientedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location, op.Orientation),
                        pin.Owner.Pins.NamesOf(pin).Select(n => $"{pin.Owner.Name}_{n}").ToArray());
                }
                else
                {
                    Pins.Add(new FixedPin($"{pin.Owner.Name}_{pin.Name}", pin.Description, this, pin.Location),
                        pin.Owner.Pins.NamesOf(pin).Select(n => $"{pin.Owner.Name}_{n}").ToArray());
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
