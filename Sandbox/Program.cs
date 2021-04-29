﻿using SimpleCircuit;
using SimpleCircuit.Components;
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
            var ckt = parser.Parse(
                "BB1[nabc] <u> X1" + Environment.NewLine +
                "- BB1.Height = 50");
            ckt.Warning += (sender, args) => Console.WriteLine("WARNING: " + args.Message);
            SimpleCircuit.Functions.Minimizer.LogInfo = true;
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
