namespace SimpleCircuit.Components
{
    /// <summary>
    /// Describes an instance that can translate, rotate and scale in one of the two dimensions.
    /// </summary>
    /// <seealso cref="ITranslating" />
    /// <seealso cref="IRotating" />
    /// <seealso cref="IScaling" />
    public interface I2DTransforming : ITranslating, IRotating, IScaling
    {
    }
}
