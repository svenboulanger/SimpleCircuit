using SimpleCircuit;
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
            var parser = new SimpleCircuitParser();
            var ckt = parser.Parse(@"gnd1 <u> R1 <u> X1 <u> R2 <u> POW1
X1 <r 30> X2 <u> C <u> X3 <u> L <u> POW2
X2 <d r> X10 <r> [b]npn
- POW1.y = POW2.y");

            SimpleCircuit.Functions.Minimizer.LogInfo = true;
            var doc = ckt.Render();

            var style = doc.CreateElement("style", SvgDrawing.Namespace);
            style.InnerText = @"path, polyline, line, circle { stroke: black; stroke-width: 0.5pt;
                fill: transparent; stroke-linecap: round; stroke-linejoin: round; }
            .point circle { fill: black; }
            .plane { stroke-width: 1pt; }
            text { font: 4pt Tahoma, Verdana, Segoe, sans-serif; }";
            doc.DocumentElement.PrependChild(style);

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
                /* fw.WriteLine("<style>");
                fw.WriteLine(@"
                path, polyline, line, circle {
                    stroke: black;
                    stroke-width: 0.5pt;
                    fill: transparent;
                    stroke-linecap: round;
                }
                .point circle {
                    fill: black;
                }
                .plane {
                    stroke-width: 1pt;
                }
                text {
                    font: 4pt Tahoma, Verdana, Segoe, sans-serif;
                }");
                fw.WriteLine("</style>"); */
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
