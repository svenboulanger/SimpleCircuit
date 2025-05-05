using SimpleCircuit.Components.Styles;
using System.Text;

namespace SimpleCircuit.Components.Styles
{
    public static class StyleHelpers
    {
        /// <summary>
        /// Creates a style attribute value for strokes and no fill that represents the <see cref="IStyle"/>.
        /// Lines do not need to have any background color.
        /// </summary>
        /// <param name="appearance">The appearance.</param>
        /// <returns>Returns the style attribute value.</returns>
        public static string CreateStrokeStyle(this IStyle appearance)
        {
            var style = new StringBuilder();

            // Deal with the foreground
            if (appearance.Opacity.IsZero())
                style.Append("stroke: none; ");
            else if ((appearance.Opacity - Style.Opaque).IsZero() || appearance.Opacity > Style.Opaque)
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
            style.Append("fill: none;");

            return style.ToString();
        }

        /// <summary>
        /// Creates a style attribute value for strokes with fill that represents the <see cref="IStyle"/>.
        /// </summary>
        /// <param name="appearance">The style.</param>
        /// <returns>Returns the style attribute value.</returns>
        public static string CreateStrokeFillStyle(this IStyle appearance)
        {
            var style = new StringBuilder();

            // Deal with the foreground
            if (appearance.Opacity.IsZero())
                style.Append("stroke: none; ");
            else if ((appearance.Opacity - Style.Opaque).IsZero() || appearance.Opacity > Style.Opaque)
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
                case LineStyles.Dashed:
                    style.Append($"stroke-dasharray: {(appearance.LineThickness * 4).ToSVG()} {(appearance.LineThickness * 4).ToSVG()}; ");
                    break;

                case LineStyles.Dotted:
                    style.Append($"stroke-dasharray: {appearance.LineThickness.ToSVG()} {(appearance.LineThickness * 4).ToSVG()}; ");
                    break;
            }

            // Deal with the background
            if (appearance.BackgroundOpacity.IsZero())
                style.Append("fill: none;");
            else if ((appearance.BackgroundOpacity - Style.Opaque).IsZero() || appearance.BackgroundOpacity > Style.Opaque)
                style.Append($"fill: {appearance.Background};");
            else
            {
                style.Append($"fill: {appearance.Background};");
                style.Append($"fill-opacity: {appearance.BackgroundOpacity.ToSVG()};");
            }
            return style.ToString();
        }

        /// <summary>
        /// Creats a style attribute for text that represents the <see cref="IStyle"/>.
        /// </summary>
        /// <param name="appearance">The style.</param>
        /// <returns>Returns the style attribute value.</returns>
        public static string CreateTextStyle(this IStyle appearance)
        {
            var sb = new StringBuilder();
            sb.Append($"font-family: {appearance.FontFamily}; ");
            sb.Append($"font-size: {appearance.FontSize.ToSVG()}pt; ");
            if (appearance.Opacity.IsZero())
                sb.Append($"fill: none; ");
            else if ((appearance.Opacity - Style.Opaque).IsZero() || appearance.Opacity > Style.Opaque)
                sb.Append($"fill: {appearance.Color}; ");
            else
                sb.Append($"fill: {appearance.Color}; fill-opacity: {appearance.Opacity}; ");
            if (appearance.Bold)
                sb.Append("font-weight: bold; ");
            sb.Append("stroke: none;");
            return sb.ToString();
        }
    }

}
