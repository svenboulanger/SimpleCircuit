using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// A list of annotation changes.
    /// </summary>
    public class AnnotationChanges : List<AnnotationChange>
    {
        /// <summary>
        /// Applies all the changes to the parsing context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns <c>true</c> if all changes were applied; otherwise, <c>false</c>.</returns>
        public bool Apply(ParsingContext context)
        {
            foreach (var change in this)
            {
                if (!change.Apply(context))
                    return false;
            }
            return true;
        }
    }
}
