using SimpleCircuit.Components;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents variant information.
    /// </summary>
    public struct VariantInfo
    {
        /// <summary>
        /// Determines whether the variant should be included. If <c>false</c>, the
        /// variant should be removed.
        /// </summary>
        public bool Include { get; }

        /// <summary>
        /// The name of the variant.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new <see cref="VariantInfo"/>.
        /// </summary>
        /// <param name="include">If <c>true</c>, the variant should be added; otherwise, the variant should be removed.</param>
        /// <param name="name">The variant name.</param>
        public VariantInfo(bool include, string name)
        {
            Include = include;
            Name = name;
        }
    }

    /// <summary>
    /// Represents component information.
    /// </summary>
    public class ComponentInfo
    {
        private IDrawable _component = null;

        /// <summary>
        /// Gets the token that describes the component name.
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// Gets the full name of the component.
        /// </summary>
        public string Fullname { get; }

        /// <summary>
        /// Gets the label of the component.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets the variants of the component.
        /// </summary>
        public List<VariantInfo> Variants { get; } = new();

        /// <summary>
        /// Creates a new component information.
        /// </summary>
        /// <param name="name">The token name.</param>
        /// <param name="fullname">The full name of the component.</param>
        public ComponentInfo(Token name, string fullname)
        {
            Name = name;
            Fullname = fullname;
        }

        /// <summary>
        /// Gets or creates a component.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returns the component, or <c>null</c> if no component could be created.</returns>
        public IDrawable GetOrCreate(ParsingContext context)
        {
            if (_component == null)
            {
                _component = context.GetOrCreate(Fullname, context.Options);
                if (_component == null)
                {
                    context.Diagnostics?.Post(Name, ErrorCodes.CouldNotRecognizeOrCreateComponent, Fullname);
                    return null;
                }

                // Handle the label
                if (Label != null && _component is ILabeled labeled)
                    labeled.Label = Label;

                // Handle variants
                foreach (var variant in Variants)
                {
                    if (variant.Include)
                        _component.Variants.Add(variant.Name);
                    else
                        _component.Variants.Remove(variant.Name);
                }
            }
            return _component;
        }
    }

    /// <summary>
    /// Represents pin information.
    /// </summary>
    public struct PinInfo
    {
        /// <summary>
        /// Gets the name of the component the pin belongs to.
        /// </summary>
        public ComponentInfo Component { get; }

        /// <summary>
        /// Gets the name of the pin of the component.
        /// </summary>
        public Token Pin { get; }

        /// <summary>
        /// Creates a new pin info.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="pin"></param>
        public PinInfo(ComponentInfo component, Token pin)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Pin = pin;
        }

        /// <summary>
        /// Gets or creates a component and pin for the given info.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="defaultPin">The default pin index if no pin name is specified. If negative, the index counts backwards from the last.</param>
        /// <returns>The pin, or <c>null</c> if no pin could be retrieved.</returns>
        public IPin GetOrCreate(ParsingContext context, int defaultPin)
        {
            if (Component == null)
                return null;

            // Get the component
            var component = Component.GetOrCreate(context);
            if (component == null)
                return null;

            // Get the pin
            if (Pin.Content.Length > 0)
            {
                string pinName = Pin.Content.ToString();
                if (!component.Pins.TryGetValue(pinName, out var pin))
                {
                    context.Diagnostics?.Post(Pin, ErrorCodes.CannotFindPin, pinName, Component.Fullname);
                    return null;
                }
                return pin;
            }
            else if (component.Pins.Count == 0)
            {
                context.Diagnostics?.Post(Component.Name, ErrorCodes.DoesNotHavePins, Component.Fullname);
                return null;
            }
            else
            {
                if (defaultPin >= 0)
                    return component.Pins[defaultPin];
                else
                    return component.Pins[component.Pins.Count + defaultPin];
            }
        }
    }
}
