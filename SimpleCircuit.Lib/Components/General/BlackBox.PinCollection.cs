using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Spans;
using SimpleCircuit.Drawing.Styles;
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
            private readonly Dictionary<string, LoosePin> _pinsByName = [];
            private readonly List<Span> _spansByIndex = [];
            private readonly List<LoosePin> _pinsByIndex = [];
            private readonly List<Orientation> _pinOrientations = [];
            private int _anonymousIndex = 0;
            private double _extraMargin = 0.0;

            /// <summary>
            /// The possible orientations for a pin.
            /// </summary>
            private enum Orientation
            {
                Left,
                Up,
                Right,
                Down
            }

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
            /// Gets or sets the inner bounds for the labels.
            /// </summary>
            public Bounds InnerBounds { get; set; }

            /// <summary>
            /// Gets the inside margins after accommodating for the pins.
            /// </summary>
            public Margins InnerMargins { get; private set; }

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

                        // Try to add an alias if possible
                        int separator = name.IndexOf('_');
                        if (separator > 0)
                        {
                            string alias = name.Substring(0, separator);
                            if (!_pinsByName.ContainsKey(alias))
                                _pinsByName.Add(alias, pin);
                        }
                    }
                    return pin;
                }
            }

            /// <inheritdoc/>
            public IPin this[int index]
            {
                get
                {
                    if (index == 0 || index == _pinsByIndex.Count - 1)
                    {
                        // If we asked the last or first pin, let's create a new one
                        _anonymousIndex++;
                        string name = $"[ap{_anonymousIndex}]_";
                        var pin = new LoosePin(name, name, _parent);
                        _pinsByIndex.Add(pin);
                        return pin;
                    }
                    return _pinsByIndex[index];
                }
            }

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
            }

            /// <inheritdoc/>
            public IEnumerable<string> NamesOf(IPin pin)
            {
                yield return pin.Name;
            }

            /// <inheritdoc />
            public void Render(IGraphicsBuilder builder, IStyle style)
            {
                double m = style.LineThickness * 0.5;
                for (int i = 0; i < _pinsByIndex.Count; i++)
                {
                    switch (_pinOrientations[i])
                    {
                        case Orientation.Left:
                            builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleLeft + new Vector2(_parent.PinMargins.Left + m, 0), Vector2.UX, TextOrientationType.Transformed);
                            break;
                        case Orientation.Right:
                            builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleRight - new Vector2(_parent.PinMargins.Right - m, 0), Vector2.UX, TextOrientationType.Transformed);
                            break;
                        case Orientation.Up:
                            builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleLeft.Perpendicular + new Vector2(0, _parent.PinMargins.Top + m), Vector2.UY, TextOrientationType.Transformed);
                            break;
                        case Orientation.Down:
                            builder.Text(_spansByIndex[i], _pinsByIndex[i].Location - _spansByIndex[i].Bounds.Bounds.MiddleRight.Perpendicular - new Vector2(0, _parent.PinMargins.Bottom - m), Vector2.UY, TextOrientationType.Transformed);
                            break;
                    }
                }
            }

            private string TransformPinName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return null;

                // Find the separator
                if (name[^1] == '_')
                    return null;
                if (name[0] == '_' && name.Length > 1)
                    return name[1..];
                int separator = name.IndexOf('_');
                if (separator < 0)
                    return name;
                name = name[separator..];
                name = name.Replace("\\[", "[");
                name = name.Replace("\\]", "]");
                return name;
            }

            /// <inheritdoc />
            public void Register(IRegisterContext context)
            {
                // Figure out the edge margins using the labels
                double marginLeft, marginTop, marginRight, marginBottom;
                marginLeft = marginTop = marginRight = marginBottom = _parent.CornerRadius;
                for (int i = 0; i < _pinsByIndex.Count; i++)
                {
                    // Ignore pins that don't have any width to them
                    if (_spansByIndex[i].Bounds.Bounds.Width.IsZero())
                        continue;

                    switch (_pinOrientations[i])
                    {
                        case Orientation.Left:
                            marginLeft = Math.Max(marginLeft, _spansByIndex[i].Bounds.Bounds.Width + _parent.PinMargins.Horizontal + _extraMargin);
                            break;
                        case Orientation.Right:
                            marginRight = Math.Max(marginRight, _spansByIndex[i].Bounds.Bounds.Width + _parent.PinMargins.Horizontal + _extraMargin);
                            break;
                        case Orientation.Up:
                            marginTop = Math.Max(marginTop, _spansByIndex[i].Bounds.Bounds.Width + _parent.PinMargins.Vertical + _extraMargin);
                            break;
                        case Orientation.Down:
                            marginBottom = Math.Max(marginBottom, _spansByIndex[i].Bounds.Bounds.Width + _parent.PinMargins.Vertical + _extraMargin);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                InnerMargins = new(marginLeft, marginTop, marginRight, marginBottom);

                // Place north pins
                int lastLeftPin = -1, lastTopPin = -1, lastBottomPin = -1, lastRightPin = -1;
                double minLeftHeight = 0, minTopWidth = 0, minRightHeight = 0, minBottomWidth = 0;
                for (int i = 0; i < _pinsByIndex.Count; i++)
                {
                    RelativeItem lastOffset, nextOffset;
                    double minimum;
                    var pin = _pinsByIndex[i];
                    switch (_pinOrientations[i])
                    {
                        case Orientation.Left:
                            {
                                nextOffset = context.GetOffset(pin.Y);
                                double b = _spansByIndex[i].Bounds.Bounds.Height;
                                if (lastLeftPin < 0)
                                {
                                    lastOffset = context.GetOffset(_parent.Y);
                                    minimum = marginTop + _parent.PinMargins.Top + 0.5 * b;
                                    minLeftHeight = marginTop + _parent.PinMargins.Vertical + b;
                                }
                                else
                                {
                                    lastOffset = context.GetOffset(_pinsByIndex[lastLeftPin].Y);
                                    minimum = _parent.PinMargins.Vertical + 0.5 * (_spansByIndex[lastLeftPin].Bounds.Bounds.Height + b);
                                    minLeftHeight += _parent.PinMargins.Vertical + b;
                                }
                                lastLeftPin = i;
                            }
                            break;

                        case Orientation.Right:
                            {
                                nextOffset = context.GetOffset(pin.Y);
                                double b = _spansByIndex[i].Bounds.Bounds.Height;
                                if (lastRightPin < 0)
                                {
                                    lastOffset = context.GetOffset(_parent.Y);
                                    minimum = marginTop + _parent.PinMargins.Top + 0.5 * b;
                                    minRightHeight = marginTop + _parent.PinMargins.Vertical + b;
                                }
                                else
                                {
                                    lastOffset = context.GetOffset(_pinsByIndex[lastRightPin].Y);
                                    minimum = _parent.PinMargins.Vertical + 0.5 * (_spansByIndex[lastRightPin].Bounds.Bounds.Height + b);
                                    minRightHeight += _parent.PinMargins.Vertical + b;
                                }
                                lastRightPin = i;
                            }
                            break;

                        case Orientation.Up:
                            {
                                nextOffset = context.GetOffset(pin.X);
                                double r = _spansByIndex[i].Bounds.Bounds.Height;
                                if (lastTopPin < 0)
                                {
                                    lastOffset = context.GetOffset(_parent.X);
                                    minimum = marginLeft + _parent.PinMargins.Left + 0.5 * r;
                                    minTopWidth = marginLeft + _parent.PinMargins.Horizontal + r;
                                }
                                else
                                {
                                    lastOffset = context.GetOffset(_pinsByIndex[lastTopPin].X);
                                    minimum = _parent.PinMargins.Horizontal + 0.5 * (_spansByIndex[lastTopPin].Bounds.Bounds.Height + r);
                                    minTopWidth += _parent.PinMargins.Horizontal + r;
                                }
                                lastTopPin = i;
                            }
                            break;

                        case Orientation.Down:
                            {
                                nextOffset = context.GetOffset(pin.X);
                                double r = _spansByIndex[i].Bounds.Bounds.Height;
                                if (lastBottomPin < 0)
                                {
                                    lastOffset = context.GetOffset(_parent.X);
                                    minimum = marginLeft + _parent.PinMargins.Left + 0.5 * r;
                                    minBottomWidth = marginLeft + _parent.PinMargins.Horizontal + r;
                                }
                                else
                                {
                                    lastOffset = context.GetOffset(_pinsByIndex[lastBottomPin].X);
                                    minimum = _parent.PinMargins.Horizontal + 0.5 * (_spansByIndex[lastBottomPin].Bounds.Bounds.Height + r);
                                    minBottomWidth += _parent.PinMargins.Horizontal + r;
                                }
                                lastBottomPin = i;
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    // Apply the minimum constraint
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.l.{i}", lastOffset, nextOffset, minimum, MinimumWeight);
                }

                // Finish the minimum constraints where necessary
                if (lastLeftPin >= 0)
                {
                    // Finish left side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastLeftPin].Y);
                    var nextOffset = context.GetOffset(Bottom);
                    double minimum = marginBottom + _parent.PinMargins.Bottom + _spansByIndex[lastLeftPin].Bounds.Bounds.Height * 0.5;
                    minLeftHeight += marginBottom;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.l.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastRightPin >= 0)
                {
                    // Finish right side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastRightPin].Y);
                    var nextOffset = context.GetOffset(Bottom);
                    double minimum = marginBottom + _parent.PinMargins.Bottom + _spansByIndex[lastRightPin].Bounds.Bounds.Height * 0.5;
                    minRightHeight += marginBottom;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.r.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastTopPin >= 0)
                {
                    // Finish left side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastTopPin].X);
                    var nextOffset = context.GetOffset(Right);
                    double minimum = marginRight + _parent.PinMargins.Right + _spansByIndex[lastTopPin].Bounds.Bounds.Height * 0.5;
                    minTopWidth += marginRight;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.t.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }
                if (lastBottomPin >= 0)
                {
                    // Finish right side minimum
                    var lastOffset = context.GetOffset(_pinsByIndex[lastBottomPin].X);
                    var nextOffset = context.GetOffset(Right);
                    double minimum = marginRight + _parent.PinMargins.Right + _spansByIndex[lastBottomPin].Bounds.Bounds.Height * 0.5;
                    minBottomWidth += marginRight;
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.b.m", lastOffset, nextOffset, minimum, MinimumWeight);
                }

                // Add minimum width/height
                double minHeight = Math.Max(_parent.MinHeight, marginTop + marginBottom + InnerBounds.Height);
                if (minLeftHeight < minHeight && minRightHeight < minHeight)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.minh", context.GetOffset(_parent.Y), context.GetOffset(Bottom), minHeight, MinimumWeight);
                double minWidth = Math.Max(_parent.MinWidth, marginLeft + marginRight + InnerBounds.Width);
                if (minTopWidth < minWidth && minBottomWidth < minWidth)
                    MinimumConstraint.AddMinimum(context.Circuit, $"{_parent.Name}.minw", context.GetOffset(_parent.X), context.GetOffset(Right), minWidth, MinimumWeight);
            }

            /// <inheritdoc />
            public PresenceResult Prepare(IPrepareContext context)
            {
                var pins = _pinsByIndex.OfType<LoosePin>();
                switch (context.Mode)
                {
                    case PreparationMode.Reset:
                        _pinOrientations.Clear();
                        break;

                    case PreparationMode.Sizes:
                        var style = context.Style.ModifyDashedDotted(_parent);
                        _extraMargin = style.LineThickness * 0.5;

                        // The orientations have just finished, let's decide on the orientation for all pins now
                        foreach (var pin in _pinsByIndex)
                        {
                            if (Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y < 0)
                                _pinOrientations.Add(Orientation.Up);
                            else if (Math.Abs(pin.Orientation.Y) > Math.Abs(pin.Orientation.X) && pin.Orientation.Y > 0)
                                _pinOrientations.Add(Orientation.Down);
                            else if (Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X < 0)
                                _pinOrientations.Add(Orientation.Left);
                            else if (Math.Abs(pin.Orientation.X) > Math.Abs(pin.Orientation.Y) && pin.Orientation.X > 0)
                                _pinOrientations.Add(Orientation.Right);
                            else
                            {
                                context.Diagnostics?.Post(pin.Sources, ErrorCodes.InvalidBlackBoxPinDirection, _parent.Name);
                                return PresenceResult.GiveUp;
                            }
                        }

                        // Format all the pin names
                        Span emptySpan = null;
                        foreach (var pin in _pinsByIndex)
                        {
                            string text = TransformPinName(pin.Name);
                            if (!string.IsNullOrWhiteSpace(text))
                                _spansByIndex.Add(context.TextFormatter.Format(text, style));
                            else
                            {
                                emptySpan ??= context.TextFormatter.Format(string.Empty, style);
                                _spansByIndex.Add(emptySpan);
                            }
                        }
                        break;

                    case PreparationMode.Offsets:
                        for (int i = 0; i < _pinsByIndex.Count; i++)
                        {
                            var pin = _pinsByIndex[i];
                            switch (_pinOrientations[i])
                            {
                                case Orientation.Up:
                                    if (!context.Offsets.Group(_parent.Y, pin.Y, 0.0))
                                    {
                                        context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongY, _parent.Y, pin.Name);
                                        return PresenceResult.GiveUp;
                                    }
                                    context.Offsets.Add(pin.X);
                                    break;
                                case Orientation.Down:
                                    if (!context.Offsets.Group(Bottom, pin.Y, 0.0))
                                    {
                                        context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongY, Bottom, pin.Name);
                                        return PresenceResult.GiveUp;
                                    }
                                    context.Offsets.Add(pin.X);
                                    break;
                                case Orientation.Left:
                                    if (!context.Offsets.Group(_parent.X, pin.X, 0.0))
                                    {
                                        context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongX, _parent.X, pin.Name);
                                        return PresenceResult.GiveUp;
                                    }
                                    context.Offsets.Add(pin.Y);
                                    break;
                                case Orientation.Right:
                                    if (!context.Offsets.Group(Right, pin.X, 0.0))
                                    {
                                        context.Diagnostics?.Post(ErrorCodes.CouldNotAlignAlongX, _parent.Name, pin.Name);
                                        return PresenceResult.GiveUp;
                                    }
                                    context.Offsets.Add(pin.Y);
                                    break;
                                default:
                                    return PresenceResult.GiveUp;
                            }
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
