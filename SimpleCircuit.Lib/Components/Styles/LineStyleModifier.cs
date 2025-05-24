using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style modifier that affects the line style.
    /// </summary>
    /// <param name="lineStyle">The line style.</param>
    public class LineStyleModifier(LineStyles lineStyle) : IStyleModifier
    {
        /// <summary>
        /// The style for a <see cref="LineStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="lineStyle">The line style.</param>
        public class Style(IStyle parent, LineStyles lineStyle) : IStyle
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
            public LineStyles LineStyle => lineStyle;
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, lineStyle);
    }
}
