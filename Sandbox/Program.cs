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
".subckt ABC R1[p] R2[n]",
"R1 <r> R2",
".ends",
"",
"ABC1 <r d> ABC <d> Xe <l> ABC <l u> ABC <u> Xs <r> ABC1",
"",
"Xs <?> ABC <?> Xe",
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
