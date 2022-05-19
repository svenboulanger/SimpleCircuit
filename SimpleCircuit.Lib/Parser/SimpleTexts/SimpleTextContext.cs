using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser.SimpleTexts
{
    /// <summary>
    /// A context for parsing SimpleCircuit text.
    /// </summary>
    public class SimpleTextContext
    {
        private readonly StringBuilder _sb = new();
        private readonly List<string> _lines = new();
        private readonly Stack<string> _tags = new();
        private double _dy = 0.0, _emSize = 1.0;

        /// <summary>
        /// Gets the base line y.
        /// </summary>
        public double BaselineY { get; set; }

        /// <summary>
        /// Gets or sets the relative font weight.
        /// </summary>
        public double RelativeFontWeight { get; set; } = 1.0;

        /// <summary>
        /// Gets all the lines.
        /// </summary>
        public IEnumerable<string> Lines
        {
            get
            {
                foreach (string line in _lines)
                    yield return line;
                if (_sb.Length > 0)
                    yield return _sb.ToString() + "</tspan>";
            }
        }

        /// <summary>
        /// Creates a new <see cref="SimpleTextContext"/>.
        /// </summary>
        public SimpleTextContext()
        {
            _sb.Append("<tspan>");
        }

        /// <summary>
        /// Appends text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Append(string text)
        {
            UpdateBaseline();
            _sb.Append(text);
        }

        /// <summary>
        /// Appends a character.
        /// </summary>
        /// <param name="c">The character.</param>
        public void Append(char c)
        {
            UpdateBaseline();
            _sb.Append(c);
        }

        private void UpdateBaseline()
        {
            if (_dy != BaselineY)
            {
                // Stop the old tspan
                _sb.Append("</tspan>");

                // The font size is important here...
                double dy = (BaselineY - _dy) / RelativeFontWeight;
                _sb.Append($"<tspan dy=\"{dy:G3}em\"{(RelativeFontWeight != 1.0 ? $" style=\"font-size: {RelativeFontWeight:G3}em\"" : "")}>");
                _dy = BaselineY;
                _emSize = RelativeFontWeight;
            }
        }

        /// <summary>
        /// Adds a new line.
        /// </summary>
        public void Newline()
        {
            // Stop the current tspan
            Append("</tspan>");
            _lines.Add(_sb.ToString());

            // Start fresh
            _sb.Clear();
            Append("<tspan>");
        }

        /// <summary>
        /// Clears the context.
        /// </summary>
        public void Clear()
        {
            _sb.Clear();
            _lines.Clear();
            _tags.Clear();
            _dy = BaselineY = 0.0;
            _emSize = RelativeFontWeight = 1.0;
        }
    }
}
