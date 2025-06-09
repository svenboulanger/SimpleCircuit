using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style modifier that changes or overrides variables for styles.
    /// </summary>
    public class VariableStyleModifier : IStyleModifier
    {
        /// <summary>
        /// Gets the variables that will override parent variables.
        /// </summary>
        public Dictionary<string, string> Variables { get; } = [];

        /// <summary>
        /// A style that modifies the variables.
        /// </summary>
        public class Style(IStyle parent, IReadOnlyDictionary<string, string> variables) : IStyle
        {
            private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            private readonly IReadOnlyDictionary<string, string> _variables = variables ?? throw new ArgumentNullException(nameof(variables));

            /// <inheritdoc />
            public string Color => _parent.Color;

            /// <inheritdoc />
            public double Opacity => _parent.Opacity;

            /// <inheritdoc />
            public string Background => _parent.Background;

            /// <inheritdoc />
            public double BackgroundOpacity => _parent.BackgroundOpacity;

            /// <inheritdoc />
            public double LineThickness => _parent.LineThickness;

            /// <inheritdoc />
            public string FontFamily => _parent.FontFamily;

            /// <inheritdoc />
            public double FontSize => _parent.FontSize;

            /// <inheritdoc />
            public bool Bold => _parent.Bold;

            /// <inheritdoc />
            public double LineSpacing => _parent.LineSpacing;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public string StrokeDashArray => _parent.StrokeDashArray;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value)
            {
                // First try to find locally
                if (_variables.TryGetValue(key, out value))
                    return true;

                // If it doesn't exist, refer to parent style
                return _parent.TryGetVariable(key, out value);
            }
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, Variables);
    }
}
