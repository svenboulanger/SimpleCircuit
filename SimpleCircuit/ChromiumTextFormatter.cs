using System;
using System.Xml;
using SimpleCircuit.Drawing;
using CefSharp;
using CefSharp.OffScreen;
using System.IO;

namespace SimpleCircuit
{
    /// <summary>
    /// A text formatter based on a Chromium browser.
    /// </summary>
    public class ChromiumTextFormatter : IElementFormatter
    {
        private readonly ChromiumWebBrowser _browser;

        /// <summary>
        /// Creates a new <see cref="ChromiumTextFormatter"/>.
        /// </summary>
        /// <param name="browser">The browser that will be used to format the text.</param>
        public ChromiumTextFormatter(ChromiumWebBrowser browser)
        {
            _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        public Bounds Format(SvgDrawing drawing, XmlElement element)
        {
            string text = null;
            using (var sw = new StringWriter())
            {
                using (var w = XmlWriter.Create(sw, new XmlWriterSettings() { OmitXmlDeclaration = true }))
                {
                    element.WriteTo(w);
                };
                text = sw.ToString();
            }

            // Make a piece of XML that allows measuring this element
            if (element.Name != "svg")
            {
                XmlNode elt = element.ParentNode;
                while (elt != null)
                {
                    text = Enclose(elt, text);
                    elt = elt.ParentNode;
                    if (elt.Name == "svg")
                        break;
                }
                text = $"<svg class=\"simplecircuit\" xmlns=\"http://www.w3.org/2000/svg\">{text}</svg>";
            }

            // Get the result from the browser
            var task = _browser.EvaluateScriptAsync("calculateBounds", text);
            task.Wait();
            dynamic result = task.Result.Result;
            return new Bounds(result.x, result.y, result.x + result.width, result.y + result.height);
        }

        private string Enclose(XmlNode enclose, string xml)
        {
            using (var sw = new StringWriter())
            {
                sw.Write('<');
                sw.Write(enclose.Name);
                if (enclose.Attributes != null)
                {
                    for (int i = 0; i < enclose.Attributes.Count; i++)
                    {
                        var attribute = enclose.Attributes[i];
                        sw.Write(' ');
                        sw.Write(attribute.Name);
                        sw.Write("=\"");
                        sw.Write(attribute.Value);
                        sw.Write('"');
                    }
                }
                sw.Write('>');
                sw.Write(xml);
                sw.Write("</");
                sw.Write(enclose.Name);
                sw.Write(">");
                return sw.ToString();
            }
        }
    }
}
