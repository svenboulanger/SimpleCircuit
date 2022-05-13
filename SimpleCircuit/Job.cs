using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuit
{
    /// <summary>
    /// A SimpleCircuit job.
    /// </summary>
    public class Job
    {
        private string _cssScript = null;
        private readonly JobDiagnosticLogger _logger = new JobDiagnosticLogger();
        private GraphicalCircuit _circuit;

        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        public bool HasErrors => _logger.Messages.Any(msg => msg.Severity == SeverityLevel.Error);

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
        /// Computes the graphical circuit.
        /// </summary>
        public async Task Compute()
        {
            // Load the input file
            if (string.IsNullOrWhiteSpace(Filename))
            {
                _logger?.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", "No filename specified"));
                return;
            }
            if (!File.Exists(Filename))
            {
                _logger?.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", $"Could not find file"));
                return;
            }
            string simpleCircuitScript = File.ReadAllText(Filename);

            // Load the CSS file if any
            _cssScript = null;
            if (!string.IsNullOrWhiteSpace(CssFilename))
            {
                if (!File.Exists(CssFilename))
                {
                    _logger?.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", $"Could not find file '{CssFilename}'"));
                    return;
                }
                _cssScript = File.ReadAllText(CssFilename);
            }
            else
            {
                // Include a CSS file if there is one with the same name as the input filename
                string tryCssFilename = Path.GetFileNameWithoutExtension(Filename) + ".css";
                if (File.Exists(tryCssFilename))
                    _cssScript = File.ReadAllText(tryCssFilename);
            }
            if (_cssScript != null)
            {
                // Add the default script unless there is a comment of the form "/* exclude default */"
                if (!Regex.IsMatch(_cssScript, @"^/\*\s*exclude\s+default\s*\*/$"))
                {
                    _cssScript = GraphicalCircuit.DefaultStyle + Environment.NewLine + _cssScript;
                }
            }

            // Now we can start parsing the input file
            _circuit = await Task.Run(() =>
            {
                var lexer = SimpleCircuitLexer.FromString(simpleCircuitScript);
                var context = new ParsingContext() { Diagnostics = _logger };
                Parser.Parser.Parse(lexer, context);

                // Solve it already
                // context.Circuit.Solve(_logger);
                return context.Circuit;
            });
        }

        /// <summary>
        /// Displays the diagnostic mesasges for this job.
        /// </summary>
        /// <param name="diagnostics">The diagnostic message handler.</param>
        public void DisplayMessages(IDiagnosticHandler diagnostics)
        {
            // First pass all the local messages
            if (diagnostics != null)
            {
                foreach (var message in _logger.Messages)
                    diagnostics.Post(new DiagnosticMessage(message.Severity, message.Code, $"{message.Message} for {Filename}"));
            }
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <param name="textFormatter">The test formatter.</param>
        /// <param name="diagnostics">The diagnostic message handler.</param>
        public async Task Render(ChromiumElementFormatter textFormatter, IDiagnosticHandler diagnostics)
        {
            // Determine the output file
            string outputFilename = OutputFilename;
            if (string.IsNullOrWhiteSpace(outputFilename))
                outputFilename = Path.GetFileNameWithoutExtension(Filename) + ".svg";

            // Render
            if (_circuit != null && _circuit.Count > 0)
            {
                await textFormatter.UpdateStyle(_cssScript ?? GraphicalCircuit.DefaultStyle);
                var doc = _circuit.Render(diagnostics, textFormatter);

                // Finally write the resulting document to svg
                using (var writer = XmlWriter.Create(outputFilename, new XmlWriterSettings()))
                {
                    doc.WriteTo(writer);
                }
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "JOB01", $"Finished converting '{Filename}', output at '{outputFilename}'."));
            }
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "JOB02", $"No circuit elements in '{Filename}'"));
            }
        }
    }
}
