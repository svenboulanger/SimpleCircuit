namespace SimpleCircuit.Components.Styles
{
    /// <summary>
    /// An aggregate style modifier.
    /// </summary>
    /// <param name="left">The left style modifier.</param>
    /// <param name="right">The right style modifier.</param>
    public class AggregateStyleModifier(IStyleModifier left, IStyleModifier right) : IStyleModifier
    {
        /// <inheritdoc />
        public IStyle Apply(IStyle parent) => right.Apply(left.Apply(parent));
    }
}
