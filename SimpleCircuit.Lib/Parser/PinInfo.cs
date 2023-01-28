using SimpleCircuit.Components;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents pin information.
    /// </summary>
    public readonly struct PinInfo
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
        /// Finds the pin in the graphical circuit.
        /// </summary>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="defaultIndex">If no pin is specified, this pin index is used. If negative, counts from the last pin.</param>
        /// <returns>The pin, or <c>null</c> if the pin could not be found.</returns>
        public IPin Find(IDiagnosticHandler diagnostics, int defaultIndex)
        {
            if (Component != null)
                return Find(Component.Component, diagnostics, defaultIndex);
            return null;
        }

        /// <summary>
        /// Finds the pin in the graphical circuit.
        /// </summary>
        /// <param name="drawable">The drawable to find the pin on.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="defaultIndex">If no pin is specified, this pin index is used. If negative, counts from the last pin.</param>
        /// <returns>The pin, or <c>null</c> if the pin could not be found.</returns>
        public IPin Find(IDrawable drawable, IDiagnosticHandler diagnostics, int defaultIndex)
        {
            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));

            // Find the pin
            if (Pin.Content.Length == 0)
            {
                if (drawable.Pins.Count == 0)
                {
                    diagnostics?.Post(Component.Name, ErrorCodes.DoesNotHavePins, Component.Fullname);
                    return null;
                }
                if (defaultIndex >= 0)
                    return drawable.Pins[defaultIndex];
                else
                    return drawable.Pins[drawable.Pins.Count + defaultIndex];
            }
            else
            {
                if (!drawable.Pins.TryGetValue(Pin.Content.ToString(), out var pin))
                {
                    diagnostics?.Post(Pin, ErrorCodes.CouldNotFindPin, Pin.Content, Component.Fullname);
                    return null;
                }
                return pin;
            }
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
