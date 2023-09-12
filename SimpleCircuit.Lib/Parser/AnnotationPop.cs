using SimpleCircuit.Diagnostics;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A removal of an annotation.
    /// </summary>
    public class AnnotationPop : AnnotationChange
    {
        /// <summary>
        /// Gets the source of the change.
        /// </summary>
        public Token Source { get; }

        /// <summary>
        /// Creates a new <see cref="AnnotationPop"/>.
        /// </summary>
        /// <param name="source">The token source.</param>
        public AnnotationPop(Token source)
        {
            Source = source;
        }

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
