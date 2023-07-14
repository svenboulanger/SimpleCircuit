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
            var script = @"// For more tutorials, go to Help > Demo's.

// A component chain is a series of components seperated by <wires>.
// The type of component is defined by the first letter(s), which have to be capital letters.
// Wires can be defined between '<' and '>', using their direction: u, d, l, r for up, down, left or right.
// Most components also have labels, which are specified as quoted strings between parenthesis.
GND1 <u> V1(""1V"") <u r> R(""1k"") <r d> C1(""1uF"") <d> GND2

// Virtual chains act like component chains but are not drawn.
// They can be used to align components.
// Virtual chains are always between brackets.
(GND1 <r> GND2)
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
