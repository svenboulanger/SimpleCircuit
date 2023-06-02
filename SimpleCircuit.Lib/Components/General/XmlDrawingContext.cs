using SimpleCircuit.Drawing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleCircuit.Components.General
{
    public class XmlDrawingContext : IXmlDrawingContext
    {
        private readonly Labels _labels;

        /// <summary>
        /// Creates a new <see cref="XmlDrawingContext"/>.
        /// </summary>
        /// <param name="labels"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public XmlDrawingContext(Labels labels)
        {
            _labels = labels ?? throw new ArgumentNullException(nameof(labels));
        }

        /// <inheritdoc />
        public string TransformText(string input)
        {
            // Simple substitution of \# with the corresponding label.
            return Regex.Replace(input, @"(?!<\\)\\(\d+)", match =>
            {
                int index = int.Parse(match.Groups[1].Value);
                return _labels[index] ?? "";
            });
        }
    }
}
