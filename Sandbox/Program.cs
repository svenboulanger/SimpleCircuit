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
X1 <n e s w> X1
X2 <ne se sw nw> X2
X3 <a 60 r a -60 a -120 l a 120> X3
X <ne> R - R - R
X7 <u> R <u r> C <r d ??> X7
Xt1 <d arrow d arrow> X
Xt2 <rarrow d arrow> X
Xt3 <arrow d> X
Xt4 <d rarrow> X
Xt5 <d one> X
Xt6 <d many> X
Xt7 <d onemany> X
Xt8 <d zeroone> X
Xt9 <d zeromany> X
Xt10 <d plus> X
Xt11 <d minus> X
Xt12 <d dashed> X
Xt13 <d dotted> X
(y Xt*)

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
