using System;

namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// A style modifier that changes the text justification.
    /// </summary>
    /// <param name="justification">The new justification.</param>
    public class JustificationStyleModifier(double justification) : IStyleModifier
    {
        /// <summary>
        /// The <see cref="IStyle"/>.
        /// </summary>
        /// <param name="parent">The parent style.</param>
        /// <param name="justification">The justification.</param>
        public class Style(IStyle parent, double justification) : IStyle
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
            public double Justification => justification;

            /// <inheritdoc />
            public string StrokeDashArray => _parent.StrokeDashArray;
        }

        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => new Style(parent, justification);
    }
}
