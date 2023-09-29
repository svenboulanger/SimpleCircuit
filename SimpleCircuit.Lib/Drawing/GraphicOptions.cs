using System;
using System.Collections.Generic;
using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Graphical options.
    /// </summary>
    public class GraphicOptions : IEquatable<GraphicOptions>
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
        /// Gets or sets the style of the path.
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// Creates new graphic options.
        /// </summary>
        /// <param name="classNames">The class names.</param>
        public GraphicOptions(params string[] classNames)
        {
            foreach (var n in classNames)
                Classes.Add(n);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = Id?.GetHashCode() ?? 0;
            hash = (hash * 1021) ^ (Style?.GetHashCode() ?? 0);
            foreach (string c in Classes)
                hash = (hash * 1021) ^ hash;
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is GraphicOptions go && Equals(go);

        /// <inheritdoc />
        public bool Equals(GraphicOptions other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (Id != other.Id)
                return false;
            if (Style != other.Style)
                return false;
            if (!Classes.SetEquals(other.Classes))
                return false;
            return true;
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
            if (!string.IsNullOrWhiteSpace(Style))
                element.SetAttribute("style", Style);
        }

        /// <summary>
        /// Overload of equality of graphic options.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        ///     Returns <c>true</c> if both are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(GraphicOptions left, GraphicOptions right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Overload of inequality of graphic options.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        ///     Returns <c>true</c> if both are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(GraphicOptions left, GraphicOptions right)
            => !(left == right);
    }
}
