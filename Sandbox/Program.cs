using SimpleCircuit;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox;

public class Program
{
    static void Main()
    {
        // ExportDemos("all");
        ExportThemes("""
        .variant ENT* even-fontsize=4.0 odd-fontsize=4.0 odd-bg="#cccccc"

        ENT1("Marks", "MarkId &#128273;", "studentid\nwith\nsome\nlonger\ntext", "SubjectId", "Date", "Mark")
        ENT2("Subjects", "SubjectId &#128273;", "Title")
        ENT3("Students", "StudentId &#128273;", "FirstName", "LastName", "GroupId")
        ENT4("SUBJ\_TEACH", "STId &#128273;", "SubjectId", "TeacherId", "GroupId")
        ENT5("Teachers", "FirstName", "LastName")
        ENT6("Groups", "GroupId &#128273;", "Name")

        ENT1 <erd-one-many r u erd-one-many> ENT2
        ENT1 <erd-one-many l d r erd-only-one> ENT3

        ENT3 <erd-one-many l d r erd-only-one> ENT6

        ENT1 <erd-one-many r d r erd-one-many> ENT4
        ENT4 <erd-one-many r erd-only-one> ENT5

        ENT6 <erd-only-one r u r erd-one-many> ENT4

        (x ENT1 <r ++5> ENT2)
        (y ENT1 <d ++5> ENT3 <d ++5> ENT6)
        """);
        // ExportThemes(@"Xtl <r +10> Xtr
        //     Xbl <r +2> Xp2 <r +6> Xbr
        //     Xtl <d +11> Xbl
        //     Xtr <d +1> Xp1 <d +5> Xbr
        //     ");
    }

    private static void Export(string filename, GraphicalCircuit circuit, IDiagnosticHandler diagnostics, string bgColor = null, bool view = false)
    {
        var doc = circuit.Render(diagnostics);
        if (doc == null)
            return;
        using var sw = new StringWriter();
        using var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true });
        doc.WriteTo(xml);
        xml.Close();

        if (File.Exists(filename))
            File.Delete(filename);
        using var fw = new StreamWriter(File.OpenWrite(filename));
        fw.WriteLine("<html>");
        fw.WriteLine("<head>");
        fw.WriteLine("</head>");
        if (!string.IsNullOrWhiteSpace(bgColor))
            fw.WriteLine($"<body style=\"background-color: {bgColor};\">");
        else
            fw.WriteLine("<body>");
        fw.WriteLine(sw.ToString());
        fw.WriteLine("</body>");
        fw.WriteLine("</html>");

        if (view)
            Process.Start(@"""cmd.exe""", "/c \"" + Path.Combine(Directory.GetCurrentDirectory(), filename) + "\"");
    }

    private static void ExportThemes(string script)
    {
        var logger = new Logger();

        var lexer = SimpleCircuitLexer.FromString(script);

        // Parse
        var parsingContext = new ParsingContext() { Diagnostics = logger };
        if (!SimpleCircuitParser.Parse(lexer, parsingContext, out var statements))
            return;
        Console.WriteLine(statements?.ToString() ?? "empty");

        // Evaluate
        var evalContext = new EvaluationContext() { Diagnostics = logger };
        StatementEvaluator.Evaluate(statements, evalContext);
        if (logger.ErrorCount > 0)
            return;

        if (evalContext.Themes.Count == 0)
            evalContext.Themes.Add("light", Style.DefaultThemes["light"]);
        var style = (Style)evalContext.Circuit.Style;
        foreach (var pair in evalContext.Themes)
        {
            // Apply the style colors
            style.Variables.Clear();
            foreach (var color in pair.Value)
                style.Variables[color.Key] = color.Value;
            string filename = evalContext.Themes.Count > 1 ? $"tmp_{pair.Key}.html" : "tmp.html";
            if (!style.Variables.TryGetValue("bg-opaque", out string bgColor))
                bgColor = null;
            Export(filename, evalContext.Circuit, evalContext.Diagnostics, bgColor, view: true);
        }
    }

    private static void ExportDemos(string folder)
    {
        // Make sure the folder exists
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Go through each symbol key
        var logger = new Logger();
        var evalContext = new EvaluationContext() { Diagnostics = logger };
        var parsingContext = new ParsingContext() { Diagnostics = logger };

        // Go through each key in the evaluation context
        foreach (var pair in evalContext.Factory.Factories)
        {
            logger.Reset();
            parsingContext.Reset();
            evalContext.Reset();

            string script = DemoHelper.CreateDemo(pair.Key, pair.Value);
            var lexer = SimpleCircuitLexer.FromString(script, pair.Key);
            
            // Parse
            if (!SimpleCircuitParser.Parse(lexer, parsingContext, out var statements))
                continue;
            if (logger.ErrorCount > 0)
                continue;

            // Evaluate
            StatementEvaluator.Evaluate(statements, evalContext);
            if (logger.ErrorCount > 0)
                continue;

            // Create a filename
            string filename = Path.Combine(folder, $"{pair.Key}.html");
            Export(filename, evalContext.Circuit, logger);
        }
    }
}
