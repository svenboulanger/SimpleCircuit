using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Drawing.Builders;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Default implementation for a component.
    /// </summary>
    public abstract class Drawable : IDrawable
    {
        private readonly static ConcurrentDictionary<Type, Dictionary<string, Func<IDrawable, Token, object, bool>>> _cacheSetters = new();

        public const string Dashed = "dashed";
        public const string Dotted = "dotted";

        /// <inheritdoc />
        public VariantSet Variants { get; } = [];

        /// <inheritdoc />
        public Labels Labels { get; } = new();

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public List<TextLocation> Sources { get; } = [];

        /// <summary>
        /// Gets the type name of the instance.
        /// </summary>
        public virtual string Type => "Drawable";

        /// <summary>
        /// Gets the pins of the component.
        /// </summary>
        public PinCollection Pins { get; } = [];

        /// <inheritdoc />
        IPinCollection IDrawable.Pins => Pins;

        /// <inheritdoc />
        public virtual int Order => 0;

        /// <summary>
        /// Allows adding classes for the group node that groups all drawing elements.
        /// </summary>
        protected virtual IEnumerable<string> GroupClasses { get; }

        /// <inheritdoc />
        public IEnumerable<string[]> Properties => GetProperties(this);

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

        /// <inheritdoc />
        public IStyleModifier Style { get; set; }

        /// <summary>
        /// Creates a new component.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        protected Drawable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        /// <inheritdoc />
        public virtual bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            => SetProperty(this, propertyToken, value, diagnostics);

        /// <summary>
        /// Creates a dictionary of setter methods for a given type using reflection.
        /// </summary>
        /// <param name="key">The type.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        /// <returns>Returns the dictionary.</returns>
        private static Dictionary<string, Func<IDrawable, Token, object, bool>> CreateCache(Type key, IDiagnosticHandler diagnostics)
        {
            // Extract the property names for the type
            var result = new Dictionary<string, Func<IDrawable, Token, object, bool>>(StringComparer.OrdinalIgnoreCase);
            var properties = key.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in properties)
            {
                names.Clear();
                if (!property.CanWrite)
                    continue;
                string description = null;
                foreach (object attribute in property.GetCustomAttributes(true))
                {
                    if (attribute is DescriptionAttribute descAttr)
                        description = descAttr.Description;
                    if (attribute is AliasAttribute aliasAttr)
                        names.Add(aliasAttr.Alias);
                }
                if (string.IsNullOrEmpty(description))
                    continue;
                names.Add(property.Name);

                // Add the information
                var p = property;
                var setter = new Func<IDrawable, Token, object, bool>((drawable, token, value) =>
                {
                    if (value is null)
                        return false;
                    var type = value.GetType();
                    if (p.PropertyType == type)
                    {
                        p.SetValue(drawable, value);
                        return true;
                    }
                    else
                    {
                        // Some implicit type conversion here
                        if (p.PropertyType == typeof(int) && type == typeof(double))
                            p.SetValue(drawable, (int)(double)value);
                        else if (p.PropertyType == typeof(double) && type == typeof(int))
                            p.SetValue(drawable, (double)(int)value);
                        else
                        {
                            diagnostics?.Post(token, ErrorCodes.CouldNotFindPropertyOrVariant, p, drawable.Name);
                            return false;
                        }
                        return true;
                    }
                });
                foreach (string name in names)
                    result[name] = setter;
            }
            return result;
        }

        /// <inheritdoc />
        public static bool SetProperty(IDrawable drawable, Token propertyToken, object value, IDiagnosticHandler diagnostics)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));

            // Find the property
            string property = propertyToken.Content.ToString().ToLower();

            // Gets the dictionary that describes the property setters
            var result = _cacheSetters.GetOrAdd(drawable.GetType(), key => CreateCache(key, diagnostics));

            // First check defined properties
            if (result.TryGetValue(property, out var setter))
                return setter(drawable, propertyToken, value);
            else
            {
                // Let's expose some special properties if there are labels at play
                int index;
                if (value is Vector2 vector)
                {
                    // 'offset#' can change the local offset of an individual label.
                    if (TryMatchIndexedProperty(property, "offset", out index))
                    {
                        drawable.Labels[index].Offset = vector;
                        return true;
                    }
                }
                else if (value is double number)
                {
                    switch (property)
                    {
                        case "transparency":
                        case "opacity":
                        case "alpha":
                            drawable.AppendStyle(new OpacityStyleModifier(number, number));
                            return true;

                        case "foregroundopacity":
                        case "fgo":
                            drawable.AppendStyle(new OpacityStyleModifier(number, null));
                            return true;

                        case "backgroundopacity":
                        case "bgo":
                            drawable.AppendStyle(new OpacityStyleModifier(null, number));
                            return true;

                        case "thickness":
                        case "t":
                        case "stroke-width":
                            drawable.AppendStyle(new StrokeWidthStyleModifier(number));
                            return true;

                        case "fontsize":
                            drawable.AppendStyle(new FontSizeStyleModifier(number));
                            return true;

                        default:
                            // 'anchor#' will designate the anchor for the label at the given index.
                            if ((number - Math.Round(number)).IsZero() &&
                                TryMatchIndexedProperty(property, "anchor", out index))
                            {
                                drawable.Labels[index].Anchor = ((int)Math.Round(number)).ToString();
                                return true;
                            }

                            // 'size#' will change the font size for the label at the given index.
                            if (TryMatchIndexedProperty(property, "size", out index) ||
                                TryMatchIndexedProperty(property, "fontsize", out index))
                            {
                                drawable.Labels[index].AppendStyle(new FontSizeStyleModifier(number));
                                return true;
                            }

                            // 'opacity#' will change the opacity of the label at the given index.
                            if (TryMatchIndexedProperty(property, "opacity", out index))
                            {
                                drawable.Labels[index].AppendStyle(new OpacityStyleModifier(number, number));
                                return true;
                            }

                            // 'justify#' will change the label justification at the given index.
                            if (TryMatchIndexedProperty(property, "justify", out index) ||
                                TryMatchIndexedProperty(property, "justification", out index))
                            {
                                drawable.Labels[index].AppendStyle(new JustificationStyleModifier(number));
                                return true;
                            }

                            // 'ls#' or 'linespacing#' will designate the line spacing for the label at the given index.
                            if (TryMatchIndexedProperty(property, "linespacing", out index) ||
                                TryMatchIndexedProperty(property, "ls", out index))
                            {
                                drawable.Labels[index].AppendStyle(new LineSpacingStyleModifier(number));
                                return true;
                            }
                            break;
                    }
                }
                if (value is string label)
                {
                    switch (property)
                    {
                        case "color":
                        case "foreground":
                        case "fg":
                            drawable.AppendStyle(new ColorStyleModifier(label, null));
                            return true;

                        case "background":
                        case "bg":
                            drawable.AppendStyle(new ColorStyleModifier(null, label));
                            return true;

                        case "fontfamily":
                        case "font":
                            drawable.AppendStyle(new FontFamilyStyleModifier(label));
                            return true;

                        default:
                            // 'label#' will change the label at the given index.
                            if (TryMatchIndexedProperty(property, "label", out index))
                            {
                                drawable.Labels[index].Value = label;
                                return true;
                            }

                            // 'anchor#' will change the anchor for the label at the given index.
                            if (TryMatchIndexedProperty(property, "anchor", out index))
                            {
                                drawable.Labels[index].Anchor = label;
                                return true;
                            }
                            break;
                    }
                }

                // No property or label, if the value is a boolean, we can add a variant instead
                if (value is bool b)
                {
                    switch (property)
                    {
                        case "bold":
                            drawable.AppendStyle(BoldTextStyleModifier.Default);
                            return true;

                        default:
                            // 'bold#' will change whether the text is bold for the label at the given index.
                            if (TryMatchIndexedProperty(property, "bold", out index))
                            {
                                drawable.Labels[index].AppendStyle(new BoldTextStyleModifier(b));
                                return true;
                            }

                            // Treat as a variant
                            if (b)
                                drawable.Variants.Add(property);
                            else
                                drawable.Variants.Remove(property);
                            return true;
                    }
                }
                else
                {
                    // No idea what to do with this
                    diagnostics?.Post(propertyToken, ErrorCodes.CouldNotFindPropertyOrVariant, property, drawable.Name);
                    return false;
                }
            }
        }

        /// <summary>
        /// Matches a property with an index.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="index">The index.</param>
        /// <returns>Returns <c>true</c> if the indexed property was matched successfully; otherwise, <c>false</c>.</returns>
        private static bool TryMatchIndexedProperty(string property, string prefix, out int index)
        {
            index = 0;

            // Try to read the prefix
            if (!property.StartsWith(prefix))
                return false;

            // Try to read the index
            for (int i = prefix.Length; i < property.Length; i++)
            {
                char c = property[i];
                if (c >= '0' && c <= '9')
                    index = (index * 10) + (c - '0');
                else
                    return false;
            }

            // Make index 1-based.
            if (index > 0)
                index--;
            return true;
        }

        /// <inheritdoc />
        public static IEnumerable<string[]> GetProperties(IDrawable drawable)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in drawable.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite)
                    continue;
                set.Clear();
                foreach (object attr in property.GetCustomAttributes(true))
                {
                    if (attr is AliasAttribute aliasAttr)
                        set.Add(aliasAttr.Alias);
                }
                set.Add(property.Name);
                yield return set.OrderBy(n => n).ToArray();
            }
        }

        /// <inheritdoc />
        public virtual PresenceResult Prepare(IPrepareContext context)
        {
            var result = PresenceResult.Success;
            
            // Give pins the opportunity
            foreach (var pin in Pins)
            {
                var r = pin.Prepare(context);
                if (r == PresenceResult.GiveUp)
                    result = PresenceResult.GiveUp;
                else if (result == PresenceResult.Success)
                    result = r;
            }

            switch (context.Mode)
            {
                case PreparationMode.Sizes:
                    FormatLabels(context);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Formats the labels of the drawable.
        /// </summary>
        /// <param name="context">The prepare context.</param>
        protected virtual void FormatLabels(IPrepareContext context)
            => Labels.Format(context.TextFormatter, context.Style.Modify(Style));

        /// <summary>
        /// Creates a transform.
        /// </summary>
        /// <returns></returns>
        protected virtual Transform CreateTransform() => Transform.Identity;

        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected abstract void Draw(IGraphicsBuilder builder);

        /// <inheritdoc />
        public virtual void Render(IGraphicsBuilder builder)
        {
            // Group all elements
            var classes = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(Type))
                classes.Add(Type.ToLower());
            foreach (string name in Variants)
                classes.Add(name.ToLower());
            if (GroupClasses != null)
            {
                foreach (string name in GroupClasses)
                    classes.Add(name);
            }

            builder.BeginBounds();
            builder.BeginGroup(Name, classes);

            // Transform all the elements inside the drawing method
            builder.BeginTransform(CreateTransform());
            Draw(builder);
            builder.EndTransform();

            // Stop grouping elements
            builder.EndGroup();
            builder.EndBounds(out var bounds);
            Bounds = bounds;
        }

        /// <inheritdoc />
        public abstract void Register(IRegisterContext context);

        /// <inheritdoc />
        public abstract void Update(IUpdateContext context);

        /// <summary>
        /// Sets the offset of the specified pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOffset(int index, Vector2 offset)
        {
            if (index < Pins.Count && index >= 0)
                SetPinOffset(Pins[index], offset);
            else if (index < 0 && index >= -Pins.Count)
                SetPinOffset(Pins[Pins.Count + index], offset);
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Sets the offset of the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOffset(IPin pin, Vector2 offset)
        {
            if (pin is FixedOrientedPin fop)
                fop.Offset = offset;
            else if (pin is FixedPin fp)
                fp.Offset = offset;
            else
                throw new ArgumentException("Wanted to set offset of an invalid pin");
        }

        /// <summary>
        /// Sets the orientation of the specified pin.
        /// </summary>
        /// <param name="index">The pin index.</param>
        /// <param name="orientation">The orientation.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOrientation(int index, Vector2 orientation)
        {
            if (index < Pins.Count && index >= 0)
                SetPinOffset(Pins[index], orientation);
            else if (index < 0 && index >= Pins.Count)
                SetPinOffset(Pins[Pins.Count + index], orientation);
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Sets the orientation of the specified pin.
        /// </summary>
        /// <param name="pin">The pin.</param>
        /// <param name="orientation">The orientation.</param>
        /// <exception cref="ArgumentException">Thrown if the pin is not a valid pin.</exception>
        protected void SetPinOrientation(IPin pin, Vector2 orientation)
        {
            if (pin is FixedOrientedPin fop)
                fop.RelativeOrientation = orientation;
            else
                throw new ArgumentException("Wanted to set orientation of an invalid pin");
        }

        /// <summary>
        /// Converts the drawable to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Name;
    }
}
