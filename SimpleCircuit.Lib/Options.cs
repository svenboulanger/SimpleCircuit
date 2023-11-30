using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuit
{
    /// <summary>
    /// Describes options for parsing SimpleCircuit.
    /// </summary>
    public class Options
    {
        private readonly struct DefaultProperty
        {
            public Token Property { get; }
            public object Value { get; }
            public DefaultProperty(Token property, object value)
            {
                Property = property;
                Value = value;
            }
        }
        private readonly Dictionary<string, HashSet<string>> _includes = new(), _excludes = new();
        private readonly Dictionary<string, List<DefaultProperty>> _properties = new();

        /// <summary>
        /// The identifier for AREI style components.
        /// </summary>
        public const string Arei = "arei";

        /// <summary>
        /// The identifier for American style components.
        /// </summary>
        public const string American = "american";

        /// <summary>
        /// The identifier for European style components.
        /// </summary>
        public const string European = "euro";

        [Description("The font family for text. The default is 'Arial'.")]
        public string FontFamily { get; set; } = "Arial";

        [Description("The default font size for text. The default is 4.")]
        public double FontSize { get; set; } = 4.0;

        /// <summary>
        /// Gets the current style.
        /// </summary>
        public Standards Standard { get; private set; }

        [Description("If true, use native symbols.")]
        public bool Native { get => Standard == Standards.Native; set => Standard = Standards.Native; }

        [Description("If true, use ANSI style symbols.")]
        public bool AmericanStyle { get => Standard == Standards.American; set => Standard = Standards.American; }

        [Description("If true, use IEC style symbols.")]
        public bool EuropeanStyle { get => Standard == Standards.European; set => Standard = Standards.European; }

        [Description("If true, some components will use symbols for electrical installations in Belgium (Algemeen Reglement op Elektrische Installaties).")]
        public bool AREI { get => Standard == Standards.AREI; set => Standard = Standards.AREI; }

        [Description("The minimum wire length used for wires when no minimum or fixed length is specified.")]
        public double MinimumWireLength { get; set; } = 10.0;

        [Description("Horizontal inter-pin spacing for black-box components.")]
        public double HorizontalPinSpacing { get; set; } = 30.0;

        [Description("Vertical inter-pin spacing for black-box components.")]
        public double VerticalPinSpacing { get; set; } = 20.0;

        [Description("The default scale for any amplifier created. The default is 1.")]
        public double Scale { get; set; } = 1.0;

        [Description("The margin along the border of the drawing.")]
        public double Margin { get; set; } = 1.0;

        [Description("If true, removes any empty groups in the SVG output. The default is true.")] 
        public bool RemoveEmptyGroups { get; set; } = true;

        [Description("The spacing between lines of text. The default is 1.")]
        public double LineSpacing { get; set; } = 1.0;

        [Description("If true, wires will draw small arcs indicating jumping over another wire. The default is false.")]
        public bool JumpOverWires { get; set; } = false;

        [Description("If non-zero, subsequent wires will be rounded when cornering. The default is 0.")]
        public double RoundWires { get; set; } = 0.0;

        [Description("The radius of corners for subsequent components that support it. The default is 0.")]
        public double CornerRadius { get; set; } = 0.0;

        [Description("The default label margin to the edge of subsequent components. The default is 1.")]
        public double LabelMargin { get; set; } = 1.0;

        [Description("If non-zero, subsequent annotation boxes will be rounded.")]
        public double AnnotationRadius { get; set; } = 0.0;

        [Description("If \"true\", subsequent annotation boxes will be polygons.")]
        public bool AnnotationPoly { get; set; } = false;

        [Description("The default margin for annotation boxes to components.")]
        public double AnnotationMargin { get; set; } = 5.0;

        [Description("The default margin for annotation boxes to wires.")]
        public double AnnotationWireMargin { get; set; } = 5.0;

        [Description("The default margin for annotation boxes at wire ends when using polygon annotation boxes.")]
        public double AnnotationWireMarginEnds { get; set; } = 2.5;

        [Description("The spacing in X-direction between two unconnected circuit diagrams. The default is 40.")]
        public double SpacingX { get; set; } = 40;

        [Description("The spacing in Y-direction between two unconnected circuit diagrams. The default is 40.")]
        public double SpacingY { get; set; } = 40;

        [Description("If true, the graphical bounds are rendered on top of each component. The default is false.")]
        public bool RenderBounds { get; set; }

        /// <summary>
        /// Adds a default property value for any drawable of the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="propertyToken">The property.</param>
        /// <param name="value">The value.</param>
        public void AddDefaultProperty(string key, Token propertyToken, object value)
        {
            if (!_properties.TryGetValue(key, out var list))
            {
                list = new();
                _properties.Add(key, list);
            }
            list.Add(new DefaultProperty(propertyToken, value));
        }

        /// <summary>
        /// Adds a variant that needs to be included for the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="variant">The variant.</param>
        public void AddInclude(string key, string variant)
        {
            // If it was excluded previously, remove it from there
            if (_excludes.TryGetValue(key, out var set))
                set.Remove(variant);
            if (!_includes.TryGetValue(key, out set))
            {
                set = new(StringComparer.OrdinalIgnoreCase);
                _includes.Add(key, set);
            }
            set.Add(variant);
        }

        /// <summary>
        /// Adds a variant that needs to be removed for the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="variant">The variant.</param>
        public void AddExclude(string key, string variant)
        {
            if (_includes.TryGetValue(key, out var set))
                set.Remove(variant);
            if (!_excludes.TryGetValue(key, out set))
            {
                set = new(StringComparer.OrdinalIgnoreCase);
                _excludes.Add(key, set);
            }
            set.Add(variant);
        }

        /// <summary>
        /// Clears the variants from the options.
        /// </summary>
        public void ClearPropertiesAndVariants()
        {
            _includes.Clear();
            _excludes.Clear();
            _properties.Clear();
        }

        /// <summary>
        /// Applies the variants for the given drawable and key.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public void Apply(string key, IDrawable drawable, IDiagnosticHandler diagnostics)
        {
            // Handle default variants
            if (_includes.TryGetValue(key, out var set))
            {
                foreach (string variant in set)
                    drawable.Variants.Add(variant);
            }
            if (_excludes.TryGetValue(key, out set))
            {
                foreach (string variant in set)
                    drawable.Variants.Remove(variant);
            }

            // Handle default properties
            if (_properties.TryGetValue(key, out var list))
            {
                foreach (var defaultProperty in list)
                    drawable.SetProperty(defaultProperty.Property, defaultProperty.Value, diagnostics);
            }
        }

        /// <summary>
        /// Applies the options to the given graphical circuit.
        /// </summary>
        /// <param name="circuit">The circuit.</param>
        public void Apply(GraphicalCircuit circuit)
        {
            circuit.SpacingX = SpacingX;
            circuit.SpacingY = SpacingY;
            circuit.RenderBounds = RenderBounds;
        }
    }
}
