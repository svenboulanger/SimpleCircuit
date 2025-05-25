using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style modifier that affects color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="backgroundColor">The background color.</param>
    public class ColorStyleModifier(string color, string backgroundColor) : IStyleModifier
    {
        /// <summary>
        /// The style for a <see cref="ColorStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="color">The color.</param>
        /// <param name="backgroundColor">The background color.</param>
        public class Style(IStyle parent, string color, string backgroundColor) : IStyle
        {
            private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            /// <inheritdoc />
            public string Color => color ?? _parent.Color;

            /// <inheritdoc />
            public double Opacity => _parent.Opacity;

            /// <inheritdoc />
            public string Background => backgroundColor ?? _parent.Background;

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
            public LineStyles LineStyle => _parent.LineStyle;

            /// <inheritdoc />
            public double Justification => _parent.Justification;
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, color, backgroundColor);
    }
}
