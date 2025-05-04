using SimpleCircuit.Components.Builders;
using System.Text;

namespace SimpleCircuit.Components.Appearance
{
    public static class AppearanceHelpers
    {
        /// <summary>
        /// Creates path options with line style based on the drawable variants.
        /// </summary>
        /// <param name="appearance">The appearance.</param>
        /// <param name="drawable">The drawable.</param>
        /// <returns>The graphic options.</returns>
        public static GraphicOptions CreatePathOptions(this IAppearanceOptions appearance, IDrawable drawable)
            => CreatePathOptions(new LineStyleAppearance(appearance, drawable.Variants.Select("dashed", "dotted")));

        public static GraphicOptions CreateMarkerOptions(this IAppearanceOptions appearance)
            => CreatePathOptions(new LineMarkerAppearanceOptions(appearance));

        /// <summary>
        /// Creates <see cref="GraphicOptions"/> for a given appearance.
        /// </summary>
        /// <param name="appearance">The appearance.</param>
        /// <returns>Returns the <see cref="GraphicOptions"/>.</returns>
        public static GraphicOptions CreatePathOptions(this IAppearanceOptions appearance)
        {
            var options = new GraphicOptions();

            // Deal with the foreground
            if (appearance.Opacity.IsZero())
                options.Style["stroke"] = "none";
            else if ((appearance.Opacity - AppearanceOptions.Opaque).IsZero() || appearance.Opacity > AppearanceOptions.Opaque)
                options.Style["stroke"] = appearance.Color;
            else
            {
                options.Style["stroke"] = appearance.Color;
                options.Style["stroke-opacity"] = appearance.Opacity.ToSVG();
            }

            // Path options
            options.Style["stroke-width"] = $"{appearance.LineThickness.ToSVG()}pt";
            options.Style["stroke-linecap"] = "round";
            options.Style["stroke-linejoin"] = "round";

            // Allow other path options
            switch (appearance.LineStyle)
            {
                case 0:
                    options.Style["stroke-dasharray"] = $"{(appearance.LineThickness * 4).ToSVG()} {(appearance.LineThickness * 4).ToSVG()}";
                    break;

                case 1:
                    options.Style["stroke-dasharray"] = $"{appearance.LineThickness.ToSVG()} {(appearance.LineThickness * 4).ToSVG()};";
                    break;
            }

            // Deal with the background
            if (appearance.BackgroundOpacity.IsZero())
                options.Style["fill"] = "none";
            else if ((appearance.BackgroundOpacity - AppearanceOptions.Opaque).IsZero() || appearance.BackgroundOpacity > AppearanceOptions.Opaque)
                options.Style["fill"] = appearance.Background;
            else
            {
                options.Style["fill"] = appearance.Background;
                options.Style["fill-opacity"] = appearance.BackgroundOpacity.ToSVG();
            }
            return options;
        }

        /// <summary>
        /// Creates a style attribute value for strokes and no fill that represents the <see cref="IAppearanceOptions"/>.
        /// Lines do not need to have any background color.
        /// </summary>
        /// <param name="appearance">The appearance.</param>
        /// <returns>Returns the style attribute value.</returns>
        public static string CreateStrokeStyle(this IAppearanceOptions appearance)
        {
            var style = new StringBuilder();

            // Deal with the foreground
            if (appearance.Opacity.IsZero())
                style.Append("stroke: none; ");
            else if ((appearance.Opacity - AppearanceOptions.Opaque).IsZero() || appearance.Opacity > AppearanceOptions.Opaque)
                style.Append($"stroke: {appearance.Color}; ");
            else
            {
                style.Append($"stroke: {appearance.Color}; ");
                style.Append($"stroke-opacity: {appearance.Opacity.ToSVG()};");
            }

            // Path options
            style.Append($"stroke-width: {appearance.LineThickness.ToSVG()}pt; ");
            style.Append("stroke-linecap: round; ");
            style.Append("stroke-linejoin: round; ");

            return style.ToString();
        }

        /// <summary>
        /// Creates a style attribute value for strokes with fill that represents the <see cref="IAppearanceOptions"/>.
        /// </summary>
        /// <param name="appearance">The style.</param>
        /// <returns>Returns the style attribute value.</returns>
        public static string CreateStrokeFillStyle(this IAppearanceOptions appearance)
        {
            var style = new StringBuilder();

            // Deal with the foreground
            if (appearance.Opacity.IsZero())
                style.Append("stroke: none; ");
            else if ((appearance.Opacity - AppearanceOptions.Opaque).IsZero() || appearance.Opacity > AppearanceOptions.Opaque)
                style.Append($"stroke: {appearance.Color}; ");
            else
            {
                style.Append($"stroke: {appearance.Color}; ");
                style.Append($"stroke-opacity: {appearance.Opacity.ToSVG()};");
            }

            // Path options
            style.Append($"stroke-width: {appearance.LineThickness.ToSVG()}pt; ");
            style.Append("stroke-linecap: round; ");
            style.Append("stroke-linejoin: round; ");

            // Allow other path options
            switch (appearance.LineStyle)
            {
                case 0:
                    style.Append($"stroke-dasharray: {(appearance.LineThickness * 4).ToSVG()} {(appearance.LineThickness * 4).ToSVG()}; ");
                    break;

                case 1:
                    style.Append($"stroke-dasharray: {appearance.LineThickness.ToSVG()} {(appearance.LineThickness * 4).ToSVG()}; ");
                    break;
            }

            // Deal with the background
            if (appearance.BackgroundOpacity.IsZero())
                style.Append("fill: none;");
            else if ((appearance.BackgroundOpacity - AppearanceOptions.Opaque).IsZero() || appearance.BackgroundOpacity > AppearanceOptions.Opaque)
                style.Append($"fill: {appearance.Background};");
            else
            {
                style.Append($"fill: {appearance.Background};");
                style.Append($"fill-opacity: {appearance.BackgroundOpacity.ToSVG()};");
            }
            return style.ToString();
        }
    }

}
