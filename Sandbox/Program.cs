using SimpleCircuit.Parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox
{
    public class Program
    {
        static void Main()
        {
            var script = @"* Give all wires a nice curve
.property wire r = 2.5

* Turn arouuuund...
FPta(""Turn\naround"")

* ... every now and then I ...
FPta <r d arrow> FP1(""every now\nand then I"")
FPta <d arrow> FP(""bright eyes"") <r a 80 +30 arrow> FP1

* ... get a little bit ...
FP1 <r arrow> FP2(""get a little bit"" width=50 height=10)

* Lines
.section line1
    FP1(""lonely and you're never coming 'round"" width=140 height=10)
.ends
.section line2 line1
line2/FP1(""tired of listening to the sound of my tears"")
.section line3 line1
line3/FP1(""nervous that the best of all the years have gone by"")
.section line4 line1
line4/FP1(""terrified and then I see the look in your eyes"")
FP2 <d +10 r arrow> line1/FP1 <r u l d arrow> FPta
FP2 <d +25 r arrow> line2/FP1 <r u l d arrow> FPta
FP2 <d +40 r arrow> line3/FP1 <r u l d arrow> FPta
FP2 <d +55 r arrow> line4/FP1 <r u l d arrow> FPta

* ... fall apart ...
FP1 <d +50 arrow> FPfa(""fall apart"")
FPfa <d> FPny(""and I\nneed you"")

* This is a little hack to allow you to connect to different positions
FPny <a 30 0 r> FPnt(""now,\ntonight"") <d +0 l arrow a 150 0> FPny
FPny <d r arrow> FP(""more than\never!!"")

.css
#FPta polygon { fill: rgb(200, 255, 200); }
#FPny polygon { fill: rgb(200, 200, 255); }
.endcss
";

            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script.AsMemory());
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            Parser.Parse(lexer, context);
            context.Circuit.Metadata.Add("script", script);

            // Draw the component
            if (context.Circuit.Count > 0 && logger.ErrorCount == 0)
            {
                context.Circuit.RenderBounds = false;
                var doc = context.Circuit.Render(logger);
                if (doc == null)
                    return;
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
                Process.Start(@"""cmd.exe""", "/c \"" + Path.Combine(Directory.GetCurrentDirectory(), "tmp.html") + "\"");
            }
        }
    }
}
