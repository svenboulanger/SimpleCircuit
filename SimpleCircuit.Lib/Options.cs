using SimpleCircuit.Components;
using System;
using System.Collections.Generic;

namespace SimpleCircuit
{
    /// <summary>
    /// Describes options for parsing SimpleCircuit.
    /// </summary>
    public class Options
    {
        private readonly Dictionary<string, HashSet<string>> _includes = new(), _excludes = new();

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

        [Description("If true, removes any empty groups in the SVG output.")] 
        public bool RemoveEmptyGroups { get; set; } = true;

        [Description("The spacing between lines of text.")]
        public double LineSpacing { get; set; } = 1.0;

        [Description("If true, wires will draw small arcs indicating jumping over another wire.")]
        public bool JumpOverWires { get; set; } = false;

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
        public void ClearVariants()
        {
            _includes.Clear();
            _excludes.Clear();
        }

        /// <summary>
        /// Applies the variants for the given drawable and key.
        /// </summary>
        /// <param name="drawable">The drawable.</param>
        public void ApplyVariants(string key, IDrawable drawable)
        {
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
        }
    }
}
