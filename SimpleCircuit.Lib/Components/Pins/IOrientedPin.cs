using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components.Pins
{
    /// <summary>
    /// Describes a pin with an orientation.
    /// </summary>
    public interface IOrientedPin : IPin
    {
        /// <summary>
        /// Gets the local orientation of the pin. This is the orientation
        /// when the owner is in its default orientation.
        /// </summary>
        public Vector2 LocalOrientation { get; }

        /// <summary>
        /// Gets the current orientation of the pin.
        /// </summary>
        public Vector2 Orientation { get; }

        /// <summary>
        /// Gets the owner of the pin.
        /// </summary>
        public new IOrientedDrawable Owner { get; }

        /// <summary>
        /// Resolves the orientation of a pin. It fails if the orientation cannot be resolved
        /// correctly, e.g. if contradicting information is used as the input.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <returns>Returns <c>true</c> if the orientation could be applied; otherwise, <c>false</c>.</returns>
        public bool ResolveOrientation(Vector2 orientation, IDiagnosticHandler diagnostics);
    }
}
