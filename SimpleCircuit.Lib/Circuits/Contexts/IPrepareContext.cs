using SimpleCircuit.Components;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.SimpleTexts;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// A context used for preparing circuit presences.
    /// </summary>
    public interface IPrepareContext
    {
        /// <summary>
        /// Gets the diagnostics handler.
        /// </summary>
        public IDiagnosticHandler Diagnostics { get; }

        /// <summary>
        /// The current mode of preparation.
        /// </summary>
        /// <remarks>
        /// This mode allows doing partial preparations.
        /// </remarks>
        public DesperatenessLevel Desparateness { get; }

        /// <summary>
        /// Gets the mode of preparation.
        /// </summary>
        public PreparationMode Mode { get; }

        /// <summary>
        /// Tracks offsets between nodes.
        /// </summary>
        public NodeOffsetFinder Offsets { get; }

        /// <summary>
        /// Tracks nodes that are linked together.
        /// </summary>
        public NodeGrouper Groups { get; }

        /// <summary>
        /// Formats text.
        /// </summary>
        /// <param name="content">The content to format.</param>
        /// <param name="fontSize">The font size in points.</param>
        /// <param name="isBold">If <c>true</c>, the text is bold.</param>
        /// <param name="options">The graphic options.</param>
        /// <returns>Returns the formatted text.</returns>
        public ISpan Format(string content, double fontSize = 4.0, bool isBold = false, GraphicOptions options = null);

        /// <summary>
        /// Finds a circuit presence with the given name.
        /// </summary>
        /// <param name="name">The full name of the presence.</param>
        /// <returns>The circuit presence, of <c>null</c> if it doesn't exist.</returns>
        public ICircuitPresence Find(string name);
    }
}
