using SimpleCircuit.Components.Styles;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes a component with a graphical representation, and pins that can be interconnected.
    /// </summary>
    public interface IDrawable : ICircuitSolverPresence, IStyled
    {
        /// <summary>
        /// Gets the variants.
        /// </summary>
        public VariantSet Variants { get; }

        /// <summary>
        /// Gets the labels.
        /// </summary>
        public Labels Labels { get; }

        /// <summary>
        /// Gets the pins.
        /// </summary>
        public IPinCollection Pins { get; }

        /// <summary>
        /// Gets the properties available for the drawable.
        /// </summary>
        public IEnumerable<string[]> Properties { get; }

        /// <summary>
        /// Gets the bounds of the drawable.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <param name="propertyToken">A token that contains the property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <returns>Returns <c>true</c> if the property was set; otherwise, <c>false</c>.</returns>
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics);

        /// <summary>
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="builder">The graphics builder.</param>
        public void Render(IGraphicsBuilder builder);
    }
}
