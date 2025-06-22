using System;

namespace SimpleCircuit.Drawing.Styles
{
    /// <summary>
    /// A style that will override styling to remove any background color.
    /// </summary>
    /// <param name="parent">The parent appearance.</param>
    public class NoFillStyleModifier : IStyleModifier
    {
        /// <summary>
        /// A style for a <see cref="NoFillStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        public class Style(IStyle parent) : IStyle
        {
            private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            /// <inheritdoc />
            public string Color => _parent.Color;

            /// <inheritdoc />
            public double Opacity => _parent.Opacity;

            /// <inheritdoc />
            public string Background => Styles.Style.None;

            /// <inheritdoc />
            public double BackgroundOpacity => Styles.Style.Opaque;

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
            public string StrokeDashArray => _parent.StrokeDashArray;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);

            /// <inheritdoc />
            public bool RegisterVariable(string key, string value) => _parent.RegisterVariable(key, value);
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent);
    }
}