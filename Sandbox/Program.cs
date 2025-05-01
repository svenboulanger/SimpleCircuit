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
.subckt ABC DIR1[in] DIR2[out]
    DIR1 <r> X1
    X1 <u r> R1 <r d> X2
    X1 <d r> C1 <r u> X2
    X2 <r> DIR2
.ends
ABC1 <r d> ABC <d> Xe <l> ABC <l u> ABC <u> Xs <r> ABC1
Xs <a -45> [DIR1_in]ABC[DIR2_out] <a -45 0> L <a -45> Xe
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
