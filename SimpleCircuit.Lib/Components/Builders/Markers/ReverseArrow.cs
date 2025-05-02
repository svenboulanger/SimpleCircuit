using System.Linq;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// A reversed arrow marker.
    /// </summary>
    /// <remarks>
    /// Creates a new arrow marker.
    /// </remarks>
    /// <param name="location">The location.</param>
    /// <param name="orientation">The orientation.</param>
    public class ReverseArrow(Vector2 location = new(), Vector2 orientation = new()) : Marker(location, orientation)
    {
        private readonly static Vector2[] _points = [new(0, -1), new(-2.5, 0), new(0, 1)];

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
        {
            var options = new GraphicOptions("marker", "arrow", "reverse");
            options.Style["stroke"] = Foreground;
            options.Style["fill"] = Foreground;
            options.Style["stroke-width"] = $"{Thickness.ToSVG()}pt";
            options.Style["stroke-linejoin"] = "round";
            options.Style["stroke-linecap"] = "round";
            builder.Polygon(_points.Select(pt => pt * 2.0 * Thickness), options);
        }
    }
}
