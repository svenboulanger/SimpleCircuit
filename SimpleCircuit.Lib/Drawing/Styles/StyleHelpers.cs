using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using System.Text;

namespace SimpleCircuit.Drawing.Styles;

/// <summary>
/// Helper methods for styles.
/// </summary>
public static class StyleHelpers
{
    /// <summary>
    /// Appends a style to an <see cref="IStyled"/>.
    /// </summary>
    /// <param name="styled">The styled item.</param>
    /// <param name="modifier">The style modifier.</param>
    public static void AppendStyle(this IStyled styled, IStyleModifier modifier)
    {
        if (styled.Modifier is not null)
            styled.Modifier = new AggregateStyleModifier(styled.Modifier, modifier);
        else
            styled.Modifier = modifier;
    }

    /// <summary>
    /// Appends a style to a <see cref="IStyleModifier"/>.
    /// </summary>
    /// <param name="modifier">The style modifier.</param>
    /// <param name="next">The next style modifier.</param>
    /// <returns>The combined style modifier.</returns>
    public static IStyleModifier Append(this IStyleModifier modifier, IStyleModifier next)
    {
        if (modifier is not null)
            return new AggregateStyleModifier(modifier, next);
        return next;
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
    /// Gets a style that is modified by the given <see cref="IDrawable"/> style.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="drawable">The drawable.</param>
    /// <returns>Returns the style.</returns>
    public static IStyle ModifyDashedDotted(this IStyle style, IDrawable drawable)
    {
        var result = drawable.Modifier?.Apply(style) ?? style;
        switch (drawable.Variants.Select("dashed", "dotted"))
        {
            case 0: result = new DashedStrokeStyleModifier.Style(result); break;
            case 1: result = new DottedStrokeStyleModifier.Style(result); break;
        }
        return result;
    }

    /// <summary>
    /// Gets a style that will override a style to have center justification.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <returns>Returns the style.</returns>
    public static IStyle JustifyCenter(this IStyle style) => new JustificationStyleModifier.Style(style, 0.0);

    /// <summary>
    /// Gets a style modifier that will override a style to have center justification.
    /// </summary>
    /// <param name="modifier">The style modifier.</param>
    /// <returns>Returns the style modifier.</returns>
    public static IStyleModifier JustifyCenter(this IStyleModifier modifier)
    {
        if (modifier is null)
            return new JustificationStyleModifier(0.0);
        return new AggregateStyleModifier(modifier, new JustificationStyleModifier(0.0));
    }

    /// <summary>
    /// Gets a style that will override a style to have foreground and/or background color.
    /// </summary>
    /// <param name="style">The parent style.</param>
    /// <param name="color">The foreround color (or <c>null</c> if the parent color can be used).</param>
    /// <param name="background">The background color (or <c>null</c> if the parent backround can be used)</param>
    /// <returns>Returns the style</returns>
    public static IStyle Color(this IStyle style, string color, string background)
        => new ColorStyleModifier.Style(style, color, background);

    /// <summary>
    /// Gets a style modifier that will override a style to have foreground and/or background color.
    /// </summary>
    /// <param name="modifier">the modifier</param>
    /// <param name="color">The foreground color (or <c>null</c> if the parent color can be used).</param>
    /// <param name="background">The background color (or <c>null</c> if the parent background can be used).</param>
    /// <returns>Returns the style modifier.</returns>
    public static IStyleModifier Color(this IStyleModifier modifier, string color, string background)
    {
        if (modifier is null)
            return new ColorStyleModifier(color, background);
        return new AggregateStyleModifier(modifier, new ColorStyleModifier(color, background));
    }

    /// <summary>
    /// Resolves a color from a style, also using the variables.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <returns>The color.</returns>
    private static string GetColor(IStyle style, string color, IDiagnosticHandler diagnostics)
    {
        if (color[0] == '-' && color[1] == '-')
        {
            // This is likely a variable, try to find the key
            if (style.TryGetVariable(color.Substring(2), out string result))
                return result;
            else
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "WARNING", $"Could not find color variable '{color}'."));
        }
        return color;
    }

    /// <summary>
    /// Resolves the foreground color from a style.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <returns>Returns the color.</returns>
    private static string GetForeground(this IStyle style, IDiagnosticHandler diagnostics) => GetColor(style, style.Color, diagnostics);

    /// <summary>
    /// Resolves the background color from a style.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <returns>Returns the background color.</returns>
    private static string GetBackground(this IStyle style, IDiagnosticHandler diagnostics) => GetColor(style, style.Background, diagnostics);

    /// <summary>
    /// Creates a style attribute value for strokes and no fill that represents the <see cref="IStyle"/>.
    /// Lines do not need to have any background color.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <returns>Returns the style attribute value.</returns>
    public static string CreateStrokeStyle(this IStyle style, IDiagnosticHandler diagnostics)
    {
        var result = new StringBuilder();

        // Deal with the foreground
        if (style.Opacity.IsZero())
            result.Append("stroke: none; ");
        else if ((style.Opacity - Style.Opaque).IsZero() || style.Opacity > Style.Opaque)
            result.Append($"stroke: {style.GetForeground(diagnostics)}; ");
        else
            result.Append($"stroke: {style.GetForeground(diagnostics)}; stroke-opacity: {style.Opacity.ToSVG()}; ");

        // Allow other path options
        if (style.StrokeDashArray is not null && !string.IsNullOrWhiteSpace(style.StrokeDashArray))
            result.Append($"stroke-dasharray: {style.StrokeDashArray}; ");

        // Path options
        result.Append($"stroke-width: {style.LineThickness.ToSVG()}pt; ");
        result.Append("stroke-linecap: round; ");
        result.Append("stroke-linejoin: round; ");
        result.Append("fill: none;");

        return result.ToString();
    }

    /// <summary>
    /// Creates a style attribute value for strokes with fill that represents the <see cref="IStyle"/>.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <returns>Returns the style attribute value.</returns>
    public static string CreateStrokeFillStyle(this IStyle style, IDiagnosticHandler diagnostics)
    {
        var result = new StringBuilder();

        // Deal with the foreground
        if (style.Opacity.IsZero())
            result.Append("stroke: none; ");
        else if ((style.Opacity - Style.Opaque).IsZero() || style.Opacity > Style.Opaque)
            result.Append($"stroke: {style.GetForeground(diagnostics)}; ");
        else
            result.Append($"stroke: {style.GetForeground(diagnostics)}; stroke-opacity: {style.Opacity.ToSVG()}; ");

        // Path options
        result.Append($"stroke-width: {style.LineThickness.ToSVG()}pt; ");
        result.Append("stroke-linecap: round; ");
        result.Append("stroke-linejoin: round; ");

        // Stroke dasharray
        if (style.StrokeDashArray is not null && !string.IsNullOrWhiteSpace(style.StrokeDashArray))
            result.Append($"stroke-dasharray: {style.StrokeDashArray}; ");

        // Deal with the background
        if (style.BackgroundOpacity.IsZero())
            result.Append("fill: none;");
        else if ((style.BackgroundOpacity - Style.Opaque).IsZero() || style.BackgroundOpacity > Style.Opaque)
            result.Append($"fill: {style.GetBackground(diagnostics)};");
        else
            result.Append($"fill: {style.GetBackground(diagnostics)}; fill-opacity: {style.BackgroundOpacity.ToSVG()};");
        return result.ToString();
    }

    /// <summary>
    /// Creats a style attribute for text that represents the <see cref="IStyle"/>.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <returns>Returns the style attribute value.</returns>
    public static string CreateTextStyle(this IStyle style, IDiagnosticHandler diagnostics)
    {
        var sb = new StringBuilder();
        sb.Append($"font-family: {style.FontFamily}; ");
        sb.Append($"font-size: {style.FontSize.ToSVG()}pt; ");
        if (style.Opacity.IsZero())
            sb.Append($"fill: none; ");
        else if ((style.Opacity - Style.Opaque).IsZero() || style.Opacity > Style.Opaque)
            sb.Append($"fill: {style.GetForeground(diagnostics)}; ");
        else
            sb.Append($"fill: {style.GetForeground(diagnostics)}; fill-opacity: {style.Opacity}; ");
        if (style.Bold)
            sb.Append("font-weight: bold; ");
        sb.Append("stroke: none;");
        return sb.ToString();
    }
}
