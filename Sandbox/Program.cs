using SimpleCircuit;
using SimpleCircuit.Components;
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
            var lexer = new Lexer(string.Join(Environment.NewLine, new[] {
"- pmos1.Packaged = 1",
"- nmos1.Packaged = 1",
"pow <d> [s]pmos1[d] <d> [d]nmos1[s] <d> gnd",
"pmos1[g] <l d r> [g]nmos1"
            }));
            var context = new ParsingContext();
            Parser.Parse(lexer, context);
            SimpleCircuit.Functions.Minimizer.LogInfo = true;
            var ckt = context.Circuit;
            var doc = ckt.Render();

            using var sw = new StringWriter();
            using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
                doc.WriteTo(xml);
            Console.WriteLine(sw.ToString());

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
