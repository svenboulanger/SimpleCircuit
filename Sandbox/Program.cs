using SimpleCircuit;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
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
            var logger = new Logger();
            var context = new ParsingContext
            {
                Diagnostics = logger
            };
            var evalContext = new EvaluationContext() { Diagnostics = logger };

            // string script = DemoHelper.CreateDemo("WP", evalContext.Factory, ["VDD"]);
            string script = @"
.theme dark
.theme light foreground=""blue""

GND <u> V(""Hello"") <u r> R <r d> C <d> GND
(y GND)
";
            // string script = "* For more tutorials, go to Help > Demo's.\r\n\r\n* A symbol is custom definition of a component with SVG-like XML for drawing any shape\r\n* The following symbol defines a seven-segment display, based on the\r\n* SVG file from Wikipedia\r\n.symbol segment\r\n    <pins>\r\n        <pin name=\"a\" x=\"0\" y=\"3.625\" nx=\"-1\" />\r\n        <pin name=\"b\" x=\"0\" y=\"10.875\" nx=\"-1\" />\r\n        <pin name=\"c\" x=\"0\" y=\"18.125\" nx=\"-1\" />\r\n        <pin name=\"d\" x=\"0\" y=\"25.375\" nx=\"-1\" />\r\n        <pin name=\"plus\" x=\"8.75\" y=\"29\" ny=\"1\" />\r\n        <pin name=\"e\" x=\"17.5\" y=\"25.375\" nx=\"1\" />\r\n        <pin name=\"f\" x=\"17.5\" y=\"18.125\" nx=\"1\" />\r\n        <pin name=\"g\" x=\"17.5\" y=\"10.875\" nx=\"1\" />\r\n        <pin name=\"h\" x=\"17.5\" y=\"3.625\" nx=\"1\" />\r\n    </pins>\r\n    <!-- Note that the full SVG specification is not supported. -->\r\n    <!-- Only basic SVG XML is supported due to it needing to interface with SimpleCircuit. -->\r\n    <drawing scale=\"0.05\">\r\n        <rect x=\"0\" y=\"0\" width=\"350\" height=\"580\" style=\"fill: black;\" />\r\n    \t<g>\r\n            <!-- The \"variant\" attribute will control when a tag is drawn -->\r\n            <!-- Middle bar -->\r\n            <polygon points=\"279,288 243,324 99,324 63,288 99,252 243,252\" style=\"fill:red; stroke: none;\" variant=\"!(zero or one or seven)\" />\r\n            <polygon points=\"279,288 243,324 99,324 63,288 99,252 243,252\" style=\"fill:#666; stroke: none;\" variant=\"zero or one or seven\" />\r\n\r\n            <!-- Bottom bar -->\r\n            <polygon points=\"279,521 243,557 99,557 63,521 99,485 243,485\" style=\"fill:red; stroke: none;\" variant=\"!(one or four or seven)\" />\r\n            <polygon points=\"279,521 243,557 99,557 63,521 99,485 243,485\" style=\"fill:#666; stroke: none;\" variant=\"one or four or seven\" />\r\n\r\n            <!-- Bottom-right bar -->\r\n            <polygon points=\"288,296 324,332 324,476 288,512 252,476 252,332\" style=\"fill:red; stroke: none;\"  variant=\"!two\" />\r\n            <polygon points=\"288,296 324,332 324,476 288,512 252,476 252,332\" style=\"fill:#666; stroke: none;\" variant=\"two\" />\r\n\r\n            <!-- Bottom-left bar -->\r\n            <polygon points=\"54,296 90,332 90,476 54,512 18,476 18,332\" style=\"fill:red; stroke: none;\" variant=\"!(one or three or four or five or seven or nine)\" />\r\n            <polygon points=\"54,296 90,332 90,476 54,512 18,476 18,332\" style=\"fill:#666; stroke: none;\" variant=\"one or three or four or five or seven or nine\" />\r\n\r\n            <!-- Top bar -->\r\n            <polygon points=\"279,55 243,91 99,91 63,55 99,19 243,19\" style=\"fill:red; stroke: none;\" variant=\"!(one or four)\" />\r\n            <polygon points=\"279,55 243,91 99,91 63,55 99,19 243,19\" style=\"fill:#666; stroke: none;\" variant=\"one or four\" />\r\n\r\n            <!-- top-right bar -->\r\n            <polygon points=\"288,64 324,100 324,244 288,280 252,244 252,100\" style=\"fill:red; stroke: none;\" variant=\"!(five or six)\" />\r\n            <polygon points=\"288,64 324,100 324,244 288,280 252,244 252,100\" style=\"fill:#666; stroke: none;\" variant=\"five or six\" />\r\n\r\n            <!-- top-left bar -->\r\n            <polygon points=\"54,64 90,100 90,244 54,280 18,244 18,100\" style=\"fill:red; stroke: none;\"  variant=\"!(one or two or three or seven)\" />\r\n            <polygon points=\"54,64 90,100 90,244 54,280 18,244 18,100\" style=\"fill:#666; stroke: none;\" variant=\"one or two or three or seven\" />\r\n        </g>\r\n        <label x=\"0\" y=\"-40\" nx=\"1\" ny=\"-1\" />\r\n    </drawing>\r\n.ends\r\n\r\n* Some terminals, just to be fancy we give it some odd angle\r\nsegment1[a] <l> T(\"a\")\r\nsegment1[b] <l> T(\"b\")\r\nsegment1[c] <l> T(\"c\")\r\nsegment1[d] <l> T(\"d\")\r\nsegment1[e] <r> T(\"e\")\r\nsegment1[f] <r> T(\"f\")\r\nsegment1[g] <r> T(\"g\")\r\nsegment1[h] <r> T(\"h\")\r\n\r\nsegment2[a] <l> T(\"a\")\r\nsegment2[b] <l> T(\"b\")\r\nsegment2[c] <l> T(\"c\")\r\nsegment2[d] <l> T(\"d\")\r\nsegment2[e] <r> T(\"e\")\r\nsegment2[f] <r> T(\"f\")\r\nsegment2[g] <r> T(\"g\")\r\nsegment2[h] <r> T(\"h\")\r\n\r\nsegment1[plus] <d r +30 x r +30 u> [plus]segment2\r\nX <d> T(\"+\")\r\n\r\n* Add a label and variants\r\nsegment1(\"label A\", four)\r\nsegment2(\"label B\", two)\r\n";
            var lexer = SimpleCircuitLexer.FromString(script);

            SimpleCircuitParser.Parse(lexer, context, out var statements);
            Console.WriteLine(statements?.ToString() ?? "empty");

            StatementEvaluator.Evaluate(statements, evalContext);

            evalContext.Circuit.Metadata.Add("script", script);

            // Draw the component
            if (evalContext.Circuit.Count > 0 && logger.ErrorCount == 0)
            {
                if (evalContext.Themes.Count == 0)
                    evalContext.Themes.Add("light", Style.DefaultThemes["light"]);
                var style = (Style)evalContext.Circuit.Style;
                foreach (var pair in evalContext.Themes)
                {
                    // Apply the style colors
                    style.Variables.Clear();
                    foreach (var color in pair.Value)
                        style.Variables[color.Key] = color.Value;
                    string filename = evalContext.Themes.Count > 1 ? $"tmp_{pair.Key}.html" : "tmp.html";
                    if (!style.Variables.TryGetValue("bg-opaque", out string bgColor))
                        bgColor = null;
                    Export(filename, evalContext.Circuit, logger, bgColor);
                }
            }
        }

        private static void Export(string filename, GraphicalCircuit circuit, IDiagnosticHandler diagnostics, string bgColor = null)
        {
            var doc = circuit.Render(diagnostics);
            if (doc == null)
                return;
            using var sw = new StringWriter();
            using var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true });
            doc.WriteTo(xml);
            xml.Close();

            if (File.Exists(filename))
                File.Delete(filename);
            using var fw = new StreamWriter(File.OpenWrite(filename));
            fw.WriteLine("<html>");
            fw.WriteLine("<head>");
            fw.WriteLine("</head>");
            if (!string.IsNullOrWhiteSpace(bgColor))
                fw.WriteLine($"<body style=\"background-color: {bgColor};\">");
            else
                fw.WriteLine("<body>");
            fw.WriteLine(sw.ToString());
            fw.WriteLine("</body>");
            fw.WriteLine("</html>");

            Process.Start(@"""cmd.exe""", "/c \"" + Path.Combine(Directory.GetCurrentDirectory(), filename) + "\"");
        }
    }
}
