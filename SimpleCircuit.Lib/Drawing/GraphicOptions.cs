using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Graphical options.
    /// </summary>
    public class GraphicOptions
    {
        /// <summary>
        /// Gets a set of classes used for the graphical component.
        /// </summary>
        public HashSet<string> Classes { get; } = new();

        /// <summary>
        /// Gets or sets the identifier of the element.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Creates new graphic options.
        /// </summary>
        /// <param name="classNames">The class names.</param>
        public GraphicOptions(params string[] classNames)
        {
            foreach (var n in classNames)
                Classes.Add(n);
        }

        /// <summary>
        /// Applies the graphic options to an XML element.
        /// </summary>
        /// <param name="element">The element.</param>
        public virtual void Apply(XmlElement element)
        {
            if (element == null)
                return;
            if (Classes.Count > 0)
                element.SetAttribute("class", string.Join(" ", Classes));
            if (!string.IsNullOrWhiteSpace(Id))
                element.SetAttribute("id", Id);
        }
    }
}
