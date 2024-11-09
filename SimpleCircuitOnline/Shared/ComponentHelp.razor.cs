using Microsoft.AspNetCore.Components;
using SimpleCircuit;
using SimpleCircuit.Components;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Labeling;
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
        private List<(PropertyInfo, string[], string)> _properties;
        private IDrawable _drawable = null;
        private readonly HashSet<string> _initialVariants = new();

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public DrawableMetadata Metadata { get; set; }

        [Parameter]
        public IDrawableFactory Factory { get; set; }

        /// <summary>
        /// Gets the properties of the component.
        /// </summary>
        protected List<(PropertyInfo, string[], string)> Properties
        {
            get
            {
                if (_properties == null && _drawable != null)
                {
                    var names = new List<string>();
                    _properties = [];
                    foreach (var p in _drawable.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (!p.CanRead || !p.CanWrite)
                            continue;
                        if (p.PropertyType != typeof(double)
                            && p.PropertyType != typeof(bool)
                            && p.PropertyType != typeof(string)
                            && p.PropertyType != typeof(int))
                            continue;
                        names.Clear();
                        string description = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
                        names.Add(p.Name);
                        foreach (var attribute in p.GetCustomAttributes<AliasAttribute>())
                            names.Add(attribute.Alias);
                        _properties.Add((p, names.ToArray(), description));
                    }
                }
                return _properties;
            }
        }

        /// <summary>
        /// Lists all the different variant modifiers used for the preview.
        /// </summary>
        protected IEnumerable<string> CurrentVariantModifiers
        {
            get
            {
                if (_drawable is null)
                    yield break;
                foreach (var variant in _drawable.Variants)
                {
                    if (_initialVariants.Contains(variant))
                        continue;
                    yield return variant;
                }
                foreach (var variant in _initialVariants)
                {
                    if (!_drawable.Variants.Contains(variant))
                        yield return $"-{variant}";
                }
            }
        }

        /// <summary>
        /// Lists all the labels specified on the drawable for the preview.
        /// </summary>
        protected IEnumerable<string> CurrentLabels
        {
            get
            {
                if (_drawable is not ILabeled labeled)
                    yield break;
                foreach (var lbl in labeled.Labels)
                    yield return lbl.Value;
            }
        }

        /// <summary>
        /// Create a property type name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name.</returns>
        protected static string GetTypeName(Type type)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(double))
                return "real";
            if (type == typeof(string))
                return "string";
            if (type == typeof(bool))
                return "bool";
            return "?";
        }

        /// <inheritdoc />
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (parameters.TryGetValue<IDrawableFactory>(nameof(Factory), out var factory) &&
                parameters.TryGetValue<DrawableMetadata>(nameof(Metadata), out var metadata))
            {
                _properties = null;

                if (factory != null && metadata != null)
                {
                    _drawable = factory.Create(metadata.Key, metadata.Key, new Options(), null);
                    if (_drawable is ILabeled labeled && string.IsNullOrWhiteSpace(labeled.Labels[0]?.Value))
                        labeled.Labels[0].Value = "label";
                    foreach (var variant in _drawable.Variants)
                        _initialVariants.Add(variant);
                    CreateSvg();
                }
            }
            await base.SetParametersAsync(parameters);
        }

        private void CreateSvg()
        {
            if (Metadata == null || _drawable == null)
            {
                _svg = default;
                return;
            }

            var drawing = new SvgDrawing(null, _textMeasurer);
            _drawable.Reset(null);
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
    }
}
