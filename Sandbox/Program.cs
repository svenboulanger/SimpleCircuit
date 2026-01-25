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
        * --- Queued Anonymous Points and Queued Margins ---

        * Queued Anonymous Points and Queued Margins are
        * completely optional. They are meant to speed up
        * describing a circuit, but you can avoid them at
        * any point.

        .options SpacingY = 20

        .box "Queued Anonymous Points" r=3 anchor1="e" m=5
            * Queued Anonymous Points are quick points that are
            * added in a wire definition using an 'x'. This
            * point is then queued, and can be reused later.
            * This avoids having to invent a lot of names for
            * points.

            * For example, let's say we have the following:
            T(in) <r> X1 <r> C <r> X2 <r> T(out)
            X1 <u r> S <r d> X2

            * We can also used Queued Anonymous Poins instead.
            * When then using anonymous points using 'X', the
            * Queued Anonymous Points will be used first, in
            * the same order as they were created!
            T(in) <r x r> C <r x r> T(out)
            X <u r> S <r d> X
        .endb


        .box "Queued Margins" r=3 anchor1="e" m=5
            * Queued Margins are a way of describing a virtual
            * wire together with a normal wire. For example,
            * to avoid overlap between the following resistors,
            * we may add a '++5' virtual wire that uses the
            * bounds of the resistors to space them apart.
            R1("R_1") <r u l> R2("R_2")
            (y R1 <u ++5> R2)

            * The same circuit can be described using a
            * Queued Margin as follows:
            R1b("R_1") <r u ++5 l> R2b("R_2")

            * Every time a component that is eligible is
            * reference, SimpleCircuit will trace back
            * until the last eligible component and automatically
            * adds a virtual wire with any queued margins in
            * between.
        .endb
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
        StatementEvaluator.EvaluateOptions(parsingContext.GlobalOptions, evalContext);
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
