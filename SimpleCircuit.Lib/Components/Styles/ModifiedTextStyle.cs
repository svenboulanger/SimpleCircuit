using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A text style modifier.
    /// </summary>
    /// <param name="color">The text color.</param>
    /// <param name="fontFamily">The font family.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="bold">If <c>true</c>, the text will be bold.</param>
    /// <param name="lineSpacing">The line spacing.</param>
    public class TextStyleModifier(string color, string fontFamily, double? fontSize, bool? bold, double? lineSpacing) : IStyleModifier
    {
        /// <summary>
        /// Creates a style for a <see cref="TextStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="color">the color.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">the font size.</param>
        /// <param name="bold">Bold text.</param>
        /// <param name="lineSpacing">The line spacing.</param>
        public class Style(IStyle parent, string color, string fontFamily, double? fontSize, bool? bold, double? lineSpacing) : IStyle
        {
            private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            /// <inheritdoc />
            public string Color => color ?? _parent.Color;

            /// <inheritdoc />
            public double Opacity => _parent.Opacity;

            /// <inheritdoc />
            public string Background => _parent.Background;

            /// <inheritdoc />
            public double BackgroundOpacity => _parent.BackgroundOpacity;

            /// <inheritdoc />
            public double LineThickness => _parent.LineThickness;

            /// <inheritdoc />
            public string FontFamily => fontFamily ?? _parent.FontFamily;

            /// <inheritdoc />
            public double FontSize => fontSize ?? _parent.FontSize;

            /// <inheritdoc />
            public bool Bold => bold ?? _parent.Bold;

            /// <inheritdoc />
            public double LineSpacing => lineSpacing ?? _parent.LineSpacing;

            /// <inheritdoc />
            public string StrokeDashArray => _parent.StrokeDashArray;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, color, fontFamily, fontSize, bold, lineSpacing);
    }
}