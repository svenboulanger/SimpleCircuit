using SimpleCircuit;
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
            var logger = new Logger();
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            var evalContext = new EvaluationContext() { Diagnostics = logger };

            // string script = DemoHelper.CreateDemo("XOR", evalContext.Factory, ["Check"]);
            string script = @"
BB1[t1] <u r> R <r d> [t2]BB1
BB1[l1] <l d> C <d r> [l2]BB1
BB1[r1] <r d> L <d l> [r2]BB1
BB1[b1] <d r> XTAL <r u> [b2]BB1
BB1(r=3)
";
            var lexer = SimpleCircuitLexer.FromString(script);

            SimpleCircuitParser.Parse(lexer, context, out var statements);
            Console.WriteLine(statements.ToString());

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
