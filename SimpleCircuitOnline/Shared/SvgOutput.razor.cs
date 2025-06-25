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
        public List<(string Theme, XmlDocument Document, string Background)> Svg { get; set; }

        [Parameter]
        public int Loading { get; set; }

        [Parameter]
        public bool ShrinkX { get; set; } = true;

        [Parameter]
        public bool ShrinkY { get; set; } = true;

        public void UpdateSvg(object sender, EventArgs args)
        {
            if (Svg is not null)
            {
                if (Svg.Count > 0 && Svg[0].Document is not null)
                {
                    using var sw = new StringWriter();
                    using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                        Svg[0].Document.WriteTo(xml);
                    var data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sw.ToString()));
                    string classes = $"{(ShrinkX ? "max-width" : "")}{(ShrinkY ? " max-height" : "")}{(_invalid ? " greyed" : "")}";
                    _svg = $"<img class=\"{classes}\" style=\"background-color:{Svg[0].Background ?? "white"};\" src=\"data:image/svg+xml;base64,{data}\" />";
                }
                else
                    _svg = string.Empty;
            }
            else if (_svg != string.Empty)
                _svg = string.Empty;
            StateHasChanged();
        }

        /// <inheritdoc />
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            // Update the SVG
            _invalid = false;
            bool update = false, render = false;
            if (parameters.TryGetValue<List<(string, XmlDocument, string)>>(nameof(Svg), out var cSvg) && !ReferenceEquals(Svg, cSvg))
            {
                Svg = cSvg;
                update = true;
            }
            if (parameters.TryGetValue<bool>(nameof(UseDOM), out var cUseDom) && UseDOM != cUseDom)
            {
                UseDOM = cUseDom;
                update = true;
            }
            if (parameters.TryGetValue<bool>(nameof(ShrinkX), out var shrinkX) && ShrinkX != shrinkX)
            {
                ShrinkX = shrinkX;
                update = true;
            }
            if (parameters.TryGetValue<bool>(nameof(ShrinkY), out var shrinkY) && ShrinkY != shrinkY)
            {
                ShrinkY = shrinkY;
                update = true;
            }
            if (parameters.TryGetValue<int>(nameof(Loading), out var loading) && Loading != loading)
            {
                Loading = loading;
                render = true;
            }

            if (update)
                UpdateSvg(this, EventArgs.Empty);
            else if (render)
                StateHasChanged();
            else
                await base.SetParametersAsync(parameters);
        }
    }
}
