using System;

namespace SimpleCircuit.Drawing.Styles;

/// <summary>
/// A style modifier that changes the stroke width.
/// </summary>
/// <param name="lineThickness">The </param>
public class StrokeWidthStyleModifier(double lineThickness = Style.DefaultLineThickness) : IStyleModifier
{
    /// <summary>
    /// A style for a <see cref="StrokeWidthStyleModifier"/>.
    /// </summary>
    /// <param name="parent">The parent style.</param>
    /// <param name="lineThickness">The line thickness.</param>
    public class Style(IStyle parent, double lineThickness) : IStyle
    {
        private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => _parent.Opacity;

        /// <inheritdoc />
        public string Background => _parent.Background;

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
        public string StrokeDashArray => _parent.StrokeDashArray;

        /// <inheritdoc />
        public double Justification => _parent.Justification;

        /// <inheritdoc />
        public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);

        /// <inheritdoc />
        public bool RegisterVariable(string key, string value) => _parent.RegisterVariable(key, value);
    }

    /// <inheritdoc />
    public IStyle Apply(IStyle parent) => new Style(parent, lineThickness);
}