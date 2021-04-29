using SimpleCircuit.Functions;

namespace SimpleCircuit.Components
{
    public partial class BlackBox
    {
        private class Pin : IPin, ITranslating, IRotating
        {
            /// <inheritdoc/>
            public string Name { get; }

            /// <inheritdoc/>
            public string Description => Name;

            /// <inheritdoc/>
            public IComponent Owner { get; }

            /// <summary>
            /// Gets the unknown that specifies the distance to the previous pin.
            /// </summary>
            public Unknown Length { get; }

            /// <summary>
            /// Gets or sets the next pin.
            /// </summary>
            public Pin Previous { get; set; }

            /// <summary>
            /// Gets or sets the X-coordinate.
            /// </summary>
            public Function X { get; set; }

            /// <summary>
            /// Gets or sets the Y-coordinate.
            /// </summary>
            public Function Y { get; set; }

            /// <inheritdoc/>
            public Function NormalX { get; }

            /// <inheritdoc/>
            public Function NormalY { get; }

            /// <inheritdoc/>
            public Function Angle => new NormalAtan2(NormalY, NormalX);

            /// <summary>
            /// Creates a new pin.
            /// </summary>
            /// <param name="name"></param>
            public Pin(string name, IComponent owner, Vector2 normal)
            {
                NormalX = normal.X;
                NormalY = normal.Y;
                Name = name;
                Owner = owner;
                Length = new Unknown($"{Owner}.{name}.l", UnknownTypes.Length);
            }
        }
    }
}
