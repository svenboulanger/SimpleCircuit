using System;

namespace SimpleCircuit.Drawing.Styles
{
    /// <summary>
    /// A style modifier that affects the line style.
    /// </summary>
    /// <param name="strokeDashArray">The line style.</param>
    public class StrokeDashArrayStyleModifier(string strokeDashArray) : IStyleModifier
    {
        /// <summary>
        /// The style for a <see cref="StrokeDashArrayStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="strokeDashArray">The line style.</param>
        public class Style(IStyle parent, string strokeDashArray) : IStyle
        {
            private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

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
            public string StrokeDashArray => strokeDashArray;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, strokeDashArray);
    }
}
