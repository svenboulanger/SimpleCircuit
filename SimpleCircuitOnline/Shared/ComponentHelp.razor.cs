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
        private const double MaxPreviewWidth = 90;
        private IDrawable _drawable;
        private List<(PropertyInfo, string)> _properties;
        private Dictionary<string, bool> _variants;

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
                _variants = null;
                _drawable = value;
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

        protected Dictionary<string, bool> Variants
        {
            get
            {
                if (_variants == null && _drawable != null)
                {
                    var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    _drawable.CollectPossibleVariants(set);
                    _variants = new(StringComparer.OrdinalIgnoreCase);
                    foreach (string name in set)
                        _variants.Add(name, _drawable.HasVariant(name));
                }
                return _variants;
            }
        }

        private string CreateSvg()
        {
            if (Metadata == null || _drawable == null)
                return "";

            // We can update the variants
            if (_variants != null)
            {
                foreach (var variant in _variants)
                {
                    if (variant.Value)
                        _drawable.AddVariant(variant.Key);
                    else
                        _drawable.RemoveVariant(variant.Key);
                }
            }

            var drawing = new SvgDrawing
            {
                Style = GraphicalCircuit.DefaultStyle
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
            return $"<img src=\"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sw.ToString()))}\" />";
        }

        private void ToggleVariant(string name)
        {
            _variants[name] = !_variants[name];
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
