using SimpleCircuit.Parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox
{
    class Program
    {
        static void Main()
        {
            var script = @"
// Input to adder
T(in, ""input"") <r arrow plus> ADD1

// Adder to gain block
ADD1 <r> DIR(""e"", flip) <r arrow> BLOCK1(""G"")

// Gain block and output
BLOCK1 <r> Xout(""y"") <r> T(out, ""output"")
- Xout.angle = 90

// Feedback branch
// Note that the direction of the wire determines
// on which side the symbol is connected
Xout <d +20 l arrow> BLOCKfb(""&#946;"") <l u arrow minus> ADD1

.box b1(""Analog"", radius=3, margin=3, poly)
    BLOCK1
    BLOCKfb
.endb
";
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
