﻿using SimpleCircuit.Components.Builders;

namespace SimpleCircuit.Components.Builders.Markers
{
    /// <summary>
    /// A reversed arrow marker.
    /// </summary>
    public class ReverseArrow : Marker
    {
        private readonly static Vector2[] _points = [new(0, -1), new(-2.5, 0), new(0, 1)];

        /// <summary>
        /// Gets the default arrow options.
        /// </summary>
        public static GraphicOptions DefaultOptions { get; } = new("marker", "arrow", "reverse");

        /// <summary>
        /// Creates a new arrow marker.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="options">The options.</param>
        public ReverseArrow(Vector2 location = new(), Vector2 orientation = new(), GraphicOptions options = null)
            : base(location, orientation, options)
        {
            Options ??= DefaultOptions;
        }

        /// <inheritdoc />
        protected override void DrawMarker(IGraphicsBuilder builder)
            => builder.Polygon(_points, Options);
    }
}
