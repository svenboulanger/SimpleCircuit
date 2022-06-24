using Microsoft.AspNetCore.Components;
using SimpleCircuit;
using SimpleCircuit.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class ComponentHelp
    {
        private MarkupString _svg;
        private const double MaxPreviewWidth = 90;
        private IDrawable _drawable;
        private List<(PropertyInfo, string)> _properties;

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public DrawableMetadata Metadata { get; set; }

        [Parameter]
        public IDrawable Drawable
        {
            get => _drawable;
            set
            {
                _properties = null;
                _drawable = value;

                // Show where the label is
                if (_drawable is ILabeled labeled && string.IsNullOrWhiteSpace(labeled.Label))
                    labeled.Label = "label";
                CreateSvg();
            }
        }

        /// <summary>
        /// Gets the properties of the component.
        /// </summary>
        protected List<(PropertyInfo, string)> Properties
        {
            get
            {
                if (_properties == null && _drawable != null)
                {
                    _properties = new();
                    foreach (var p in _drawable.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (!p.CanRead || !p.CanWrite)
                            continue;
                        if (p.PropertyType != typeof(double)
                            && p.PropertyType != typeof(bool)
                            && p.PropertyType != typeof(string)
                            && p.PropertyType != typeof(int))
                            continue;
                        string description = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
                        _properties.Add((p, description));
                    }
                }
                return _properties;
            }
        }

        private void CreateSvg()
        {
            if (Metadata == null || _drawable == null)
            {
                _svg = default;
                return;
            }

            var drawing = new SvgDrawing
            {
                Style = GraphicalCircuit.DefaultStyle,
                ElementFormatter = _jsTextFormatter
            };
            _drawable.Render(drawing);
            var doc = drawing.GetDocument();

            // Try to resize the component
            if (!double.TryParse(doc.DocumentElement.GetAttribute("width"), out double w))
                w = 16.0;
            if (!double.TryParse(doc.DocumentElement.GetAttribute("height"), out double h))
                h = 16.0;
            if (w > MaxPreviewWidth)
            {
                h = h / w * MaxPreviewWidth;
                w = MaxPreviewWidth;
            }
            doc.DocumentElement.SetAttribute("width", $"{w:F0}px");
            doc.DocumentElement.SetAttribute("height", $"{h:F0}px");

            using var sw = new StringWriter();
            using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                doc.WriteTo(xml);

            // Converting to Base 64 allows us to not interfere with any styling going on outside
            _svg = (MarkupString)$"<img src=\"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sw.ToString()))}\" />";
        }

        private void ToggleVariant(string name)
        {
            if (_drawable != null)
            {
                if (_drawable.Variants.Contains(name))
                    _drawable.Variants.Remove(name);
                else
                    _drawable.Variants.Add(name);
                _drawable.Variants.Reset();
                CreateSvg();
            }
            StateHasChanged();
        }

        private string GetTypeName(PropertyInfo property)
        {
            var type = property.PropertyType;
            string typeName;
            if (type == typeof(double) || type == typeof(int))
                typeName = "number";
            else if (type == typeof(string))
                typeName = "string";
            else if (type == typeof(bool))
                typeName = "boolean";
            else
                typeName = "?";
            return typeName;
        }
    }
}
