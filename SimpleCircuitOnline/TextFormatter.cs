﻿using Microsoft.JSInterop;
using SimpleCircuit;
using SimpleCircuit.Drawing;
using System;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace SimpleCircuitOnline
{
    public class TextFormatter : IElementFormatter
    {
        private readonly IJSRuntime _runtime;

        public TextFormatter(IJSRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        /// <inheritdoc />
        public Bounds Format(SvgDrawing drawing, XmlElement element)
        {
            string text = null;
            using (var sw = new StringWriter())
            {
                using (var w = XmlWriter.Create(sw, new XmlWriterSettings() { OmitXmlDeclaration = true }))
                {
                    element.WriteTo(w);
                }
                text = sw.ToString();
            }

            // Make a piece of XML that allows measuring this element
            if (element.Name != "svg")
            {
                XmlNode elt = element.ParentNode;
                while (elt != null)
                {
                    text = Enclose(elt, text);
                    elt = elt.ParentNode;
                    if (elt.Name == "svg")
                        break;
                }
                text = $"<svg xmlns=\"http://www.w3.org/2000/svg\"><style>{drawing.Style}</style>{text}</svg>";
            }

            JsonElement obj = ((IJSInProcessRuntime)_runtime).Invoke<JsonElement>("calculateBounds", text);
            double x = obj.GetProperty("x").GetDouble();
            double y = obj.GetProperty("y").GetDouble();
            double width = obj.GetProperty("width").GetDouble();
            double height = obj.GetProperty("height").GetDouble();
            return new Bounds(x, y, x + width, y + height);
        }

        private string Enclose(XmlNode enclose, string xml)
        {
            using var sw = new StringWriter();
            sw.Write('<');
            sw.Write(enclose.Name);
            if (enclose.Attributes != null)
            {
                for (int i = 0; i < enclose.Attributes.Count; i++)
                {
                    var attribute = enclose.Attributes[i];
                    sw.Write(' ');
                    sw.Write(attribute.Name);
                    sw.Write("=\"");
                    sw.Write(attribute.Value);
                    sw.Write('"');
                }
            }
            sw.Write('>');
            sw.Write(xml);
            sw.Write("</");
            sw.Write(enclose.Name);
            sw.Write(">");
            return sw.ToString();
        }
    }
}