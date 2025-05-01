using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox
{
    public class Program
    {
        static void Main()
        {
            string script = @"
A/Xo <r d> C <d> GND1
.section A
    GND1 <u> V1 <u r> TL <r> Xo
    GND1(signal)
.endsection
(y A/GND1 <r> GND1)

* You can re-use previously defined sections
.section B A
B/Xo <r d> L <d> GND2
(y B/GND1 <r> GND2)

.section C A
C/Xo <r d> R <d> GND3
(y C/GND1 <r> GND3)


* Or we can also align instances across sections
(y */V1)
";

            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script);
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            SimpleCircuitParser.Parse(lexer, context, out var statements);
            Console.WriteLine(statements.ToString());

            var evalContext = new EvaluationContext() { Diagnostics = logger };
            StatementEvaluator.Evaluate(statements, evalContext);

            evalContext.Circuit.Metadata.Add("script", script);

            // Draw the component
            if (evalContext.Circuit.Count > 0 && logger.ErrorCount == 0)
            {
                evalContext.Circuit.RenderBounds = false;
                var doc = evalContext.Circuit.Render(logger);
                if (doc == null)
                    return;
                using var sw = new StringWriter();
                using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
                    doc.WriteTo(xml);
            
                if (File.Exists("tmp.html"))
                    File.Delete("tmp.html");
                using (var fw = new StreamWriter(File.OpenWrite("tmp.html")))
                {
                    fw.WriteLine("<html>");
                    fw.WriteLine("<head>");
                    fw.WriteLine("</head>");
                    fw.WriteLine("<body>");
                    fw.WriteLine(sw.ToString());
                    fw.WriteLine("</body>");
                    fw.WriteLine("</html>");
                }
                Process.Start(@"""cmd.exe""", "/c \"" + Path.Combine(Directory.GetCurrentDirectory(), "tmp.html") + "\"");
            }
        }
    }
}
