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
* For more tutorials, go to Help > Demo's.

* Variants allow changing the appearance of certain components
* For example, a resistor can have the ""programmable"" variant:
T1(""in"") <r> R1(programmable) <r> T2(""out"")

* Many components also have properties that can be specified as well
R1(scale=2 zigs=7)

* The property syntax can also be used to specify variants
T1(input)
T2(output)

* Variants can be removed again by adding a '-' before them
T2(-output, +pad)
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
