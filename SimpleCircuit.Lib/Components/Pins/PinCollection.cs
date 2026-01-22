using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuit.Components.Pins;

/// <summary>
/// A default implementation for a pin collection.
/// </summary>
public class PinCollection : IPinCollection
{
    private readonly Dictionary<string, IPin> _pinsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IPin> _pinsByIndex = [];

    /// <inheritdoc />
    public int Count => _pinsByIndex.Count;

    /// <inheritdoc />
    public IPin this[string name]
    {
        get
        {
            if (_pinsByName.TryGetValue(name, out var pin))
                return pin;
            return null;
        }
    }

    /// <inheritdoc />
    public IPin this[int index]
    {
        get
        {
            if (index < 0 || index >= _pinsByIndex.Count)
                return null;
            return _pinsByIndex[index];
        }
    }

    /// <summary>
    /// Adds a pin to the collection.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <param name="names">The names.</param>
    public void Add(IPin pin, params string[] names)
    {
        _pinsByIndex.Add(pin ?? throw new ArgumentNullException(nameof(pin)));
        foreach (string name in names)
            _pinsByName.Add(name, pin);
    }

    /// <summary>
    /// Adds a pin to the collection.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <param name="names">The names.</param>
    public void Add(IPin pin, IEnumerable<string> names)
    {
        _pinsByIndex.Add(pin ?? throw new ArgumentNullException(nameof(pin)));
        foreach (string name in names)
        {
            _pinsByName.Add(name, pin);
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> NamesOf(IPin pin) => _pinsByName.Where(p => ReferenceEquals(p.Value, pin)).Select(p => p.Key);

    /// <inheritdoc />
    public IEnumerator<IPin> GetEnumerator() => _pinsByIndex.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public bool TryGetValue(string name, out IPin pin) => _pinsByName.TryGetValue(name, out pin);

    /// <inheritdoc />
    public void Clear()
    {
        _pinsByIndex.Clear();
        _pinsByName.Clear();
    }
}
