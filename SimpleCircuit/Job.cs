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
        public async Task Execute(ChromiumTextFormatter textFormatter, IDiagnosticHandler diagnostics)
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

            // Now we can start parsing the input file
            var lexer = SimpleCircuitLexer.FromString(simpleCircuitScript);
            var context = new ParsingContext() { Diagnostics = diagnostics };
            Parser.Parser.Parse(lexer, context);

            var ckt = context.Circuit;
            if (ckt.Count > 0)
            {
                await textFormatter.UpdateStyle(cssScript);
                var doc = ckt.Render(diagnostics, textFormatter);

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
