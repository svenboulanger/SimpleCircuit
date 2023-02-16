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
            var script = @"// Top diode
Tlt <r> Xlt <r> D1 <r> Xrt <r> Trt(""+"")

// Bottom diode
Tlb <r> Xlb <r> [n]D4[p] <r> Xrb <r> Trb(""-"")

// Cross diodes
Xlb <u r> D2 <r u> Xrt
Xrb <u l> D3 <l u> Xlt

// Space the diodes apart for at least 15pt
(y D1 <d +15> D2 <d +15> D3 <d +15> D4)

// Alignment of some wires
(x Xlt <r 5> Xlb)

// Align the terminals
(x Tlt <d> Tlb)
(x Trt <d> Trb
(x D*)
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
