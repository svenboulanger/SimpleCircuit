using SimpleCircuit.Components;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SimpleCircuit
{
    /// <summary>
    /// Describes options for parsing SimpleCircuit.
    /// </summary>
    public class Options
    {
        private readonly Dictionary<string, HashSet<string>> _includes = new(), _excludes = new();
        private readonly Dictionary<string, List<Action<IDrawable, IDiagnosticHandler>>> _properties = new();
        private double _spacingX = 20.0, _spacingY = 20.0;

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

        [Description("The spacing in X-direction between two unconnected diagrams.")]
        public double SpacingX
        {
            get => _spacingX;
            set
            {
                if (value != _spacingX)
                {
                    _spacingX = value;
                    SpacingXChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the event that is called when <see cref="SpacingX"/> changes.
        /// </summary>
        public event EventHandler<EventArgs> SpacingXChanged;

        [Description("The spacing in Y-direction between two unconnected diagrams.")]
        public double SpacingY
        {
            get => _spacingY;
            set
            {
                if (value != _spacingY)
                {
                    _spacingY = value;
                    SpacingYChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the event that is called when <see cref="SpacingY"/> changes.
        /// </summary>
        public event EventHandler<EventArgs> SpacingYChanged;

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

            var action = new Action<IDrawable, IDiagnosticHandler>((drawable, diagnostics) => SetProperty(diagnostics, drawable, propertyToken, value));
            list.Add(action);
        }

        /// <summary>
        /// Sets a property on a drawable.
        /// </summary>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="drawable">The drawable.</param>
        /// <param name="propertyToken">The property token.</param>
        /// <param name="value">The parsed value.</param>
        public bool SetProperty(IDiagnosticHandler diagnostics, IDrawable drawable, Token propertyToken, object value)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));

            string property = propertyToken.Content.ToString();
            var info = drawable.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (info == null)
            {
                if (drawable is ILabeled labeled && value is string label)
                {
                    var match = Regex.Match(property, @"^label(?<index>\w+)?$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (match.Groups["index"].Success)
                        {
                            int index = int.Parse(match.Groups["index"].Value);
                            if (index < 1 || index > labeled.Labels.Count)
                                diagnostics?.Post(propertyToken, ErrorCodes.TooManyLabels);
                            else
                                labeled.Labels[index - 1] = label;
                        }
                        else
                            labeled.Labels[0] = label;
                        return true;
                    }
                    else
                    {
                        diagnostics?.Post(propertyToken, ErrorCodes.CouldNotFindPropertyOrVariant, property, drawable.Name);
                        return false;
                    }
                }

                // No property or label, if the value is a boolean, we can instead add a variant instead
                if (value is bool b)
                {
                    // Just add the variant with the given property
                    if (b)
                        drawable.Variants.Add(property);
                    else
                        drawable.Variants.Remove(property);
                    return true;
                }
                else
                {
                    // No idea what to do with this
                    diagnostics?.Post(propertyToken, ErrorCodes.CouldNotFindPropertyOrVariant, property, drawable.Name);
                    return false;
                }
            }
            else
            {
                // We have the property
                var type = value.GetType();
                if (info.PropertyType == type)
                {
                    info.SetValue(drawable, value);
                    return true;
                }
                else
                {
                    // Some implicit type conversion here
                    if (info.PropertyType == typeof(int) && type == typeof(double))
                        info.SetValue(drawable, (int)(double)value);
                    else if (info.PropertyType == typeof(double) && type == typeof(int))
                        info.SetValue(drawable, (double)(int)value);
                    else
                    {
                        diagnostics?.Post(propertyToken, ErrorCodes.CouldNotFindPropertyOrVariant, property, drawable.Name);
                        return false;
                    }
                    return true;
                }
            }
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
            if (_properties.TryGetValue(key, out var list))
            {
                foreach (var action in list)
                    action(drawable, diagnostics);
            }
        }
    }
}
