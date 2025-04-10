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
.symbol ARROW
    <pins>
        <pin name=""A"" x=""0"" y=""0"" nx=""-1"" ny=""0"" />
        <pin name=""B"" x=""10"" y=""0"" nx=""1"" ny=""0"" />
    </pins>
    <drawing>
        <line x1=""0"" y1=""0"" x2=""5"" y2=""-5"" marker-end=""arrow"" />
        <line x1=""8"" y1=""0"" x2=""10"" y2=""0"" />
    </drawing>
.endsymbol
R <a 45> ARROW(flip) <a 45> R
";

            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script);
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            SimpleCircuitParser.Parse(lexer, context, out var statements);
            foreach (var stmt in statements)
                Console.WriteLine(stmt.ToString());

            var evalContext = new EvaluationContext(context);
            StatementEvaluator.Evaluate(statements, evalContext);

            context.Circuit.Metadata.Add("script", script);

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
