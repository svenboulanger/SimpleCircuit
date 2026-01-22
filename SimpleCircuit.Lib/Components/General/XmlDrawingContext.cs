using SimpleCircuit.Circuits.Contexts;
using SimpleCircuit.Components.Labeling;
using SimpleCircuit.Components.Pins;
using SimpleCircuit.Components.Variants;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing;
using SimpleCircuit.Drawing.Builders;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuit.Components.General;

/// <summary>
/// A context for drawing XML drawables.
/// </summary>
/// <remarks>
/// Creates a new <see cref="XmlDrawingContext"/>.
/// </remarks>
/// <param name="labels">The defined labels.</param>
/// <param name="variants">The defined variants.</param>
public class XmlDrawingContext(Labels labels, VariantSet variants) : IXmlDrawingContext
{
    private readonly VariantSet _variants = variants;

    /// <inheritdoc />
    public IList<LabelAnchorPoint> Anchors { get; } = [];

    /// <inheritdoc />
    public Labels Labels { get; } = labels ?? throw new ArgumentNullException(nameof(labels));

    /// <inheritdoc />
    public VariantSet Variants => _variants;

    /// <inheritdoc />
    public IPinCollection Pins => throw new NotImplementedException();

    /// <inheritdoc />
    public IEnumerable<string[]> Properties => [];

    /// <inheritdoc />
    public Bounds Bounds => throw new NotImplementedException();

    /// <inheritdoc />
    /// <remarks>Not used.</remarks>
    public string Name => throw new NotImplementedException();

    /// <inheritdoc />
    /// <remarks>Not used.</remarks>
    public List<TextLocation> Sources => throw new NotImplementedException();

    /// <inheritdoc />
    /// <remarks>Not used.</remarks>
    public int Order => throw new NotImplementedException();

    /// <inheritdoc />
    public IStyleModifier Modifier { get; set; }

    /// <inheritdoc />
    public bool Contains(string variant) => _variants?.Contains(variant) ?? false;

    /// <inheritdoc />
    public bool SetProperty(Token propertyToken, object value, IDiagnosticHandler diagnostics) => throw new NotImplementedException();

    /// <inheritdoc />
    public void Render(IGraphicsBuilder builder) => throw new NotImplementedException();

    /// <inheritdoc />
    public void Register(IRegisterContext context) => throw new NotImplementedException();

    /// <inheritdoc />
    public void Update(IUpdateContext context) => throw new NotImplementedException();

    /// <inheritdoc />
    public PresenceResult Prepare(IPrepareContext context) => throw new NotImplementedException();
}
