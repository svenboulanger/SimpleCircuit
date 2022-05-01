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
            var script = @"// Inputs
Ta(""A"") <r> Xa <r> [a]XOR1
Tb(""B"") < r > Xb<r>[b]XOR1
Tc(""Cin"") < r > Xc<r>[b]XOR2

// First XOR
XOR1[o] < r se r> Xab<r>[a]XOR2
Xab < d r > [a]AND1

  // XOR gate and two AND gates
  XOR2[o] < r > Ts(""S"")
Xc < d r > [b]AND1
Xa < d r > [a]AND2
Xb < d r > [b]AND2

  // Last OR gate for carry out
  AND1[o] < r se r> [a]OR1
   AND2[o] < r ne r> [b]OR1
    OR1[o] < r > Tco(""Cout"")

    // Alignment
    (X Ta < 0 > Tb < 0 > Tc)
    (X Xb < r + 5 > Xa)
    (X Xc < r + 5 > Xab)
    (X XOR2[a] < 0 > [a]AND1[a] < 0 > [a]AND2)
    (X Ts < 0 > Tco)
    (Y XOR2 < d + 15 > AND1 < d + 15 > AND2)
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
