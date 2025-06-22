using System;

namespace SimpleCircuit.Drawing.Styles
{
    /// <summary>
    /// A style that fixes text size and line thickness. It applies the following style modifications:
    /// the line thickness is fixed for over- and underlines, the font size overridden.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="fontSize">The font size.</param>
    public class FontSizeStyleModifier(double fontSize = Style.DefaultFontSize) : IStyleModifier
    {   
        /// <summary>
        /// The style for a <see cref="FontSizeStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="fontSize">The font size.</param>
        public class Style(IStyle parent, double fontSize) : IStyle
        {
            private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            /// <inheritdoc />
            public string Color => _parent.Color;

            /// <inheritdoc />
            public double Opacity => _parent.Opacity;

            /// <inheritdoc />
            public string Background => _parent.Color;

            /// <inheritdoc />
            public double BackgroundOpacity => _parent.Opacity;

            /// <inheritdoc />
            public double LineThickness => _parent.LineThickness;

            /// <inheritdoc />
            public string FontFamily => _parent.FontFamily;

            /// <inheritdoc />
            public double FontSize => fontSize;

            /// <inheritdoc />
            public bool Bold => _parent.Bold;

            /// <inheritdoc />
            public double LineSpacing => _parent.LineSpacing;

            /// <inheritdoc />
            public string StrokeDashArray => _parent.StrokeDashArray;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);

            /// <inheritdoc />
            public bool RegisterVariable(string key, string value) => _parent.RegisterVariable(key, value);
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, fontSize);
    }
}
