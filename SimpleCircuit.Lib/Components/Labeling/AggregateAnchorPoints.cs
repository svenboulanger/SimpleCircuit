using SimpleCircuit.Drawing.Styles;
using System.Linq;

namespace SimpleCircuit.Components.Labeling;

/// <summary>
/// The aggregate of two lists of label anchor points.
/// </summary>
/// <remarks>
/// Creates a new aggregate anchor points list.
/// </remarks>
/// <param name="a">The first list.</param>
/// <param name="b">The second list.</param>
public class AggregateAnchorPoints<T>(ILabelAnchorPoints<T> a, ILabelAnchorPoints<T> b) : LabelAnchorPoints<T> where T : IDrawable
{
    private readonly ILabelAnchorPoints<T> _a = a, _b = b;

    /// <inheritdoc />
    public override int Count => _a.Count + _b.Count;

    /// <inheritdoc />
    public override LabelAnchorPoint GetAnchorPoint(T subject, int index, IStyle style)
    {
        index %= Count;
        if (index < 0)
            index += Count;

        if (index < _a.Count)
            return _a.GetAnchorPoint(subject, index, style);
        else
            return _b.GetAnchorPoint(subject, index - _a.Count, style);
    }

    /// <inheritdoc />
    public override bool TryGetAnchorIndex(string name, out int index)
    {
        if (name.All(char.IsDigit))
        {
            index = int.Parse(name);
            index %= Count;
            if (index < 0)
                index += Count;
            return true;
        }
        else if (_a.TryGetAnchorIndex(name, out index))
            return true;
        else if (_b.TryGetAnchorIndex(name, out index))
            return true;
        index = -1;
        return false;
    }
}
