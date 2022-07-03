using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Represents a drawable that can be oriented and has a 2D location.
    /// </summary>
    public interface IOrientedDrawable : ILocatedDrawable, ITransformingDrawable
    {
        /// <summary>
        /// Determines whether the relative orientation given is already constrained.
        /// </summary>
        /// <param name="p">The local vector orientation.</param>
        /// <returns>Returns <c>true</c> if the orientation is constrained; otherwise, <c>false</c>.</returns>
        public bool IsConstrained(Vector2 p);

        /// <summary>
        /// Constrains the component using a pin orientation and a result. All vectors are assumed unit length.
        /// For two pins (p1, p2) that need to be constrained to two orientations (b1, b2), we can write that:
        /// A * p1 = b1 and A * p2 = b2 or A * (p1, p2) = (b1, b2).
        /// then the matrix A can be solved by:
        /// A = (b1, b2) * (p1, p2)^(-1)
        /// The result is only accepted if the orientation is represented by an orthonormal matrix.
        /// </summary>
        /// <param name="p">The local vector orientation.</param>
        /// <param name="b">The constraining result.</param>
        /// <param name="diagnostics">The diagnostic handler.</param>
        /// <returns>Returns <c>true</c> if the orientation can be constrained; otherwise, <c>false</c>.</returns>
        public bool ConstrainOrientation(Vector2 p, Vector2 b, IDiagnosticHandler diagnostics);
    }
}
