using System.Text.RegularExpressions;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// A simple text formatter.
    /// </summary>
    public class ElementFormatter : IElementFormatter
    {
        private static readonly Regex _fontSize = new(@"font-size: (?<size>[0-9\.]+)em", RegexOptions.Compiled);
        private static readonly Regex _dy = new(@"(?<dy>[\+\-]?[0-9\.]+)em", RegexOptions.Compiled);

        /// <summary>
        /// Gets the middle-line factor.
        /// </summary>
        public double MidLineFactor { get; set; } = 0.2;

        /// <summary>
        /// Gets the font size.
        /// </summary>
        public double Size { get; set; } = 4.0;

        /// <inheritdoc />
        public Bounds Measure(XmlElement element)
        {
            bool isFirst = true;
            double x = 0, y = -Size * MidLineFactor, h = Size, w = 0;
            foreach (XmlElement e in element.GetElementsByTagName("tspan"))
            {
                string style = e.Attributes["style"]?.Value ?? "";
                var match = _fontSize.Match(style);
                double f = 1;
                if (match.Success)
                    f = double.Parse(match.Groups["size"].Value);

                string dy = e.Attributes["dy"]?.Value ?? "";
                match = _dy.Match(dy);
                if (match.Success)
                {
                    double ny = double.Parse(match.Groups["dy"].Value);
                    double by = (-ny - f * MidLineFactor) * Size;
                    if (by < y)
                        y = by;
                    double ty = (-ny + f) * Size;
                    if (ty > h)
                        h = ty;
                }

                if (e.InnerText.Length > 0)
                {
                    foreach (char c in e.InnerText)
                        w += MeasureCharacter(c) * f;

                    if (isFirst)
                    {
                        isFirst = false;
                        x = MeasureLeftCharacter(e.InnerText[0]);
                    }
                }
            }
            return new(x, -h, w * Size, -y);
        }

        private double MeasureLeftCharacter(char c)
        {
            return c switch
            {
                '/' => -0.039,
                'A' => -0.02,
                'T' => -0.02,
                'V' => -0.02,
                'Y' => -0.02,
                '_' => -0.02,
                'j' => -0.079,
                'v' => -0.02,
                'y' => -0.02,
                'Ʌ' => -0.02,
                'Α' => -0.02,
                'Δ' => -0.02,
                'Λ' => -0.02,
                'Τ' => -0.02,
                'Υ' => -0.02,
                'γ' => -0.02,
                'λ' => -0.02,
                'ν' => -0.02,
                'τ' => -0.02,
                'χ' => -0.02,
                _ => 0,
            };
        }

        private double MeasureCharacter(char c)
        {
            // Values based on Tahoma font
            return c switch
            {
                '!' => 0.443,
                '"' => 0.535,
                '#' => 0.97,
                '$' => 0.728,
                '%' => 1.302,
                '&' => 0.905,
                '\'' => 0.281,
                '(' => 0.51,
                ')' => 0.51,
                '*' => 0.728,
                '+' => 0.97,
                ',' => 0.404,
                '-' => 0.484,
                '.' => 0.404,
                '/' => 0.514,
                char d when char.IsDigit(d) => 0.728,
                ':' => 0.471,
                ';' => 0.471,
                '<' => 0.97,
                '=' => 0.97,
                '>' => 0.97,
                '?' => 0.631,
                '@' => 1.212,
                'A' => 0.804,
                'B' => 0.786,
                'C' => 0.801,
                'D' => 0.904,
                'E' => 0.748,
                'F' => 0.695,
                'G' => 0.89,
                'H' => 0.9,
                'I' => 0.497,
                'J' => 0.555,
                'K' => 0.788,
                'L' => 0.664,
                'M' => 1.027,
                'N' => 0.89,
                'O' => 0.943,
                'P' => 0.735,
                'Q' => 0.943,
                'R' => 0.831,
                'S' => 0.729,
                'T' => 0.725,
                'U' => 0.874,
                'V' => 0.801,
                'W' => 1.202,
                'X' => 0.775,
                'Y' => 0.772,
                'Z' => 0.74,
                '[' => 0.51,
                '\\' => 0.512,
                ']' => 0.51,
                '^' => 0.97,
                '_' => 0.732,
                '`' => 0.728,
                'a' => 0.7,
                'b' => 0.737,
                'c' => 0.612,
                'd' => 0.737,
                'e' => 0.702,
                'f' => 0.423,
                'g' => 0.737,
                'h' => 0.743,
                'i' => 0.306,
                'j' => 0.384,
                'k' => 0.668,
                'l' => 0.308,
                'm' => 1.12,
                'n' => 0.743,
                'o' => 0.724,
                'p' => 0.737,
                'q' => 0.737,
                'r' => 0.482,
                's' => 0.595,
                't' => 0.437,
                'u' => 0.743,
                'v' => 0.667,
                'w' => 0.99,
                'x' => 0.661,
                'y' => 0.667,
                'z' => 0.592,
                '{' => 0.641,
                '|' => 0.51,
                '}' => 0.641,
                '~' => 0.97,
                '±' => 0.97,
                'µ' => 0.757,
                'Α' => 0.804,
                'Β' => 0.786,
                'Γ' => 0.678,
                'Δ' => 0.83,
                'Ε' => 0.748,
                'Ζ' => 0.745,
                'Η' => 0.9,
                'Θ' => 0.943,
                'Ι' => 0.497,
                'Κ' => 0.788,
                'Λ' => 0.801,
                'Μ' => 1.027,
                'Ν' => 0.89,
                'Ξ' => 0.755,
                'Ο' => 0.943,
                'Π' => 0.9,
                'Ρ' => 0.735,
                'Σ' => 0.742,
                'Τ' => 0.783,
                'Υ' => 0.772,
                'Φ' => 0.998,
                'Χ' => 0.775,
                'Ψ' => 1.058,
                'Ω' => 0.936,
                'α' => 0.74,
                'β' => 0.74,
                'γ' => 0.667,
                'δ' => 0.724,
                'ε' => 0.607,
                'ζ' => 0.528,
                'η' => 0.743,
                'θ' => 0.74,
                'ι' => 0.305,
                'κ' => 0.667,
                'λ' => 0.667,
                'μ' => 0.757,
                'ν' => 0.667,
                'ξ' => 0.583,
                'ο' => 0.724,
                'π' => 0.751,
                'ρ' => 0.736,
                'ς' => 0.577,
                'σ' => 0.777,
                'τ' => 0.625,
                'υ' => 0.742,
                'φ' => 0.941,
                'χ' => 0.667,
                'ψ' => 0.952,
                'ω' => 0.966,
                _ => 1.33,
            };
        }
    }
}
