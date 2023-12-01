using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// An addition of an annotation.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="AnnotationPush"/>.
    /// </remarks>
    /// <param name="info">The info.</param>
    public class AnnotationPush(AnnotationInfo info) : AnnotationChange
    {
        /// <summary>
        /// Gets the annotation info that needs to be pushed.
        /// </summary>
        public AnnotationInfo Info { get; } = info ?? throw new ArgumentNullException(nameof(info));

        /// <inheritdoc />
        public override bool Apply(ParsingContext context)
        {
            var annotation = Info.GetOrCreate(context);
            if (annotation == null)
                return false;
            context.PushAnnotation(annotation);
            return true;
        }
    }

}
