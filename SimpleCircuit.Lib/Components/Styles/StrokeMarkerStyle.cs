using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style modifier that turns a style suitable for non-filled markers.
    /// </summary>
    /// <param name="lineThickness">The line thickness.</param>
    public class StrokeMarkerStyleModifier(double? lineThickness) : IStyleModifier
    {
        /// <summary>
        /// A stroke marker with default line thickness.
        /// </summary>
        public static StrokeMarkerStyleModifier Default { get; } = new StrokeMarkerStyleModifier(Styles.Style.DefaultLineThickness);

        /// <summary>
        /// A style for a <see cref="StrokeMarkerStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="lineThickness">The line thickness.</param>
        public class Style(IStyle parent, double? lineThickness = null) : IStyle
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
            public double LineThickness => lineThickness ?? _parent.LineThickness;

            /// <inheritdoc />
            public string FontFamily => _parent.FontFamily;

            /// <inheritdoc />
            public double FontSize => _parent.FontSize;

            /// <inheritdoc />
            public bool Bold => _parent.Bold;

            /// <inheritdoc />
            public double LineSpacing => _parent.LineSpacing;

            /// <inheritdoc />
            public string StrokeDashArray => null;

            /// <inheritdoc />
            public double Justification => _parent.Justification;

            /// <inheritdoc />
            public bool TryGetVariable(string key, out string value) => _parent.TryGetVariable(key, out value);
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, lineThickness);
    }
}