using SimpleCircuit.Components.Wires;
using SimpleCircuit.Parser;

namespace SimpleCircuit.Components.Annotations
{
    /// <summary>
    /// Describes an annotation for other components.
    /// </summary>
    public interface IAnnotation
    {
        /// <summary>
        /// Adds a drawable to the annotation.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public void Add(ComponentInfo drawable);

        /// <summary>
        /// Adds a wire to the annotation.
        /// </summary>
        /// <param name="wire">The wire.</param>
        public void Add(WireInfo wire);
    }
}
