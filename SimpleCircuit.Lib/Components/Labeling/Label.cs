using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;

namespace SimpleCircuit.Components.Labeling
{
    /// <summary>
    /// Label information.
    /// </summary>
    public class Label : IStyled
    {
        /// <summary>
        /// Gets or sets the value of the label.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the formatted text.
        /// </summary>
        public Span Formatted { get; private set; }

        /// <inheritdoc />
        public IStyleModifier Style { get; set; }

        /// <summary>
        /// Gets or sets the anchor to which the label will attach.
        /// </summary>
        public string Anchor { get; set; }

        /// <summary>
        /// Gets or sets an offset that can be assigned to tweak the label position.
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// Formats the label.
        /// </summary>
        /// <param name="formatter">A text formatter.</param>
        /// <param name="parentStyle">The parent style.</param>
        public void Format(ITextFormatter formatter, IStyle parentStyle) => Formatted = formatter.Format(Value, Style?.Apply(parentStyle) ?? parentStyle);
    }
}
