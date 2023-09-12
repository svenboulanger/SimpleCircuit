using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// An addition of an annotation.
    /// </summary>
    public class AnnotationPush : AnnotationChange
    {
        /// <summary>
        /// Gets the annotation info that needs to be pushed.
        /// </summary>
        public AnnotationInfo Info { get; }

        /// <summary>
        /// Creates a new <see cref="AnnotationPush"/>.
        /// </summary>
        /// <param name="info">The info.</param>
        public AnnotationPush(AnnotationInfo info)
        {
            Info = info ?? throw new ArgumentNullException(nameof(info));
        }

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
