using SimpleCircuit.Drawing.Styles;
using System;
using System.Linq;

namespace SimpleCircuit.Components.Labeling;

/// <summary>
/// A list of custom label anchor points.
/// </summary>
public class CustomLabelAnchorPoints : LabelAnchorPoints<IDrawable>
{
    private readonly LabelAnchorPoint[] _points;

    /// <summary>
    /// Gets or sets the label anchor point at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The label anchor point.</returns>
    public LabelAnchorPoint this[int index]
    {
        get => _points[index];
        set => _points[index] = value;
    }

    /// <inheritdoc />
    public override int Count => _points.Length;

    /// <summary>
    /// Creates a new <see cref="CustomLabelAnchorPoints"/>.
    /// </summary>
    /// <param name="points">The points.</param>
    public CustomLabelAnchorPoints(params LabelAnchorPoint[] points)
    {
        _points = points ?? throw new ArgumentNullException(nameof(points));
    }

    /// <summary>
    /// Creates a new <see cref="CustomLabelAnchorPoints"/>.
    /// </summary>
    /// <param name="count">The number of label anchors.</param>
    public CustomLabelAnchorPoints(int count)
    {
        _points = new LabelAnchorPoint[count];
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
        index = -1;
        return false;
    }

    /// <inheritdoc />
    public override LabelAnchorPoint GetAnchorPoint(IDrawable subject, int index, IStyle style)
    {
        index %= Count;
        if (index < 0)
            index += Count;
        return _points[index];
    }

    /// <summary>
    /// Tries to calculate the label anchor point just based on index.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <returns>Returns <c>true</c> if the label anchor point was found; otherwise, <c>false</c>.</returns>
    protected bool TryCalculate(string name, out LabelAnchorPoint value)
    {
        if (TryGetAnchorIndex(name, out int index))
        {
            value = _points[index];
            return true;
        }
        value = default;
        return false;
    }
}
