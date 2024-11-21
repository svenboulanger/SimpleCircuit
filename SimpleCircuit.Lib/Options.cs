using SimpleCircuit.Components;
using SimpleCircuit.Components.Labeling;
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
        private readonly struct DefaultProperty(Token property, object value)
        {
            public Token Property { get; } = property;
            public object Value { get; } = value;
        }
        private readonly Dictionary<string, HashSet<string>> _includes = [], _excludes = [];
        private readonly Dictionary<string, List<DefaultProperty>> _properties = [];

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

        [Description("The default line spacing for text. The default is 1.5.")]
        public double LineSpacing { get; set; } = 1.5;

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

        [Description("The default scale for any amplifier created. The default is 1.")]
        public double Scale { get; set; } = 1.0;

        [Description("If true, removes any empty groups in the SVG output. The default is true.")] 
        public bool RemoveEmptyGroups { get; set; } = true;

        [Description("The radius of corners for subsequent components that support it. The default is 0.")]
        public double CornerRadius { get; set; } = 0.0;

        [Description("The default label margin to the edge of subsequent components. The default is 1.")]
        public double LabelMargin { get; set; } = 1.0;

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
                list = [];
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
        /// <param name="key">The drawable factory key.</param>
        /// <param name="drawable">The drawable.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        public void Apply(string key, IDrawable drawable, IDiagnosticHandler diagnostics)
        {
            // Default scale
            if (drawable is IScaledDrawable scaled)
                scaled.Scale = Scale;

            // Default standard
            if (drawable is IStandardizedDrawable standardized)
            {
                if (Standard == Standards.AREI && (standardized.Supported & Standards.AREI) == Standards.AREI)
                    standardized.Variants.Add(Arei);
                if (Standard == Standards.European && (standardized.Supported & Standards.European) == Standards.European)
                    standardized.Variants.Add(European);
                if (Standard == Standards.American && (standardized.Supported & Standards.American) == Standards.American)
                    standardized.Variants.Add(American);
            }

            // Rounded box
            if (drawable is IRoundedBox rb)
                rb.CornerRadius = CornerRadius;

            // Labels
            if (drawable is IBoxDrawable bl)
                bl.LabelMargin = LabelMargin;
            if (drawable is IEllipseDrawable el)
                el.LabelMargin = LabelMargin;
            drawable.Labels.FontSize = FontSize;
            drawable.Labels.LineSpacing = LineSpacing;

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
