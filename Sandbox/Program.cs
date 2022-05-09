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
            var script = @"// Define a single pixel
.subckt PIXEL DIRleft DIRtop DIRbottom DIRright
// Reset
GND <u> D(photodiode) <u> Xd <u> MNrst <u> POW
MNrst[g] <l 0> T(""RST"")
Xd<r>[g]MNsf[d] < u > POW
MNsf[s] < d r > MNsel < r > Xcol
MNsel[g] < u 60 > Xrow

// Make column and row lines (DIR is used as direction for pins)
            Xcol < u 70 > DIRtop
Xcol < d 15 > DIRbottom
Xrow < l 60 > DIRleft
Xrow < r 20 > DIRright
 .ends

// We have chosen the subcircuit names such that they are unique,
// allowing us to use the short names
            PIXEL1<r> PIXEL2
PIXEL1[DIRbottom] < d > [DIRtop]PIXEL3
PIXEL3<r> PIXEL4[DIRtop] < u > [DIRbottom]PIXEL2

 // Add some terminals
 PIXEL1[DIRtop] < u 0 > T(""COL_{OUT} k"")
PIXEL2[DIRtop] < u 0 > T(""COL OUT k+1"")
PIXEL2[DIRright] < r 0 > T(""ROW SEL i"")
PIXEL4[DIRright] < r 0 > T(""ROW SEL i+1"")
";
            var logger = new Logger();
            var lexer = SimpleCircuitLexer.FromString(script);
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
