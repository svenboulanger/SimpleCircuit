using SimpleCircuit.Parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var script = string.Join(Environment.NewLine, new[]
            {
                "// A component chain is a series of components seperated by <wires>.",
                "// The type of component is defined by the first letter(s), which have to be capital letters.",
                "// Wires can be defined between '<' and '>', using their direction: u, d, l, r for up, down, left or right.",
                "GND1 <u +5> V1(\"1V\") </ u +5 r /> R(\"1k\") <r d +5> C1(\"1uF\") <d +5> GND2",
                "",
                "// In a lot of cases, we wish to align pins or components. This can be done using virtual chains.",
                "// These are between brackets, and first indicate along which axis you wish to align.",
                "(Y GND1 <0> GND2)"
            });
            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script);
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            Parser.Parse(lexer, context);
            context.Circuit.Metadata.Add("script", script);

            // Draw the component
            var doc = context.Circuit.Render(logger);
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
            Process.Start(@"""C:\Program Files (x86)\Google\Chrome\Application\chrome.exe""", "\"" + Path.Combine(Directory.GetCurrentDirectory(), "tmp.html") + "\"");
        }
    }
}
