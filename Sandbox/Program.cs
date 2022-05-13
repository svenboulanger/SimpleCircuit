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
            var script = @"
.symbol TG
    <pin x=""33"" y=""-1.8"" nx=""0"" ny=""-1"" name=""g"" />
    <pin x=""38.5"" y=""0"" nx=""0"" ny=""-1"" name=""fd"" />
    <drawing scale=""2"">
        <path d = ""M 0,0 H 21.41547 V 8.4099703 H 0 Z"" style = ""fill:#ffe9a6; stroke:none;"" />
        <path d = ""M 0,0 V 3.6855632 H 13.211597 c 1.282568,0 2.315104,-1.0325367 2.315104,-2.3151042 V 0 Z"" style = ""fill:#b49ddf;stroke:none;"" />
        <path d = ""M 0,0 H 15.077854 V 0.51971721 H 0 Z"" style = ""fill:#ffc107;stroke:none;"" />
        <path d = ""M 0,7.8902531 H 21.415472 V 8.4099703 H 0 Z"" style = ""fill:#ffc107;stroke:none;"" />
        <path d = ""m 18.00751,0 v 0.73483891 c 0,0.48121869 0.387464,0.86868079 0.86868,0.86868079 h 0.918808 c 0.481219,0 0.86868,-0.3874621 0.86868,-0.86868079 V 0 Z"" style = ""fill:#b49ddf;stroke:none;"" />
        <path d = ""m 15.2757,-0.90203458 h 2.832035 V 0 H 15.2757 Z"" style = ""fill:#dc3545;stroke:none;"" />
    </drawing>
.endsymbol

TG1[fd] <u> T(""fd"")
TG1[g] <u> T(""tg"")
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
