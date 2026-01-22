using System;

namespace SimpleCircuit.Drawing.Styles;

/// <summary>
/// A style modifier that affects opacity.
/// </summary>
/// <param name="opacity">The opacity.</param>
/// <param name="backgroundOpacity">The background opacity.</param>
public class OpacityStyleModifier(double? opacity, double? backgroundOpacity) : IStyleModifier
{
    /// <summary>
    /// A style for a <see cref="OpacityStyleModifier"/>.
    /// </summary>
    /// <param name="parent">The parent style.</param>
    /// <param name="opacity">The foreground opacity.</param>
    /// <param name="backgroundOpacity">The background opacity.</param>
    public class Style(IStyle parent, double? opacity, double? backgroundOpacity) : IStyle
    {
        private readonly IStyle _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color => _parent.Color;

        /// <inheritdoc />
        public double Opacity => opacity ?? _parent.Opacity;

        /// <inheritdoc />
        public string Background => _parent.Background;

        /// <inheritdoc />
        public double BackgroundOpacity => backgroundOpacity ?? _parent.BackgroundOpacity;

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
    public IStyle Apply(IStyle parent) => new Style(parent, opacity, backgroundOpacity);
}
