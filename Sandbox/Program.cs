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
        * You can change the array size with these parameters
        .param rows = 3
        .param columns = 3

        * Define a single pixel
        .subckt PIXEL DIRleft DIRtop DIRbottom DIRright fg="--foreground"
            .property *|wire fg={fg}

            * Main branch
            GND <u> D(photodiode, flip) <u> Xd <u> MNrst <u> POW
            Xd <r> [g]MNsf[d] <u> POW

            * Inputs
            MNrst[g] <l> T("RST")
            MNsf[s] <d r> MNsel <r> Xcol
            MNsel[g] <u 60> Xrow

            * Make column and row lines (DIR is used as direction for pins)
            Xcol <u 70> DIRtop
            Xcol <d 15> DIRbottom
            Xrow <l 60> DIRleft
            Xrow <r 20> DIRright
        .ends

        * Now we will use for-loops to make an array of the pixel
        .for r 1 {rows} 1
            .for c 1 {columns} 1
                Xh_{r}_{c} <r> PIXEL_{r}_{c} <r> Xh_{r}_{c+1}
                Xv_{r}_{c} <d> [DIRtop]PIXEL_{r}_{c}[DIRbottom] <d> Xv_{r+1}_{c}
            .endf
        .endf

        * Show the row driver
        .for r 1 {rows} 1
            .param index = {r - round((rows + 1) / 2)}
            T(label1={"ROWSEL_in,y" + (index > 0 ? "+" + index : index == 0 ? "" : index)}, in) <r> Xh_{r}_1
        .endf

        * Show the column output
        .for c 1 {columns} 1
            .param index = {c - round((columns + 1) / 2)}
            T(label1={"COL_out,x" + (index > 0 ? "+" + index : index == 0 ? "" : index)}, out) <u> Xv_{rows+1}_{c}
        .endf

        * Let's make the center pixel in the primary color
        PIXEL_{round((rows + 1) / 2)}_{round((columns + 1) / 2)}(fg="--primary")

        * Let's make the top-left pixel in danger color
        PIXEL_1_1(fg="--danger")
        
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
