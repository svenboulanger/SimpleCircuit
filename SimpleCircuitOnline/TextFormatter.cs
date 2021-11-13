using SimpleCircuit.Drawing;
using System;

namespace SimpleCircuitOnline
{
    public class TextFormatter : ITextFormatter
    {
        private readonly Func<string, double, Bounds> _method;

        public TextFormatter(Func<string, double, Bounds> method)
        {
            _method = method;
        }

        public FormattedText Format(string text, double size)
        {
            var bounds = _method(text, size);
            return new(text, bounds);
        }
    }
}
