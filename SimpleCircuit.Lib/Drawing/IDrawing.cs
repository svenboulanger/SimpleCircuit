using SimpleCircuit.Diagnostics;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Describes an instance that is able to draw.
    /// </summary>
    public interface IDrawing
    {
        /// <summary>
        /// Draws the current XML description of some elements.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public void Draw(XmlDocument description, IDiagnosticHandler diagnostics);

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The starting point.</param>
        /// <param name="end">The ending point.</param>
        /// <param name="options">The options.</param>
        public void Line(Vector2 start, Vector2 end, GraphicOptions options);
    }
}
