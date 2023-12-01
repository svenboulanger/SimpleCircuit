using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A removal of an annotation.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="AnnotationPop"/>.
    /// </remarks>
    /// <param name="source">The token source.</param>
    public class AnnotationPop(Token source) : AnnotationChange
    {
        /// <summary>
        /// Gets the source of the change.
        /// </summary>
        public Token Source { get; } = source;

        /// <inheritdoc />
        public override bool Apply(ParsingContext context)
        {
            if (context.PopAnnotation() == null)
            {
                context.Diagnostics?.Post(Source, ErrorCodes.AnnotationMismatch);
                return false;
            }
            return true;
        }
    }

}
