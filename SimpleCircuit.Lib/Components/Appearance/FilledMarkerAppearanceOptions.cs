using System;

namespace SimpleCircuit.Components.Appearance
{
    /// <summary>
    /// An <see cref="IAppearanceOptions"/> that fills up any shapes with the foreground color.
    /// </summary>
    public class FilledMarkerAppearanceOptions(IAppearanceOptions parent) : IAppearanceOptions
    {
        private readonly IAppearanceOptions _parent = parent ?? throw new ArgumentNullException(nameof(parent));

        /// <inheritdoc />
        public string Color { get => _parent.Color; set { } }

        /// <inheritdoc />
        public double Opacity { get => _parent.Opacity; set { } }

        /// <inheritdoc />
        public string Background { get => _parent.Color; set { } }

        /// <inheritdoc />
        public double BackgroundOpacity { get => _parent.Opacity; set { } }

        /// <inheritdoc />
        public double LineThickness { get => _parent.LineThickness; set { } }

        /// <inheritdoc />
        public string FontFamily { get => _parent.FontFamily; set { } }

        /// <inheritdoc />
        public double FontSize { get => _parent.FontSize; set { } }

        /// <inheritdoc />
        public bool Bold { get => _parent.Bold; set { } }

        /// <inheritdoc />
        public double LineSpacing { get => _parent.LineSpacing; set { } }

        /// <inheritdoc />
        public int LineStyle { get => -1; set { } }
    }
}
