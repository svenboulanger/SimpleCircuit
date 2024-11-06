using SimpleCircuit.Parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Sandbox
{
    class Program
    {
        static void Main()
        {
            // var script = @"V(""This is some longer text"") <u> X";
            var script = "* For more tutorials, go to Help > Demo's.\r\n\r\n* Wires do not have to be horizontal or vertical, SimpleCircuit can solve any angle\r\n* For shorthand notation, the 4 cardinal directions can be used\r\nX1 <n e s w> X1\r\n\r\n* In fact, the 4 ordinal directions can also be used!\r\nX2 <ne se sw nw> X2\r\n\r\n* It is possible to specify any angled wires by using <a #> as a wire segment with # the angle of the wire (counter-clockwise)\r\nX3 <a 60 r a -60 a -120 l a 120> X3\r\n* One note of caution: Using fine increments of angles can lead to rather unexpected results! Uncomment the next example to see what could happen:\r\n* X4 <e n a -91> X4\r\n* In such events, consider using unconstrained wires instead\r\n\r\n* We might just use <?> or '-' to copy the wire orientation from the pin it is connected to\r\nX5 <ne> R <?> R - R\r\n* Sometimes this could lead to errors though, for example:\r\n* X6 - R\r\n\r\n* Unconstrained wires are wires that do not constrain anything\r\n* While this may be useful for very odd angles, it also means that the components will need to be constrained in other ways.\r\n* They are specified using the <??> syntax:\r\nX7 <u> R <u r> C <r d ??> X7\r\n\r\n* Wires can also have special appearances:\r\nX8 <r arrow r arrow> X\r\nX9 <rarrow r arrow> X\r\nX10 <arrow r> X\r\nX11 <r rarrow> X\r\nX12 <r dashed> X\r\nX13 <r dotted> X\r\n";

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
