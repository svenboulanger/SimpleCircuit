using SimpleCircuit.Contributors;
using System.Collections.Generic;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes a component with a graphical representation, and pins that can be interconnected.
    /// </summary>
    /// <seealso cref="IItem" />
    public interface IComponent
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the pins.
        /// </summary>
        /// <value>
        /// The pins.
        /// </value>
        IReadOnlyList<IPin> Pins { get; }

        /// <summary>
        /// Gets the contributors that define the component.
        /// </summary>
        /// <value>
        /// The contributors.
        /// </value>
        IEnumerable<Contributor> Contributors { get; }

        /// <summary>
        /// Renders the component in the specified drawing.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        void Render(SvgDrawing drawing);
    }
}
