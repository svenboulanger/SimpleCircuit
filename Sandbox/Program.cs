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
            var script = @"T <r 10> [b]AND2
XOR1 <r 10 se r 10> AND2";

            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script.AsMemory());
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            Parser.Parse(lexer, context);
            context.Circuit.Metadata.Add("script", script);

            // Draw the component
            if (context.Circuit.Count > 0 && logger.ErrorCount == 0)
            {
                context.Circuit.RenderBounds = false;
                var doc = context.Circuit.Render(logger);
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
