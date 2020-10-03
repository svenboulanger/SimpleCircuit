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
            var ckt = parser.Parse(@"// This chain starts with a ground and continues up to V1, up-right to R, right-down to C and down to another ground
GND1 <u> V1 <u r> R <r d> C <d> GND2

// Here we align the y-coordinates of GND1 and GND2 to make it look a little bit nicer
- GND1.y = GND2.y");

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
