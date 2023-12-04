using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using Svg.Skia;
using System.Text.RegularExpressions;
using System.Xml;

namespace SimpleCircuit
{
    /// <summary>
    /// A SimpleCircuit job.
    /// </summary>
    public partial class Job
    {
        private readonly JobDiagnosticLogger _logger = new();
        private GraphicalCircuit? _circuit;

        /// <summary>
        /// Gets the number of errors.
        /// </summary>
        public bool HasErrors => _logger.Messages.Any(msg => msg.Severity == SeverityLevel.Error);

        /// <summary>
        /// Gets or sets the filename containing the SimpleCircuit script.
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filename to where the result should be written.
        /// </summary>
        public string OutputFilename { get; set; } = string.Empty;

        /// <summary>
        /// Computes the graphical circuit.
        /// </summary>
        public void Compute()
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

            // Now we can start parsing the input file
            {
                var lexer = SimpleCircuitLexer.FromString(simpleCircuitScript.AsMemory(), Filename);
                var context = new ParsingContext() { Diagnostics = _logger };
                Parser.Parser.Parse(lexer, context);

                // Solve it already
                _circuit = context.Circuit;
            }
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
                    diagnostics.Post(message);
            }
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        /// <param name="diagnostics">The diagnostic message handler.</param>
        public void Render(IDiagnosticHandler diagnostics)
        {
            if (_circuit == null)
                return;

            // Determine the output file
            string outputFilename = OutputFilename;
            if (string.IsNullOrWhiteSpace(outputFilename))
                outputFilename = Path.Combine(Path.GetDirectoryName(Filename) ?? string.Empty, Path.GetFileNameWithoutExtension(Filename) + ".svg");
            else if (!Path.IsPathRooted(outputFilename))
                outputFilename = Path.Combine(Directory.GetCurrentDirectory(), outputFilename);

            // Render
            if (_circuit != null && _circuit.Count > 0)
            {
                var doc = _circuit.Render(diagnostics);

                switch (Path.GetExtension(outputFilename).ToLower())
                {
                    case ".png":
                        using (var svg = new SKSvg())
                        using (var reader = new XmlNodeReader(doc))
                        {
                            var picture = svg.Load(reader);
                            if (picture != null)
                            {
                                svg.Save(outputFilename, SkiaSharp.SKColors.Transparent, SkiaSharp.SKEncodedImageFormat.Png);
                                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "JOB01", $"Finished converting '{Filename}' to a PNG, output at '{outputFilename}'."));
                            }
                        }
                        break;

                    case ".jpg":
                    case ".jpeg":
                        using (var svg = new SKSvg())
                        using (var reader = new XmlNodeReader(doc))
                        {
                            var picture = svg.Load(reader);
                            if (picture != null)
                            {
                                svg.Save(outputFilename, SkiaSharp.SKColors.White, SkiaSharp.SKEncodedImageFormat.Jpeg);
                                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "JOB01", $"Finished converting '{Filename}' to JPG, output at '{outputFilename}'."));
                            }
                        }
                        break;

                    default:
                        // Finally write the resulting document to svg
                        using (var writer = XmlWriter.Create(outputFilename, new XmlWriterSettings()))
                        {
                            doc.WriteTo(writer);
                        }
                        diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Info, "JOB01", $"Finished converting '{Filename}' to an SVG, output at '{outputFilename}'."));
                        break;
                }
            }
            else
            {
                diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Error, "JOB02", $"No circuit elements in '{Filename}'"));
            }
        }

        [GeneratedRegex(@"^/\*\s*exclude\s+default\s*\*/$")]
        private static partial Regex ExcludeDefault();
    }
}
