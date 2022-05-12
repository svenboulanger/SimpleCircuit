using CefSharp;
using CefSharp.OffScreen;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuit
{
    /// <summary>
    /// A SimpleCircuit job.
    /// </summary>
    public class Job
    {
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

        /// <summary>
        /// Gets or sets the filename containing the SimpleCircuit script.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the filename containing the CSS code.
        /// </summary>
        public string CssFilename { get; set; }

        /// <summary>
        /// Gets or sets the filename to where the result should be written.
        /// </summary>
        public string OutputFilename { get; set; }

        /// <summary>
        /// Executes the job.
        /// </summary>
        public async Task Execute(IDiagnosticHandler diagnostics)
        {
            // Load the input file
            if (string.IsNullOrWhiteSpace(Filename))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", "No filename specified"));
                return;
            }
            if (!File.Exists(Filename))
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", $"Could not find file '{Filename}'"));
                return;
            }
            string simpleCircuitScript = File.ReadAllText(Filename);

            // Load the CSS file if any
            string cssScript = GraphicalCircuit.DefaultStyle;
            if (!string.IsNullOrWhiteSpace(CssFilename))
            {
                if (!File.Exists(CssFilename))
                {
                    diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", $"Could not find file '{CssFilename}'"));
                    return;
                }
                cssScript = File.ReadAllText(CssFilename);
            }
            else
            {
                // Include a CSS file if there is one with the same name as the input filename
                string tryCssFilename = Path.GetFileNameWithoutExtension(Filename) + ".css";
                if (File.Exists(tryCssFilename))
                    cssScript = File.ReadAllText(tryCssFilename);
            }

            // Determine the output file
            string outputFilename = OutputFilename;
            if (string.IsNullOrWhiteSpace(outputFilename))
                outputFilename = Path.GetFileNameWithoutExtension(Filename) + ".svg";

            // Start a browser that will be measuring all our text
            var browserSettings = new BrowserSettings()
            {
                WindowlessFrameRate = 1,
                Javascript = CefState.Enabled
            };
            var requestContextSettings = new RequestContextSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            };
            using (var requestContext = new RequestContext(requestContextSettings))
            using (var browser = new ChromiumWebBrowser("http://simplecircuit/", browserSettings, requestContext, true))
            {
                browser.LoadHtml(_browserHtml, "http://simplecircuit/");

                // Wait for the browser to finish loading
                await browser.WaitForInitialLoadAsync();

                // Make sure the script knows what style we are using here!
                await browser.EvaluateScriptAsync("updateStyle", cssScript);

                // Now we can start parsing the input file
                var lexer = SimpleCircuitLexer.FromString(simpleCircuitScript);
                var context = new ParsingContext() { Diagnostics = diagnostics };
                Parser.Parser.Parse(lexer, context);

                var ckt = context.Circuit;
                if (ckt.Count > 0)
                {
                    var doc = ckt.Render(diagnostics, new ChromiumTextFormatter(browser));

                    // Finally write the resulting document to svg
                    using (var writer = XmlWriter.Create(outputFilename, new XmlWriterSettings()))
                    {
                        doc.WriteTo(writer);
                    }
                    diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "JOB01", $"Finished converting '{Filename}', output at '{outputFilename}'."));
                }
            }
        }
    }
}
