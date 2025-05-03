using SimpleCircuit.Components.Builders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SimpleCircuit.Components
{
    public class AppearanceOptions
    {
        public const string Dashed = "dashed";
        public const string Dotted = "dotted";

        public const string Black = "black";
        public const string White = "white";
        public const string None = "none";
        public const double Opaque = 1.0;
        public const double Transparent = 0.0;
        public const double DefaultLineThickness = 0.5;
        public const double DefaultFontSize = 4.0;
        public const string DefaultFontFamily = "Arial";

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public string Color { get; set; } = Black;

        /// <summary>
        /// Gets or sets the opacity.
        /// </summary>
        public double Opacity { get; set; } = Opaque;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public string Background { get; set; } = None;

        /// <summary>
        /// Gets or sets the background color opacity.
        /// </summary>
        public double BackgroundOpacity { get; set; } = Opaque;

        /// <summary>
        /// Gets or sets the line thickness.
        /// </summary>
        public double LineThickness { get; set; } = DefaultLineThickness;

        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        public string FontFamily { get; set; } = DefaultFontFamily;

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double FontSize { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets whether text appears bold.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Gets or sets the line spacing of text lines.
        /// </summary>
        public double LineSpacing { get; set; } = 1.5;

        /// <summary>
        /// Creates graphic options from the current appearance.
        /// </summary>
        /// <param name="parent">An optional drawable that can allow certain extra options.</param>
        /// <returns>Returns the graphic options.</returns>
        public GraphicOptions CreatePathOptions(IDrawable parent = null)
        {
            var options = new GraphicOptions();

            // Deal with the foreground
            if (Opacity.IsZero())
                options.Style["stroke"] = "none";
            else if ((Opacity - Opaque).IsZero() || Opacity > Opaque)
                options.Style["stroke"] = Color;
            else
            {
                options.Style["stroke"] = Color;
                options.Style["stroke-opacity"] = Opacity.ToSVG();
            }

            // Path options
            options.Style["stroke-width"] = $"{LineThickness.ToSVG()}pt";
            options.Style["stroke-linecap"] = "round";
            options.Style["stroke-linejoin"] = "round";
            if (parent is not null)
            {
                // Allow other path options
                switch (parent.Variants.Select(Dashed, Dotted))
                {
                    case 0:
                        options.Style["stroke-dasharray"] = $"{(LineThickness * 4).ToSVG()} {(LineThickness * 4).ToSVG()}";
                        break;

                    case 1:
                        options.Style["stroke-dasharray"] = $"{LineThickness.ToSVG()} {(LineThickness * 4).ToSVG()};";
                        break;
                }
            }

            // Deal with the background
            if (BackgroundOpacity.IsZero())
                options.Style["fill"] = "none";
            else if ((BackgroundOpacity - Opaque).IsZero() || BackgroundOpacity > Opaque)
                options.Style["fill"] = Background;
            else
            {
                options.Style["fill"] = Background;
                options.Style["fill-opacity"] = BackgroundOpacity.ToSVG();
            }
            return options;
        }

        /// <summary>
        /// Creates graphic options for markers.
        /// </summary>
        /// <returns>The graphic options.</returns>
        public GraphicOptions CreateMarkerOptions(bool hasStroke = true, bool hasFill = true, bool scaleStrokeWidth = true)
        {
            var options = new GraphicOptions();

            // Deal with the foreground
            if (Opacity.IsZero())
            {
                options.Style["stroke"] = None;
                options.Style["fill"] = None;
                hasStroke = false;
            }
            else if ((Opacity - Opaque).IsZero() || Opacity > Opaque)
            {
                options.Style["stroke"] = hasStroke ? Color : None;
                options.Style["fill"] = hasFill ? Color : None;
            }
            else
            {
                options.Style["stroke"] = hasStroke ? Color : None;
                options.Style["fill"] = hasFill ? Color : None;
                string opacity = Opacity.ToSVG();
                if (hasStroke)
                    options.Style["stroke-opacity"] = opacity;
                if (hasFill)
                    options.Style["fill-opacity"] = opacity;
            }

            // Stroke width
            if (hasStroke)
            {
                if (scaleStrokeWidth)
                    options.Style["stroke-width"] = $"{LineThickness.ToSVG()}pt";
                else
                    options.Style["stroke-width"] = $"{DefaultLineThickness.ToSVG()}pt";
                options.Style["stroke-linecap"] = "round";
                options.Style["stroke-linejoin"] = "round";
            }

            return options;
        }

        /// <summary>
        /// Gets a cloned appearance object.
        /// </summary>
        /// <returns>Returns the cloned appearance options.</returns>
        public AppearanceOptions Clone() => (AppearanceOptions)MemberwiseClone();
    }
}
