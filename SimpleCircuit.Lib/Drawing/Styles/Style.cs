using SimpleCircuit.Components.Variants;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Drawing.Styles
{
    /// <summary>
    /// Describes a style.
    /// </summary>
    public class Style : IStyle
    {
        /// <summary>
        /// Gets the default "light" theme colors.
        /// </summary>
        public static Dictionary<string, string> LightTheme { get; } = new()
        {
            { "foreground", "#212529" }, // From Bootstrap 5
            { "background", "none" },
            { "primary", "#007bff" }, // From Bootstrap 5
            { "secondary", "#6c757d" }, // From Bootstrap 5
            { "success", "#28a745" }, // From Bootstrap 5
            { "warning", "#ffc107" }, // From Bootstrap 5
            { "danger", "#dc3545" }, // From Bootstrap 5
            { "light", "#f8f9fa" }, // From Bootstrap 5
            { "dark", "#343a40" }, // From Bootstrap 5
            { "bg-opaque", "white" },
        };

        /// <summary>
        /// Gets the default "dark" theme colors.
        /// </summary>
        public static Dictionary<string, string> DarkTheme { get; } = new()
        {
            { "foreground", "#dee2e6" }, // From Bootstrap 5
            { "background", "none" },
            { "primary", "#007bff" }, // From Bootstrap 5
            { "secondary", "#6c757d" }, // From Bootstrap 5
            { "success", "#28a745" }, // From Bootstrap 5
            { "warning", "#ffc107" }, // From Bootstrap 5
            { "danger", "#dc3545" }, // From Bootstrap 5
            { "light", "#f8f9fa" }, // From Bootstrap 5
            { "dark", "#343a40" }, // From Bootstrap 5
            { "bg-opaque", "#343a40" },
        };

        /// <summary>
        /// Gets a dictionary of variables.
        /// </summary>
        public Dictionary<string, string> Variables { get; } = [];

        /// <summary>
        /// The default color.
        /// </summary>
        public const string DefaultColor = "black";

        /// <summary>
        /// An identifier representing no color.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// Represents an opaque opacity.
        /// </summary>
        public const double Opaque = 1.0;

        /// <summary>
        /// The default line thickness.
        /// </summary>
        public const double DefaultLineThickness = 0.5;

        /// <summary>
        /// The default font family.
        /// </summary>
        public const string DefaultFontFamily = "Arial";

        /// <summary>
        /// The default font size.
        /// </summary>
        public const double DefaultFontSize = 4.0;

        /// <summary>
        /// the default line spacing.
        /// </summary>
        public const double DefaultLineSpacing = 1.5;

        /// <inheritdoc />
        public string Color { get; set; } = $"--foreground";

        /// <inheritdoc />
        public double Opacity { get; set; } = Opaque;

        /// <inheritdoc />
        public string Background { get; set; } = $"--background";

        /// <inheritdoc />
        public double BackgroundOpacity { get; set; } = Opaque;

        /// <inheritdoc />
        public double LineThickness { get; set; } = DefaultLineThickness;

        /// <inheritdoc />
        public string FontFamily { get; set; } = DefaultFontFamily;

        /// <inheritdoc />
        public double FontSize { get; set; } = DefaultFontSize;

        /// <inheritdoc />
        public bool Bold { get; set; } = false;

        /// <inheritdoc />
        public double LineSpacing { get; set; } = DefaultLineSpacing;

        /// <inheritdoc />
        public string StrokeDashArray { get; set; } = null;

        /// <inheritdoc />
        public double Justification { get; set; } = 1.0;

        /// <summary>
        /// Creates a new <see cref="Style"/>.
        /// </summary>
        /// <param name="theme">The theme colors.</param>
        public Style(IEnumerable<KeyValuePair<string, string>> theme = null)
        {
            if (theme is not null)
                ApplyTheme(theme);
            else
                ApplyTheme(LightTheme);
        }

        /// <inheritdoc />
        public bool TryGetVariable(string key, out string value) => Variables.TryGetValue(key, out value);

        /// <inheritdoc />
        public bool RegisterVariable(string key, string value)
        {
            if (Variables.ContainsKey(key))
                return false;
            Variables.Add(key, value);
            return true;
        }

        /// <summary>
        /// Apply colors for a theme.
        /// </summary>
        /// <param name="pairs">The key-value pairs that represent theme colors.</param>
        public void ApplyTheme(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
                Variables[pair.Key] = pair.Value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string[] items = [
                $"color=\"{Color}\"",
                $"opacity=\"{Opacity.ToSVG()}\"",
                $"bg=\"{Background}\"",
                $"bgo=\"{BackgroundOpacity.ToSVG()}\"",
                $"thickness=\"{LineThickness.ToSVG()}\"",
                $"fontfamily=\"{FontFamily}\"",
                $"fontsize=\"{FontSize.ToSVG()}\"",
                $"bold={(Bold ? "true" : "false")}",
                $"linestyle=\"{StrokeDashArray}\"",
                $"justification=\"{Justification.ToSVG()}\""
                ];
            return string.Join(", ", items);
        }
    }
}
