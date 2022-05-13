using System;
using System.Xml;
using SimpleCircuit.Drawing;
using CefSharp;
using CefSharp.OffScreen;
using System.IO;
using System.Threading.Tasks;

namespace SimpleCircuit
{
    /// <summary>
    /// A text formatter based on a Chromium browser.
    /// </summary>
    public class ChromiumElementFormatter : IElementFormatter, IDisposable
    {
        static ChromiumElementFormatter()
        {
            Cef.EnableWaitForBrowsersToClose();
            var settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                IgnoreCertificateErrors = true,
                CookieableSchemesExcludeDefaults = true,
            };
            CefSharpSettings.ShutdownOnExit = true;
            settings.CefCommandLineArgs.Add("no-proxy-server", "1");
            settings.CefCommandLineArgs.Add("disable-extensions", "1");
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        private const string _browserHtml = @"
<html>
    <head>
        <style id=""svg-style""></style>
    </head>
    <body>
        <div id=""div_measure"">
        </div>
        <script>
            function calculateBounds(element) {
                var div_measure = document.getElementById('div_measure');

                // We simply parse the XML and return the bounds
                var parser = new DOMParser();
                var e = parser.parseFromString(element, ""image/svg+xml"").documentElement;
                div_measure.appendChild(e);
                var b = e.getBBox();
                div_measure.removeChild(e);
                return {
                    x: b.x,
                    y: b.y,
                    width: b.width,
                    height: b.height
                };
            }

            function updateStyle(style)
            {
                document.getElementById(""svg-style"").innerHTML = style;
            }
        </script>
    </body>
</html>";
        private readonly ChromiumWebBrowser _browser;
        private readonly RequestContext _requestContext;

        /// <summary>
        /// Creates a new <see cref="ChromiumElementFormatter"/>.
        /// </summary>
        /// <param name="browser">The browser that will be used to format the text.</param>
        public ChromiumElementFormatter()
        {
            // Start a browser that will be measuring all our text
            var browserSettings = new BrowserSettings()
            {
                WindowlessFrameRate = 1,
                Javascript = CefState.Enabled,
            };
            var requestContextSettings = new RequestContextSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            };
            _requestContext = new RequestContext(requestContextSettings);
            _browser = new ChromiumWebBrowser("http://simplecircuit/", browserSettings, _requestContext, true);
            _browser.LoadHtml(_browserHtml, "http://simplecircuit/");

            var task = _browser.WaitForInitialLoadAsync();
            task.Wait();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _browser.Dispose();
            _requestContext.Dispose();
        }

        /// <summary>
        /// Updates the style.
        /// </summary>
        /// <param name="style">The style.</param>
        public async Task UpdateStyle(string style)
        {
            await _browser.EvaluateScriptAsync("updateStyle", style);
        }

        /// <inheritdoc />
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

        private static string Enclose(XmlNode enclose, string xml)
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
