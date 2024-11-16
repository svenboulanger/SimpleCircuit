using SimpleCircuit.Components;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser.SimpleTexts;
using System;

namespace SimpleCircuit.Circuits.Contexts
{
    /// <summary>
    /// An implementation of the <see cref="IPrepareContext"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="PrepareContext"/>.
    /// </remarks>
    /// <param name="circuit">The circuit.</param>
    /// <param name="diagnostics">The diagnostics.</param>
    public class PrepareContext(GraphicalCircuit circuit, ITextMeasurer measurer, IDiagnosticHandler diagnostics) : IPrepareContext
    {
        private readonly GraphicalCircuit _circuit = circuit ?? throw new ArgumentNullException(nameof(circuit));

        /// <inheritdoc />
        public IDiagnosticHandler Diagnostics { get; } = diagnostics;

        /// <inheritdoc />
        public DesperatenessLevel Desparateness { get; set; } = DesperatenessLevel.Normal;

        /// <inheritdoc />
        public PreparationMode Mode { get; set; }

        /// <inheritdoc />
        public NodeOffsetFinder Offsets { get; } = new();

        /// <inheritdoc />
        public NodeGrouper Groups { get; } = new();


        /// <inheritdoc />
        public ICircuitPresence Find(string name)
        {
            if (_circuit.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <inheritdoc />
        public ISpan Format(string content, double fontSize = 4.0, bool isBold = false, GraphicOptions options = null)
        {
            var lexer = new SimpleTextLexer(content);
            var context = new SimpleTextContext(measurer);
            context.FontSize = fontSize;
            context.IsBold = isBold;
            return SimpleTextParser.Parse(lexer, context);
        }
    }
}
