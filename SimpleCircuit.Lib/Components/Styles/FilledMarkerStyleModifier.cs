using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style modifier that will represent filled markers.
    /// </summary>
    /// <param name="lineThickness">The line thickness.</param>
    public class FilledMarkerStyleModifier(double? lineThickness = null) : IStyleModifier
    {
        /// <summary>
        /// A default <see cref="FilledMarkerStyleModifier"/> that will copy the line thickness from the parent.
        /// </summary>
        public static FilledMarkerStyleModifier Default { get; } = new FilledMarkerStyleModifier();

        /// <summary>
        /// A default <see cref="FilledMarkerStyleModifier"/> that will have a default line thickness.
        /// </summary>
        public static FilledMarkerStyleModifier DefaultThickness { get; } = new FilledMarkerStyleModifier(Styles.Style.DefaultLineThickness);

        /// <summary>
        /// The style for a <see cref="FilledMarkerStyleModifier"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="lineThickness">The line thickness.</param>
        public class Style(IStyle parent, double? lineThickness) : IStyle
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
            public string StrokeDashArray => _parent.StrokeDashArray;

            /// <inheritdoc />
            public double Justification => _parent.Justification;
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, lineThickness);
    }
}
