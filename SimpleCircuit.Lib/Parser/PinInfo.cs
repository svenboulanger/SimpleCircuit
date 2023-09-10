using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Represents pin information.
    /// </summary>
    public class PinInfo
    {
        private IPin _pin;

        /// <summary>
        /// Gets the name of the component the pin belongs to.
        /// </summary>
        public ComponentInfo Component { get; }

        /// <summary>
        /// Gets the name of the pin of the component.
        /// </summary>
        public Token Name { get; }

        /// <summary>
        /// Gets the pin.
        /// </summary>
        public IPin Pin => _pin;

        /// <summary>
        /// Creates a new pin info.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="pin"></param>
        public PinInfo(ComponentInfo component, Token pin)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Name = pin;
            _pin = null;
        }

        /// <summary>
        /// Gets or creates the pin.
        /// </summary>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="defaultIndex">The default index in case the pin is not named.</param>
        /// <returns>The pin; or <c>null</c> if it couldn't be found.</returns>
        public IPin GetOrCreate(IDiagnosticHandler diagnostics, int defaultIndex)
        {
            if (_pin == null)
            {
                if (Component == null)
                    return null;

                var drawable = Component.Component;
                if (Name.Content.Length == 0)
                {
                    // Get the pin by its index
                    if (drawable.Pins != null)
                    {
                        if (defaultIndex >= 0)
                            _pin = drawable.Pins[defaultIndex];
                        else
                            _pin = drawable.Pins[drawable.Pins.Count + defaultIndex];
                    }
                    if (_pin == null)
                        diagnostics?.Post(Component.Source, ErrorCodes.DoesNotHavePins, Component.Fullname);
                }
                else
                {
                    // Get the pin by its name
                    _pin = drawable.Pins[Name.Content.ToString()];
                    if (_pin == null)
                        diagnostics?.Post(Name, ErrorCodes.CouldNotFindPin, Name.Content, Component.Fullname);
                }
            }
            return _pin;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Name.Content.Length > 0)
                return $"{Component.Fullname}[{Name.Content}]";
            else
                return Component.Fullname;
        }
    }
}
