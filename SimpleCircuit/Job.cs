using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using System.Text.RegularExpressions;
using System.Xml;

namespace SimpleCircuit;

/// <summary>
/// A SimpleCircuit job.
/// </summary>
public partial class Job
{
    private readonly JobDiagnosticLogger _logger = new();
    private EvaluationContext? _context = null;

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
    /// Gets or sets the text formatter.
    /// </summary>
    public ITextFormatter? TextFormatter { get; set; }

    /// <summary>
    /// Gets or sets whether the font face should be embedded directly into the generated SVG.
    /// This produces a fully self-contained SVG at the cost of a much larger file size.
    /// </summary>
    public bool EmbedFonts { get; set; }

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
            var lexer = SimpleCircuitLexer.FromString(simpleCircuitScript, Filename);
            
            // Parse
            var parsingContext = new ParsingContext() { Diagnostics = _logger };
            if (!SimpleCircuitParser.Parse(lexer, parsingContext, out var statements))
                return;

            // Evaluate
            _context = new EvaluationContext(true, new Style(), TextFormatter);
            StatementEvaluator.Evaluate(statements, _context);
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
        if (_context?.Circuit == null)
            return;

        // Determine the output file
        string outputFilename = OutputFilename;
        if (string.IsNullOrWhiteSpace(outputFilename))
            outputFilename = Path.Combine(Path.GetDirectoryName(Filename) ?? string.Empty, Path.GetFileNameWithoutExtension(Filename) + ".svg");
        else if (!Path.IsPathRooted(outputFilename))
            outputFilename = Path.Combine(Directory.GetCurrentDirectory(), outputFilename);

        // Render
        if (_context?.Circuit != null && _context.Circuit.Count > 0)
        {
            var doc = _context.Circuit.Render(diagnostics, EmbedFonts);
            if (doc is null)
                return;

            // Raster output (PNG/JPG) is no longer supported now that the SVG rasterizer
            // (SkiaSharp/Svg.Skia) has been removed. Warn and emit an SVG instead.
            switch (Path.GetExtension(outputFilename).ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                    diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "JOB03", $"Raster output is no longer supported; writing an SVG instead of '{Path.GetFileName(outputFilename)}'."));
                    outputFilename = Path.ChangeExtension(outputFilename, ".svg");
                    goto default;

                default:
                    // Write the resulting document to svg
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
