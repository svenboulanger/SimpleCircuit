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
            // var script = @"V(""This is some longer text"") <u> X";
            var script = "* For more tutorials, go to Help > Demo's.\r\n\r\n* Subcircuits are solved separately on their own, after which they act like a component\r\n* The pins need to be specified\r\n.subckt ABC DIR1[in] DIR2[out]\r\n    DIR1 <r> X1\r\n    X1 <u r> R1 <r d> X2\r\n    X1 <d r> C1 <r u> X2\r\n    X2 <r> DIR2\r\n.ends\r\n\r\n* Now we can instantiate this subcircuit definition multiple times.\r\nABC1 <r d> ABC <d> Xe <l> ABC <l u> ABC <u> Xs <r> ABC1\r\n\r\n* They can even be angled because our pins also have a direction!\r\n* Also showing how you can refer to pins\r\nXs <a -45> [DIR1_in]ABC[DIR2_out] <a -45 0> L <a -45> Xe\r\n";

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
