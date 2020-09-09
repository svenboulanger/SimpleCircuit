using SimpleCircuit;
using SimpleCircuit.Constraints;
using SimpleCircuit.Contributors;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new SimpleCircuitParser();
            var ckt = p.Parse(@"G1 u X1
X1 lu X2
X1 ru X3
X2 rd X4
X3 ru X4
");
            // ckt.AddConstraint(new EqualsConstraint(ckt["X1"].Contributors.First(c => c.Type == UnknownTypes.X), new ConstantContributor(UnknownTypes.X, 0)));
            // ckt.AddConstraint(new EqualsConstraint(ckt["X1"].Contributors.First(c => c.Type == UnknownTypes.Y), new ConstantContributor(UnknownTypes.Y, 0)));
            var doc = ckt.Render();

            using var sw = new StringWriter();
            using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
                doc.WriteTo(xml);

            Console.WriteLine(sw.ToString());
        }
    }
}
