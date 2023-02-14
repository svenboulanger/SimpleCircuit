using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Graphical options for path operations.
    /// </summary>
    public class PathOptions : GraphicOptions
    {
        /// <summary>
        /// Creates new path options.
        /// </summary>
        /// <param name="classNames">Class names.</param>
        public PathOptions(params string[] classNames)
            : base(classNames)
        {
        }

        /// <inheritdoc />
        public override void Apply(XmlElement element)
        {
            if (element == null)
                return;
            base.Apply(element);
        }
    }
}
