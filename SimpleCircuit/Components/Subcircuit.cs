using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// A subcircuit.
    /// </summary>
    /// <seealso cref="TransformingComponent" />
    public class Subcircuit : TransformingComponent
    {
        private readonly Circuit _ckt;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subcircuit"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Subcircuit(string name, Circuit definition, IEnumerable<Pin> pins)
            : base(name)
        {
            _ckt = definition ?? throw new ArgumentNullException(nameof(definition));
            if (!_ckt.Solved)
                _ckt.Solve();

            // Find the pins in the subcircuit
            foreach (var pin in pins)
            {
                Pins.Add(
                    Name,
                    new[] { pin.Name.Replace('.', '_') }, 
                    pin.ToString(),
                    new Vector2(pin.X.Value, pin.Y.Value),
                    new Vector2(pin.NormalX.Value, pin.NormalY.Value));
            }
        }

        /// <inheritdoc/>
        protected override void Draw(SvgDrawing drawing)
        {
            _ckt.Render(drawing);
        }
    }
}
