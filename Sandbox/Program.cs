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
            var logger = new Logger();
            var lexer = new Lexer(@"
T <r> Xia <r> NAND1 <r> Xoa <r> T
T <r> Xib <r> NAND2 <r> Xob <r> T
NAND1[b] <l d se d> Xob
Xoa <d sw d r> [b]NAND2
(X NAND1[o] <0> [o]NAND2)
");
            var context = new ParsingContext();
            context.Diagnostics = logger;
            Parser.Parse(lexer, context);

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
