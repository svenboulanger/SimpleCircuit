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
        private bool _invalid;

        [Parameter]
        public bool UseDOM { get; set; } = true;

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
            // Update the SVG
            _invalid = false;
            bool update = false, render = true;
            if (parameters.TryGetValue<XmlDocument>(nameof(Svg), out var cSvg) && !ReferenceEquals(Svg, cSvg))
            {
                Svg = cSvg;
                update = true;
            }
            if (parameters.TryGetValue<bool>(nameof(UseDOM), out var cUseDom) && UseDOM != cUseDom)
            {
                UseDOM = cUseDom;
                update = true;
            }
            if (parameters.TryGetValue<bool>(nameof(ShrinkX), out var shrinkX))
                ShrinkX = shrinkX;
            if (parameters.TryGetValue<bool>(nameof(ShrinkY), out var shrinkY))
                ShrinkY = shrinkY;
            if (parameters.TryGetValue<int>(nameof(Loading), out var loading) && Loading != loading)
            {
                Loading = loading;
                render = true;
            }

            if (update)
            {
                if (Svg == null)
                    _invalid = true;
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
                    doc.DocumentElement.SetAttribute("class", $"simplecircuit{(ShrinkX ? " max-width" : "")}{(ShrinkY ? " max-height" : "")}{(_invalid ? " greyed" : "")}");

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
                StateHasChanged();
            }
            else if (render)
                StateHasChanged();
            else
                await base.SetParametersAsync(parameters);
        }
    }
}
