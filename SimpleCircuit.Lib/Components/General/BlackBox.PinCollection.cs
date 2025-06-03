using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Circuits.Spans;
using SimpleCircuit.Components.Builders;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Styles;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components
{
    public partial class BlackBox
    {
        /// <summary>
        /// A pin-collection that makes pins as they are requested.
        /// </summary>
        protected class PinCollection : IPinCollection
        {
            private readonly Instance _parent;
            private readonly Dictionary<string, IPin> _pinsByName = [];
            private readonly List<Span> _spansByIndex = [];
            private readonly List<IPin> _pinsByIndex = [];

            /// <summary>
            /// The weight for minimum distances.
            /// </summary>
            public const double MinimumWeight = 100.0;

            /// <summary>
            /// Gets the name of the node for the right side of the black box.
            /// </summary>
            public string Right { get; }

            /// <summary>
            /// Gets the name of the node for the bottom side of the black box.
            /// </summary>
            public string Bottom { get; }

            /// <summary>
            /// Gets or sets the corner radius (used for margins).
            /// </summary>
            public double CornerRadius { get; set; } = 0.0;

            /// <inheritdoc/>
            public IPin this[string name]
            {
                get
                {
                    if (!_pinsByName.TryGetValue(name, out var pin))
                    {
                        pin = new LoosePin(name, name, _parent);
                        _pinsByName.Add(name, pin);
                        _pinsByIndex.Add(pin);
                    }
                    return pin;
                }
            }

            /// <inheritdoc/>
            public IPin this[int index] => _pinsByIndex[index];

            /// <inheritdoc/>
            public int Count => _pinsByIndex.Count;

            /// <summary>
            /// Creates a new <see cref="PinCollection"/>.
            /// </summary>
            /// <param name="parent">the parent component.</param>
            public PinCollection(Instance parent)
            {
                _parent = parent;
                Right = $"{parent.Name}.right";
                Bottom = $"{parent.Name}.bottom";

                // This pin is not used, but avoids breaking things...
                var pin = new FixedPin("x", "x", _parent, new());
                _pinsByIndex.Add(pin);
            }

            /// <inheritdoc/>
            public IEnumerable<string> NamesOf(IPin pin)
            {
                yield return pin.Name;
            }

            /// <inheritdoc />
            public void Render(IGraphicsBuilder builder)
            {
                var style = builder.Style.ModifyDashedDotted(_parent);
                for (int i = 0; i < _pinsByIndex.Count; i++)
                {
                    if (_pinsByIndex[i] is not LoosePin pin || _spansByIndex[i] is null)
                        continue;
                    if (PointsLeft(pin))
                        builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleLeft + new Vector2(_parent.Margin.Left, 0), TextOrientation.Transformed);
                    if (PointsRight(pin))
                        builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleRight - new Vector2(_parent.Margin.Right, 0), TextOrientation.Transformed);
                    if (PointsUp(pin))
                        builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleLeft.Perpendicular + new Vector2(0, _parent.Margin.Top), new(new(0, 1), TextOrientationTypes.Transformed));
                    if (PointsDown(pin))
                        builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleRight.Perpendicular - new Vector2(0, _parent.Margin.Bottom), new(new(0, 1), TextOrientationTypes.Transformed));
                }
            }

            private string TransformPinName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;
                if (name[0] == '_')
                    return null;
                return name;
            }

            /// <inheritdoc />
            public void Register(IRegisterContext context)
            {
                // Figure out the edge margins
                double marginLeft = CornerRadius, marginTop = CornerRadius, marginRight = CornerRadius, marginBottom = CornerRadius;
                for (int i = 0; i < _pinsByIndex.Count; i++)
                {
                    if (_pinsByIndex[i] is not LoosePin pin)
                        continue;
                    if (PointsLeft(pin))
                        marginLeft = Math.Max(marginLeft, _spansByIndex[i].Bounds.Bounds.Width + _parent.Margin.Horizontal);
                    if (PointsRight(pin))
                        marginRight = Math.Max(marginRight, _spansByIndex[i].Bounds.Bounds.Width + _parent.Margin.Horizontal);
                    if (PointsUp(pin))
                        marginTop = Math.Max(marginTop, _spansByIndex[i].Bounds.Bounds.Width + _parent.Margin.Vertical);
                    if (PointsDown(pin))
                        marginBottom = Math.Max(marginBottom, _spansByIndex[i].Bounds.Bounds.Width + _parent.Margin.Vertical);
                }

                // Place north pins
                int lastLeftPin = -1, lastTopPin = -1, lastBottomPin = -1, lastRightPin = -1;
                double minLeftHeight = 0, minTopWidth = 0, minRightHeight = 0, minBottomWidth = 0;
                for (int i = 0; i < _pinsByIndex.Count; i++)
                {
                    if (_pinsByIndex[i] is not LoosePin pin)
                        continue;

                    RelativeItem lastOffset, nextOffset;
                    double minimum;
                    if (PointsLeft(pin))
                    {
                        nextOffset = context.GetOffset(pin.Y);
                        double b = _spansByIndex[i].Bounds.Bounds.Height;
                        if (lastLeftPin < 0)
                        {
                            lastOffset = context.GetOffset(_parent.Y);
                            minimum = marginTop + _parent.Margin.Top + 0.5 * b;
                            minLeftHeight = marginTop + _parent.Margin.Vertical + b;
                        }
                        else
                        {
                            lastOffset = context.GetOffset(_pinsByIndex[lastLeftPin].Y);
                            minimum = _parent.Margin.Vertical + 0.5 * (_spansByIndex[lastLeftPin].Bounds.Bounds.Height + b);
                            minLeftHeight += _parent.Margin.Vertical + b;
                        }
                        lastLeftPin = i;
                    }
                    else if (PointsRight(pin))
                    {
                        nextOffset = context.GetOffset(pin.Y);
                        double b = _spansByIndex[i].Bounds.Bounds.Height;
                        if (lastRightPin < 0)
                        {
                            lastOffset = context.GetOffset(_parent.Y);
                            minimum = marginTop + _parent.Margin.Top + 0.5 * b;
                            minRightHeight = marginTop + _parent.Margin.Vertical + b;
                        }
                        else
                        {
                            lastOffset = context.GetOffset(_pinsByIndex[lastRightPin].Y);
                            minimum = _parent.Margin.Vertical + 0.5 * (_spansByIndex[lastRightPin].Bounds.Bounds.Height + b);
                            minRightHeight += _parent.Margin.Vertical + b;
                        }
                        lastRightPin = i;
                    }
                    else if (PointsUp(pin))
                    {
                        nextOffset = context.GetOffset(pin.X);
                        double r = _spansByIndex[i].Bounds.Bounds.Height;
                        if (lastTopPin < 0)
                        {
                            lastOffset = context.GetOffset(_parent.X);
                            minimum = marginLeft + _parent.Margin.Left + 0.5 * r;
                            minTopWidth = marginLeft + _parent.Margin.Horizontal + r;
                        }
                        else
                        {
                            lastOffset = context.GetOffset(_pinsByIndex[lastTopPin].X);
                            minimum = _parent.Margin.Horizontal + 0.5 * (_spansByIndex[lastTopPin].Bounds.Bounds.Height + r);
                            minTopWidth += _parent.Margin.Horizontal + r;
                        }
                        lastTopPin = i;
                    }
                    else if (PointsDown(pin))
                    {
                        nextOffset = context.GetOffset(pin.X);
                        double r = _spansByIndex[i].Bounds.Bounds.Height;
                        if (lastBottomPin < 0)
                        {
                            lastOffset = context.GetOffset(_parent.X);
                            minimum = marginLeft + _parent.Margin.Left + 0.5 * r;
                            minBottomWidth = marginLeft + _parent.Margin.Horizontal + r;
                        }
                        else
                        {
                            lastOffset = context.GetOffset(_pinsByIndex[lastBottomPin].X);
                            minimum = _parent.Margin.Horizontal + 0.5 * (_spansByIndex[lastBottomPin].Bounds.Bounds.Height + r);
                            minBottomWidth += _parent.Margin.Horizontal + r;
                        }
                        lastBottomPin = i;
                    }
                    else
                        throw new NotImplementedException();

                    // Apply the minimum constraint
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.l.{i}", lastOffset, nextOffset, minimum, MinimumWeight);
                }

                // Finish the minimum constraints where necessary
                if (lastLeftPin >= 0)
                {
                    // Finish left side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastLeftPin].Y);
                    var nextOffset = context.GetOffset(Bottom);
                    double minimum = marginBottom + _parent.Margin.Bottom + _spansByIndex[lastLeftPin].Bounds.Bounds.Height * 0.5;
                    minLeftHeight += marginBottom;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.l.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastRightPin >= 0)
                {
                    // Finish right side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastRightPin].Y);
                    var nextOffset = context.GetOffset(Bottom);
                    double minimum = marginBottom + _parent.Margin.Bottom + _spansByIndex[lastRightPin].Bounds.Bounds.Height * 0.5;
                    minRightHeight += marginBottom;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.r.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastLeftPin < 0 && lastRightPin < 0)
                {
                    // There are no vertical pins to determine spacing, so let's place a minimum constraint
                    var lastOffset = context.GetOffset(_parent.Y);
                    var nextOffset = context.GetOffset(Bottom);
                    double minimum = Math.Max(marginTop + marginBottom, _parent.MinHeight);
                    minLeftHeight = minRightHeight = marginTop + marginBottom;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.lr.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastTopPin >= 0)
                {
                    // Finish left side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastTopPin].X);
                    var nextOffset = context.GetOffset(Right);
                    double minimum = marginBottom + _parent.Margin.Right + _spansByIndex[lastTopPin].Bounds.Bounds.Height * 0.5;
                    minTopWidth += marginRight;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.t.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastBottomPin >= 0)
                {
                    // Finish right side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastBottomPin].X);
                    var nextOffset = context.GetOffset(Right);
                    double minimum = marginBottom + _parent.Margin.Right + _spansByIndex[lastBottomPin].Bounds.Bounds.Height * 0.5;
                    minBottomWidth += marginRight;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.b.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastTopPin < 0 && lastBottomPin < 0)
                {
                    // There are no vertical pins to determine spacing, so let's place a minimum constraint
                    var lastOffset = context.GetOffset(_parent.Y);
                    var nextOffset = context.GetOffset(Bottom);
                    double minimum = Math.Max(marginTop + marginBottom, _parent.MinHeight);
                    minTopWidth = minBottomWidth = marginLeft + marginRight;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.tb.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }

                // Add minimum width/height
                if (minLeftHeight < _parent.MinHeight && minRightHeight < _parent.MinHeight)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.minh", context.GetOffset(_parent.Y), context.GetOffset(Bottom), _parent.MinHeight, MinimumWeight);
                if (minTopWidth < _parent.MinWidth && minBottomWidth < _parent.MinWidth)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.minw", context.GetOffset(_parent.X), context.GetOffset(Right), _parent.MinWidth, MinimumWeight);
            }

            private static bool PointsUp(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y < 0;
            private static bool PointsDown(LoosePin pin) => Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y > 0;
            private static bool PointsLeft(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X < 0;
            private static bool PointsRight(LoosePin pin) => Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X > 0;

            /// <inheritdoc />
            public PresenceResult Prepare(IPrepareContext context)
            {
                var pins = _pinsByIndex.OfType<LoosePin>();
                switch (context.Mode)
                {
                    case PreparationMode.Offsets:
                        foreach (var pin in pins)
                        {
                            if (PointsUp(pin))
                            {
                                if (!context.Offsets.Group(_parent.Y, pin.Y, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongY, _parent.Y, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                context.Offsets.Add(pin.X);
                            }
                            else if (PointsDown(pin))
                            {
                                if (!context.Offsets.Group(Bottom, pin.Y, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongY, Bottom, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                context.Offsets.Add(pin.X);
                            }
                            else if (PointsLeft(pin))
                            {
                                if (!context.Offsets.Group(_parent.X, pin.X, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongX, _parent.X, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                context.Offsets.Add(pin.Y);
                            }
                            else if (PointsRight(pin))
                            {
                                if (!context.Offsets.Group(Right, pin.X, 0.0))
                                {
                                    context.Diagnostics?.Post(ErrorCodes.CannotAlignAlongX, _parent.Name, pin.Name);
                                    return PresenceResult.GiveUp;
                                }
                                context.Offsets.Add(pin.Y);
                            }
                        }
                        break;

                    case PreparationMode.Sizes:
                        var style = context.Style.ModifyDashedDotted(_parent);
                        foreach (var pin in _pinsByIndex)
                        {
                            string text = TransformPinName(pin.Name);
                            if (!string.IsNullOrWhiteSpace(text))
                                _spansByIndex.Add(context.TextFormatter.Format(text, style));
                            else
                                _spansByIndex.Add(null);
                        }
                        break;

                    case PreparationMode.Groups:
                        // Everything will be linked to the parent X,Y coordinates
                        foreach (var pin in pins)
                        {
                            context.Group(_parent.X, pin.X);
                            context.Group(_parent.Y, pin.Y);
                        }
                        context.Group(_parent.X, Right);
                        context.Group(_parent.Y, Bottom);
                        break;
                }
                return PresenceResult.Success;
            }

            /// <inheritdoc />
            public IEnumerator<IPin> GetEnumerator() => _pinsByIndex.GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public bool TryGetValue(string name, out IPin pin)
            {
                pin = this[name];
                return true;
            }

            /// <inheritdoc />
            public void Clear()
            {
                _pinsByIndex.Clear();
                _pinsByName.Clear();
            }
        }
    }
}
