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
            var script = @"// Black boxes are boxes that can have custom pins.
// When accessing pins on a black box, the order is important, as they will appear
// from top to bottom or from left to right.
// Additionally, the first letter of the pin indicates n(orth), s(outh), e(ast) or w(est).
// These statements only serve to instantiate the pins in the correct order:
BB1[nVDD]
BB1[wInput1]
BB1[wInput2]
BB1[sVSS]
BB1[eOutput1]
BB1[eOutput2]

// The order of the pins is fixed now, so we can connect whatever we want.
BB1[nVDD] <u> POW
BB1[sVSS] <d> GND

// The pins are ordered, but their spacing can still depend on other elements
BB1[eOutput1] <r d> R <d l> [eOutput2]BB1

// We can also align the pins and resize the black box using them
(X BB1[wInput1] <r 0 l +60> [eOutput1]BB1)
";
            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script);
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
}
