using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Appearance;
using SimpleCircuit.Components.Builders;
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
        public AppearanceOptions Appearance { get; } = new();

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
                foreach (var attribute in property.GetCustomAttributes(true))
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
                    if (TryMatchIndexedProperty(property, "offset", out index))
                    {
                        drawable.Labels[index].Offset = vector;
                        return true;
                    }
                    if (TryMatchIndexedProperty(property, "expand", out index))
                    {
                        drawable.Labels[index].Expand = vector / vector.Length;
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
                            drawable.Appearance.Opacity = number;
                            drawable.Appearance.BackgroundOpacity = number;
                            return true;

                        case "foregroundopacity":
                        case "fgo":
                            drawable.Appearance.Opacity = number;
                            return true;

                        case "backgroundopacity":
                        case "bgo":
                            drawable.Appearance.Opacity = number;
                            return true;

                        case "thickness":
                        case "t":
                            drawable.Appearance.LineThickness = number;
                            return true;

                        default:
                            if ((number - Math.Round(number)).IsZero())
                            {
                                // Integer-only
                                if (TryMatchIndexedProperty(property, "anchor", out index))
                                {
                                    drawable.Labels[index].Location = ((int)Math.Round(number)).ToString();
                                    return true;
                                }
                            }
                            if (TryMatchIndexedProperty(property, "size", out index))
                            {
                                drawable.Labels[index].Size = number;
                                return true;
                            }
                            if (TryMatchIndexedProperty(property, "linespacing", out index) ||
                                TryMatchIndexedProperty(property, "ls", out index))
                            {
                                drawable.Labels[index].LineSpacing = number;
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
                            drawable.Appearance.Color = label;
                            return true;

                        case "background":
                        case "bg":
                            drawable.Appearance.Background = label;
                            return true;

                        default:
                            if (TryMatchIndexedProperty(property, "label", out index))
                            {
                                drawable.Labels[index].Value = label;
                                return true;
                            }
                            if (TryMatchIndexedProperty(property, "anchor", out index))
                            {
                                drawable.Labels[index].Location = label;
                                return true;
                            }
                            break;
                    }
                }

                // No property or label, if the value is a boolean, we can add a variant instead
                if (value is bool b)
                {
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
                case PreparationMode.Reset:
                    Appearance.Parent = context.GlobalAppearance;
                    break;

                case PreparationMode.Sizes:
                    Labels.Format(context, Appearance);
                    break;
            }
            return result;
        }

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
            var go = new GraphicOptions() { Id = Name };
            if (!string.IsNullOrWhiteSpace(Type))
                go.Classes.Add(Type.ToLower());
            foreach (string name in Variants)
                go.Classes.Add(name.ToLower());
            if (GroupClasses != null)
            {
                foreach (string name in GroupClasses)
                    go.Classes.Add(name);
            }

            builder.BeginBounds();
            builder.BeginGroup(go);

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
        /// Converts the drawable to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString() => Name;
    }
}
