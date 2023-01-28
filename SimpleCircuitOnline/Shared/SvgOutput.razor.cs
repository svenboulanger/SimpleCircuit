using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class SvgOutput
    {
        private string _svg;

        [Parameter]
        public bool UseDOM { get; set; } = true;

        [Parameter]
        public bool Invalid { get; set; }

        [Parameter]
        public EventCallback<bool> InvalidChanged { get; set; }

        [Parameter]
        public XmlDocument Svg { get; set; }

        [Parameter]
        public int Loading { get; set; }

        [Parameter]
        public bool ShrinkX { get; set; } = true;

        [Parameter]
        public bool ShrinkY { get; set; } = true;

        /// <inheritdoc />
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            // Update the SVG
            bool oldInvalid = Invalid;
            Invalid = false;
            if (Svg == null)
                Invalid = true;
            else if (UseDOM)
            {
                // Remove any styling from the document, as it is defined elsewhere in the document
                var doc = (XmlDocument)Svg.Clone();
                using StringWriter style = new();
                var tags = new List<XmlNode>();
                foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("style"))
                    tags.Add(node);
                foreach (var tag in tags)
                    tag.ParentNode.RemoveChild(tag);
                doc.DocumentElement.SetAttribute("class", $"simplecircuit{(ShrinkX ? " max-width" : "")}{(ShrinkY ? " max-height" : "")}{(Invalid ? " greyed" : "")}");

                // Write out the stripped document XML
                using var sw = new StringWriter();
                using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
                    doc.WriteTo(xml);
                _svg = sw.ToString();
            }
            else
            {
                using var sw = new StringWriter();
                using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                    Svg.WriteTo(xml);
                var data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sw.ToString()));
                _svg = $"<img src=\"data:image/svg+xml;base64,{data}\" />";
            }

            // Raise an event if the invalid flag has changed
            if (Invalid != oldInvalid)
                await InvalidChanged.InvokeAsync(Invalid);

            StateHasChanged();
        }
    }
}
