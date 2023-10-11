using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleCircuit.Components
{
    /// <summary>
    /// Default implementation for a component.
    /// </summary>
    public abstract class Drawable : IDrawable
    {
        /// <summary>
        /// Gets the variants of the drawable.
        /// </summary>
        public VariantSet Variants { get; } = new();

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets the type name of the instance.
        /// </summary>
        public virtual string Type => "Drawable";

        /// <summary>
        /// Gets the pins of the component.
        /// </summary>
        public PinCollection Pins { get; } = new();

        /// <inheritdoc />
        IPinCollection IDrawable.Pins => Pins;

        /// <inheritdoc />
        public virtual int Order => 0;

        /// <summary>
        /// Allows adding classes for the group node that groups all drawing elements.
        /// </summary>
        protected virtual IEnumerable<string> GroupClasses { get; }

        /// <inheritdoc />
        public IEnumerable<string> Properties => GetProperties(this);

        /// <inheritdoc />
        public Bounds Bounds { get; private set; }

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
        public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics)
            => SetProperty(this, propertyToken, value, diagnostics);

        /// <inheritdoc />
        public static bool SetProperty(IDrawable drawable, Token propertyToken, object value, IDiagnosticHandler diagnostics)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));

            // Find the property
            string property = propertyToken.Content.ToString().ToLower();

            // Search using reflection
            var info = drawable.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (info == null)
            {
                if (drawable is ILabeled labeled)
                {
                    // Let's expose some special properties if there are labels at play
                    int index;
                    if (value is Vector2 vector)
                    {
                        if (TryMatchIndexedProperty(property, "offset", out index))
                        {
                            labeled.Labels[index].Offset = vector;
                            return true;
                        }
                        if (TryMatchIndexedProperty(property, "expand", out index))
                        {
                            labeled.Labels[index].Expand = vector;
                            return true;
                        }
                    }
                    else if (value is double number && (number - Math.Round(number)).IsZero())
                    {
                        if (TryMatchIndexedProperty(property, "anchor", out index))
                        {
                            labeled.Labels[index].Location = ((int)Math.Round(number)).ToString();
                            return true;
                        }
                    }
                    if (value is string label)
                    {
                        if (TryMatchIndexedProperty(property, "label", out index))
                        {
                            labeled.Labels[index].Value = label;
                            return true;
                        }
                        if (TryMatchIndexedProperty(property, "anchor", out index))
                        {
                            labeled.Labels[index].Location = label;
                            return true;
                        }
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
            else
            {
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
            if (!property.StartsWith(property))
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
        public static IEnumerable<string> GetProperties(IDrawable drawable)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));
            foreach (var property in drawable.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite)
                    continue;
                yield return property.Name;
            }
        }

        /// <inheritdoc />
        public virtual bool Reset(IResetContext context)
        {
            foreach (var pin in Pins)
            {
                if (!pin.Reset(context))
                    return false;
            }
            return true;
        }

        /// <inheritdoc />
        public virtual PresenceResult Prepare(IPrepareContext context) => PresenceResult.Success;

        /// <summary>
        /// Creates a transform.
        /// </summary>
        /// <returns></returns>
        protected virtual Transform CreateTransform() => Transform.Identity;

        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="drawing">The drawing.</param>
        protected abstract void Draw(SvgDrawing drawing);

        /// <inheritdoc />
        public virtual void Render(SvgDrawing drawing)
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

            drawing.BeginGroup(go);

            // Transform all the elements inside the drawing method
            drawing.BeginTransform(CreateTransform());
            Draw(drawing);
            drawing.EndTransform();

            // Stop grouping elements
            Bounds = drawing.EndGroup();
        }

        /// <inheritdoc />
        public virtual bool DiscoverNodeRelationships(IRelationshipContext context) => true;

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
