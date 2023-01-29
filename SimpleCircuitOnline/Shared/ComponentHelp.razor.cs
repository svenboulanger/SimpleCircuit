using Microsoft.AspNetCore.Components;
using SimpleCircuit;
using SimpleCircuit.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class ComponentHelp
    {
        private MarkupString _svg;
        private const double MaxPreviewWidth = 90;
        private List<(PropertyInfo, string)> _properties;

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public DrawableMetadata Metadata { get; set; }

        [Parameter]
        public IDrawable Drawable { get; set; }

        /// <summary>
        /// Gets the properties of the component.
        /// </summary>
        protected List<(PropertyInfo, string)> Properties
        {
            get
            {
                if (_properties == null && Drawable != null)
                {
                    _properties = new();
                    foreach (var p in Drawable.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
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

        /// <inheritdoc />
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (parameters.TryGetValue<IDrawable>(nameof(Drawable), out var drawable))
            {
                _properties = null;

                // Show where the primary label is
                Drawable = drawable;
                if (drawable is ILabeled labeled && string.IsNullOrWhiteSpace(labeled.Labels[0]))
                    labeled.Labels[0] = "label";
                CreateSvg();
            }
            await base.SetParametersAsync(parameters);
        }

        private void CreateSvg()
        {
            if (Metadata == null || Drawable == null)
            {
                _svg = default;
                return;
            }

            var drawing = new SvgDrawing
            {
                Style = GraphicalCircuit.DefaultStyle,
                ElementFormatter = _jsTextFormatter
            };
            Drawable.Reset();
            Drawable.Render(drawing);
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
            if (Drawable != null)
            {
                if (Drawable.Variants.Contains(name))
                    Drawable.Variants.Remove(name);
                else
                    Drawable.Variants.Add(name);
                Drawable.Variants.Reset();
                CreateSvg();
            }
            StateHasChanged();
        }
    }
}
