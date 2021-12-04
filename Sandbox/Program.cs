using SimpleCircuit;
using SimpleCircuit.Components.Analog;
using SimpleCircuit.Components.General;
using SimpleCircuit.Parser;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a drawable from XML
            var xmlDrawable = @"
<symbol>
    <pin name=""a"" x=""-4"" y=""0"" nx=""-1"" ny=""0"" />
    <pin name=""b"" x=""4"" y=""0"" nx=""1"" ny=""0"" />
    <drawing>
        <circle cx=""0"" cy=""0"" r=""4"" />
        <text x=""0"" y=""0"" value=""M"" />
    </drawing>
</symbol>";
            var xmlDrawableDoc = new XmlDocument();
            xmlDrawableDoc.LoadXml(xmlDrawable);

            var script = @"GND1 <u> V <u a -30> TL <a -30 d> C(""C"") <d> GND2
(XY GND1 <r> GND2)
";
            var logger = new Logger();
            var lexer = new Lexer(script);
            var context = new ParsingContext();
            context.Diagnostics = logger;
            Parser.Parse(lexer, context);
            context.Circuit.Metadata.Add("script", script);

            // Draw the component
            var doc = context.Circuit.Render(logger);
            using var sw = new StringWriter();
            using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
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
