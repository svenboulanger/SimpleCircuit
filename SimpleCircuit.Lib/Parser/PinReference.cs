using SimpleCircuit.Components;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using System;

namespace SimpleCircuit.Parser
{
    /// <summary>
    /// Pin information.
    /// </summary>
    /// <param name="drawable">The component.</param>
    /// <param name="name">The pin name.</param>
    /// <param name="source">The source.</param>
    public class PinReference(IDrawable drawable, string name, TextLocation source)
    {
        private IPin _pin = null;

        /// <summary>
        /// Gets the name of the component the pin belongs to.
        /// </summary>
        public IDrawable Drawable { get; } = drawable ?? throw new ArgumentNullException(nameof(drawable));

        /// <summary>
        /// Gets the name of the pin of the component.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the source of the reference.
        /// </summary>
        public TextLocation Source => source;

        /// <summary>
        /// Gets the pin.
        /// </summary>
        public IPin Pin => _pin;

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
                if (Drawable == null)
                    return null;
                if (Name is null)
                {
                    // Get the pin by its index
                    if (Drawable.Pins != null)
                    {
                        if (defaultIndex >= 0)
                            _pin = Drawable.Pins[defaultIndex];
                        else
                            _pin = Drawable.Pins[drawable.Pins.Count + defaultIndex];
                    }
                    if (_pin == null)
                        diagnostics?.Post(default(TextLocation), ErrorCodes.DoesNotHavePins, drawable.Name);
                }
                else
                {
                    // Get the pin by its name
                    _pin = Drawable.Pins[Name];
                    if (_pin == null)
                        diagnostics?.Post(Source, ErrorCodes.CouldNotFindPin, Name, drawable.Name);
                }
            }
            return _pin;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Name.Length > 0)
                return $"{Drawable.Name}[{Name}]";
            else
                return Drawable.Name;
        }
    }
}
