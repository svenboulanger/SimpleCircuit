using SimpleCircuit.Components.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Components.Styles
{
    public class LineThicknessAppearance(IStyle parent, double lineThickness = Style.DefaultLineThickness) : IStyle
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
        public double LineThickness => lineThickness;

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
    }
}