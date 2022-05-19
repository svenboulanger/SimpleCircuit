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
            var script = @".subckt CTIA DIRo
    .options minimumwirelength = 5
    .options scale = 0.75
    A1 <r> Xo <r 0> DIRo
    - A1.scale = 0.5
    Xo <u l> C <l d +10> Xi <r 5> [i]A1
    Xi <d> [n]D(photodiode, flip)[p] <d 3> POW("""")
.ends

CTIA <u> A(""1"") <u> TL <u> A1(""1"") <u> X1
CTIA <u> A(""1"") <u> TL <u> A2(""1"") <u> X2
CTIA <u> A(""1"") <u> TL <u> A3(""1"") <u> X3
X1 <r +35> X2 <r> X <r +10 dashed> Xo <r +10 dashed> X <r> X3

T(""enable"") <r> BUS <r> Xb1 <u r 5 arrow> [vp]A1
Xb1 <r> Xb2 <u r 5 arrow> [vp]A2
Xb2 <r 10> X <r dashed> X <r 10> Xb3
Xb3 <u r 5 arrow> [vp]A3

T(""enable"") <r> BUS2 <r> Xbb1

Xo <u> Ao(""1"") <u> T(""bondpad"")
- Ao.scale = 1.5
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
