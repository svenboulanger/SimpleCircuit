using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class SvgOutput
    {
        private XmlDocument _doc;
        private string _svg;
        private int _loading;
        private bool _useDom = true, _shrinkToWidth = true, _shrinkToHeight = true;

        [Parameter]
        public bool UseDOM
        {
            get => _useDom;
            set
            {
                if (value != _useDom)
                {
                    _useDom = value;
                    StateHasChanged();
                }
            }
        }

        [Parameter]
        public XmlDocument Svg
        {
            get => _doc;
            set
            {
                // No need to compute again
                if (ReferenceEquals(_doc, value))
                    return;

                _doc = value;
                UpdateSvg();

                // Update
                StateHasChanged();
            }
        }

        private void UpdateSvg()
        {
            if (_doc == null)
                _svg = null;
            else if (_useDom)
            {
                // Remove any styling from the document, as it is defined elsewhere in the document
                var doc = (XmlDocument)_doc.Clone();
                using StringWriter style = new();
                var tags = new List<XmlNode>();
                foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("style"))
                    tags.Add(node);
                foreach (var tag in tags)
                    tag.ParentNode.RemoveChild(tag);
                doc.DocumentElement.SetAttribute("class", $"simplecircuit{(_shrinkToWidth ? " max-width" : "")}{(_shrinkToHeight ? " max-height" : "")}");

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
                    _doc.WriteTo(xml);
                var data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sw.ToString()));
                _svg = $"<img src=\"data:image/svg+xml;base64,{data}\" />";
            }
        }

        [Parameter]
        public int Loading
        {
            get => _loading;
            set
            {
                if (value != _loading)
                {
                    _loading = value;
                    StateHasChanged();
                }
            }
        }

        public void SetShrinkToSize(bool shrinkToWidth, bool shrinkToHeight)
        {
            _shrinkToWidth = shrinkToWidth;
            _shrinkToHeight = shrinkToHeight;
            UpdateSvg();
            StateHasChanged();
        }
    }
}
