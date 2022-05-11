using System.Xml;

namespace SimpleCircuit.Drawing
{
    /// <summary>
    /// Graphical options for path operations.
    /// </summary>
    public class PathOptions : GraphicOptions
    {
        /// <summary>
        /// Possible markers.
        /// </summary>
        public enum MarkerTypes
        {
            /// <summary>
            /// No markers.
            /// </summary>
            None,

            /// <summary>
            /// An arrowhead.
            /// </summary>
            Arrow,

            /// <summary>
            /// A dot.
            /// </summary>
            Dot,

            /// <summary>
            /// A slash.
            /// </summary>
            Slash,
        }

        /// <summary>
        /// Possible line types.
        /// </summary>
        public enum LineTypes
        {
            /// <summary>
            /// Just a straight line.
            /// </summary>
            None,

            /// <summary>
            /// A dashed line
            /// </summary>
            Dashed,

            /// <summary>
            /// A dotted line
            /// </summary>
            Dotted,
        }

        /// <summary>
        /// Gets or sets an end point marker.
        /// </summary>
        public MarkerTypes EndMarker { get; set; }

        /// <summary>
        /// Gets or sets a middle point marker.
        /// </summary>
        public MarkerTypes MiddleMarker { get; set; }

        /// <summary>
        /// Gets or sets a start point marker.
        /// </summary>
        public MarkerTypes StartMarker { get; set; }

        /// <summary>
        /// Gets or sets the line type.
        /// </summary>
        public LineTypes LineType { get; set; }

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
            ApplyMarkers(element);
            ApplyLineTypes(element);
        }

        private string GetDefinition(XmlDocument document, MarkerTypes marker)
        {
            if (marker == MarkerTypes.None)
                return null;
            string ns = document.DocumentElement.NamespaceURI;

            // Find the metadata tag
            var defs = document.DocumentElement.SelectSingleNode("defs");
            if (defs == null)
                defs = document.DocumentElement.PrependChild(document.CreateElement("defs", ns));

            // For each marker, provide a definition
            XmlElement elt;
            switch (marker)
            {
                case MarkerTypes.Arrow:
                    elt = (XmlElement)defs.SelectSingleNode("marker[@id='arrow']");
                    if (elt == null)
                    {
                        elt = document.CreateElement("marker", ns);
                        elt.SetAttribute("id", "arrow");
                        elt.SetAttribute("refX", "0");
                        elt.SetAttribute("refY", "0");
                        elt.SetAttribute("orient", "auto");
                        elt.SetAttribute("markerUnits", "userSpaceOnUse");
                        elt.SetAttribute("style", "overflow:visible");
                        var pl = document.CreateElement("polygon", ns);
                        pl.SetAttribute("class", "arrowhead");
                        pl.SetAttribute("points", "-2.5,1 0,0 -2.5,-1 -2,0");
                        elt.AppendChild(pl);
                        defs.AppendChild(elt);
                    }
                    return "url(#arrow)";

                case MarkerTypes.Dot:
                    elt = (XmlElement)defs.SelectSingleNode("marker[@id='dot']");
                    if (elt == null)
                    {
                        elt = document.CreateElement("marker", ns);
                        elt.SetAttribute("id", "dot");
                        elt.SetAttribute("refX", "0");
                        elt.SetAttribute("refY", "0");
                        elt.SetAttribute("orient", "auto");
                        elt.SetAttribute("markerUnits", "userSpaceOnUse");
                        elt.SetAttribute("style", "overflow:visible");
                        var c = document.CreateElement("circle", ns);
                        c.SetAttribute("cx", "0");
                        c.SetAttribute("cy", "0");
                        c.SetAttribute("r", "1");
                        c.SetAttribute("class", "dot");
                        elt.AppendChild(c);
                        defs.AppendChild(elt);
                    }
                    return "url(#dot)";

                case MarkerTypes.Slash:
                    elt = (XmlElement)defs.SelectSingleNode("marker[@id='slash']");
                    if (elt == null)
                    {
                        elt = document.CreateElement("marker", ns);
                        elt.SetAttribute("id", "slash");
                        elt.SetAttribute("refX", "0");
                        elt.SetAttribute("refY", "0");
                        elt.SetAttribute("orient", "auto");
                        elt.SetAttribute("markerUnits", "userSpaceOnUse");
                        elt.SetAttribute("style", "overflow:visible");
                        var pl = document.CreateElement("line", ns);
                        pl.SetAttribute("x1", "-1");
                        pl.SetAttribute("y1", "2");
                        pl.SetAttribute("x2", "1");
                        pl.SetAttribute("y2", "-2");
                        elt.AppendChild(pl);
                        defs.AppendChild(elt);
                    }
                    return "url(#slash)";
            }
            return null;
        }

        private void ApplyMarkers(XmlElement element)
        {
            var doc = element.OwnerDocument;

            // Deal with the definitions for the markers
            string url = GetDefinition(doc, EndMarker);
            if (url != null)
                element.SetAttribute("marker-end", url);

            url = GetDefinition(doc, MiddleMarker);
            if (url != null)
                element.SetAttribute("marker-mid", url);

            url = GetDefinition(doc, StartMarker);
            if (url != null)
                element.SetAttribute("marker-start", url);
        }

        private void ApplyLineTypes(XmlElement element)
        {
            // Simply give the line type a class that will modify the line type
            switch (LineType)
            {
                case LineTypes.None:
                    return;

                case LineTypes.Dashed:
                    element.SetAttribute("stroke-dasharray", "2 2");
                    return;

                case LineTypes.Dotted:
                    element.SetAttribute("stroke-dasharray", "0.5 2");
                    return;
            }
        }
    }
}
