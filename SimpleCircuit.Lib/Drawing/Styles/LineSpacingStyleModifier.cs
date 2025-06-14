﻿using System;

namespace SimpleCircuit.Drawing.Styles
{
    /// <summary>
    /// A style modifier that changes line spacing.
    /// </summary>
    /// <param name="lineSpacing">The line spacing.</param>
    public class LineSpacingStyleModifier(double lineSpacing) : IStyleModifier
    {
        /// <summary>
        /// The style for a <see cref="LineSpacingStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="lineSpacing">The line spacing.</param>
        public class Style(IStyle parent, double lineSpacing) : IStyle
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
            public double LineSpacing => lineSpacing;

            /// <inheritdoc />
            public string StrokeDashArray => _parent.StrokeDashArray;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, lineSpacing);
    }
}
