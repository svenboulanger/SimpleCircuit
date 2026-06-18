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
        * Example for ERD diagrams

        * General styling
        .variant ENT*|ENT r=2
        + header-bg="--primary" header-fg="white"
        + odd-bg="#eeeeee" odd-fg="black" odd-fontsize=3
        + even-fontsize=3

        * Define the tables
        ENTplayers("Players", "Player Id &#128273;",
        + "First name", "Last name")
        ENTgame("Games", "Game Id &#128273;", 
        + "Player 1 Id &#8674;", "Player 2 Id &#8674;",
        + "Score 1", "Score 2", "Score 3", "Date")
        ENTranking("Ranking", "Ranking Id &#128273;",
        + "Player Id &#8674;",
        + "Date", "Rank")
        ENTtournament("Tournament", "Tournament Id &#128273;"
        + "Name", "Date")
        ENTcompetition("Competition", "Competition Id &#128273;"
        + "Name", "StartDate")
        ENTmeeting("Competition Meeting", "Meeting Id &#128273;"
        + "Competition Id &#8674;",
        + "Date")

        * Display the links
        ENTgame <erd-one-many r 10 u r erd-only-one> ENTplayers
        ENTplayers <erd-only-one d r erd-zero-many> ENTranking

        ENTmeeting <erd-zero-many r d erd-zero-many> ENTplayers
        ENTcompetition <erd-zero-many r d erd-zero-many> ENTplayers
        ENTtournament <erd-zero-many r d erd-zero-many> ENTplayers

        ENTcompetition <erd-only-one l d r erd-zero-many> ENTmeeting

        .option resolveoverlaps = true
        
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
