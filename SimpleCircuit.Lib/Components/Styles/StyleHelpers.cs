using System.Text;

namespace SimpleCircuit.Components.Styles
{
    public static class StyleHelpers
    {
        /// <summary>
        /// Appends a style to an <see cref="IStyled"/>.
        /// </summary>
        /// <param name="styled">The styled item.</param>
        /// <param name="modifier">The style modifier.</param>
        public static void AppendStyle(this IStyled styled, IStyleModifier modifier)
        {
            if (styled.Style is not null)
                styled.Style = new AggregateStyleModifier(styled.Style, modifier);
            else
                styled.Style = modifier;
        }

        /// <summary>
        /// Applies style modifier based on variants to make a dashed or dotted line.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public static void ApplyDrawableLineStyle(this IDrawable drawable)
        {
            switch (drawable.Variants.Select("dashed", "dotted"))
            {
                case 0:
                    drawable.AppendStyle(new LineStyleModifier(LineStyles.Dashed));
                    break;

                case 1:
                    drawable.AppendStyle(new LineStyleModifier(LineStyles.Dotted));
                    break;
            }
        }

        /// <summary>
        /// Gets a style that overrides another style to have the same fill as stroke color.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="lineThickness">An optional line thickness.</param>
        /// <returns>Returns the style.</returns>
        public static IStyle AsFilledMarker(this IStyle style, double? lineThickness = null) => new FilledMarkerStyleModifier.Style(style, lineThickness);

        /// <summary>
        /// Gets a style that overrides another style to have no fill and no line style.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="lineThickness">An optional line thickness.</param>
        /// <returns></returns>
        public static IStyle AsStrokeMarker(this IStyle style, double? lineThickness = null) => new StrokeMarkerStyleModifier.Style(style, lineThickness);

        /// <summary>
        /// Gets a style that overrides another style to have no fill.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <returns>Returns the style.</returns>
        public static IStyle AsStroke(this IStyle style) => new NoFillStyleModifier.Style(style);

        /// <summary>
        /// Gets a style that overrides another style to have a fixed line thickness.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="thickness">The line thickness.</param>
        /// <returns>Returns the style.</returns>
        public static IStyle AsLineThickness(this IStyle style, double thickness) => new StrokeWidthStyleModifier.Style(style, thickness);

        /// <summary>
        /// Gets a style that is modified by the given <see cref="IStyleModifier"/>.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="styleModifier">The style modifier.</param>
        /// <returns>Returns the style.</returns>
        public static IStyle Modify(this IStyle style, IStyleModifier styleModifier) => styleModifier?.Apply(style) ?? style;

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

            // Allow other path options
            switch (appearance.LineStyle)
            {
                case LineStyles.Dashed:
                    style.Append($"stroke-dasharray: {(appearance.LineThickness * 4).ToSVG()} {(appearance.LineThickness * 3).ToSVG()}; ");
                    break;

                case LineStyles.Dotted:
                    style.Append($"stroke-dasharray: {appearance.LineThickness.ToSVG()} {(appearance.LineThickness * 3).ToSVG()}; ");
                    break;
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
                    style.Append($"stroke-dasharray: {(appearance.LineThickness * 4).ToSVG()} {(appearance.LineThickness * 3).ToSVG()}; ");
                    break;

                case LineStyles.Dotted:
                    style.Append($"stroke-dasharray: {appearance.LineThickness.ToSVG()} {(appearance.LineThickness * 3).ToSVG()}; ");
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
