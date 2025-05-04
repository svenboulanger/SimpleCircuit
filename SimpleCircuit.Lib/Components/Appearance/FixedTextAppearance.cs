using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Appearance
{
    public class FixedTextAppearance(IAppearanceOptions parent, double? fontSize = AppearanceOptions.DefaultFontSize, double? lineThickness = 0.1) : IAppearanceOptions
    {
        private readonly IAppearanceOptions _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => _parent.Opacity;

        /// <inheritdoc />
        public string Background => _parent.Color;

        /// <inheritdoc />
        public double BackgroundOpacity => _parent.Opacity;

        /// <inheritdoc />
        public double LineThickness => lineThickness ?? _parent.LineThickness;

        /// <inheritdoc />
        public string FontFamily => _parent.FontFamily;

        /// <inheritdoc />
        public double FontSize => fontSize ?? _parent.FontSize;

        /// <inheritdoc />
        public bool Bold => _parent.Bold;

        /// <inheritdoc />
        public double LineSpacing => _parent.LineSpacing;

        /// <inheritdoc />
        public int LineStyle => _parent.LineStyle;
    }
}
