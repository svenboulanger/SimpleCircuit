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
            var script = @".subckt M DIRpa[i] DIRpb[o] DIRsa[i] DIRsb[o]
DIRpa <d 0> L1(dot) <d 0> DIRpb
DIRsa <d 0> L2(dot) <d 0> DIRsb
(X L1 <r 10> L2)
(Y L1[p] <0> L2)
- L2.Flipped = true
.ends

// Primary side
V1 <u r d> [DIRpa]M1[DIRpb] <d l u> V1

// Secondary side to second transformer
M1[DIRsa] <u r d> [DIRpa]M2[DIRpb] <d l u> [DIRsb]M1

// Load
M2[DIRsa] <u r d> RL <d l u> [DIRsb]M2

// Alignment
(X V1 <r +25> M1 <r +25> M2 <r +25> RL)
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
