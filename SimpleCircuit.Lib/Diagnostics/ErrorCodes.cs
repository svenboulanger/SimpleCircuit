namespace SimpleCircuit.Diagnostics;

/// <summary>
/// The error codes used by SimpleCircuit.
/// </summary>
public enum ErrorCodes
{
    /// <summary>
    /// No error.
    /// </summary>
    None = 0,

    [Diagnostic(SeverityLevel.Error, null, "The pin '{0}' on component {1} cannot be resolved to a unique orientation")]
    AmbiguousOrientation,

    [Diagnostic(SeverityLevel.Error, null, "Variable '{0}' cannot be used when assigning to itself")]
    CircularVariableReference,
    [Diagnostic(SeverityLevel.Warning, null, "Component '{0}' cannot change location")]
    ComponentCannotChangeLocation,
    [Diagnostic(SeverityLevel.Warning, null, "Component '{0}' does not have any pins")]
    ComponentDoesNotHaveAnyPins,

    [Diagnostic(SeverityLevel.Error, null, "Could not align segment to the correct direction for '{0}'")]
    CouldNotAlignDirection,
    [Diagnostic(SeverityLevel.Error, null, "Could not align '{0}' and '{1}' along the X-axis")]
    CouldNotAlignAlongX,
    [Diagnostic(SeverityLevel.Error, null, "Could not align '{0}' and '{1}' along the Y-axis")]
    CouldNotAlignAlongY,
    [Diagnostic(SeverityLevel.Error, null, "Could not backtrack to anonymous components inside sections")]
    CouldNotBacktrackToSections,
    [Diagnostic(SeverityLevel.Error, null, "Could not constrain orientation of {0}")]
    CouldNotConstrainOrientation,
    [Diagnostic(SeverityLevel.Error, null, "Could not create a component for the name '{0}'")]
    CouldNotCreateComponentForName,
    [Diagnostic(SeverityLevel.Error, null, "Could not find any pins matching '{0}' on components matching '{1}'")]
    CouldNotFindAnyPinsMatching,
    [Diagnostic(SeverityLevel.Error, null, "Could not find the base theme '{0}'")]
    CouldNotFindBaseTheme,
    [Diagnostic(SeverityLevel.Error, null, "Could not find the component '{0}' in subcircuit '{1}' for creating a port")]
    CouldNotFindComponentInSubcircuitForPort,
    [Diagnostic(SeverityLevel.Warning, null, "Could not find any component matching '{0}'")]
    CouldNotFindComponentMatching,
    [Diagnostic(SeverityLevel.Error, null, "Could not find an anonymous component to backtrack to for '{0}' going back {1} components")]
    CouldNotFindBacktrackedAnonymousComponent,
    [Diagnostic(SeverityLevel.Error, null, "Could not find any pins matching '{0}' on component '{1}'")]
    CouldNotFindMatchingPinOnComponent,
    [Diagnostic(SeverityLevel.Error, null, "Cannot find the pin '{0}' on component {1}")]
    CouldNotFindPin,
    [Diagnostic(SeverityLevel.Error, null, "Could not find a pin '{0}' on component '{1}' in subcircuit '{2}' for creating a port")]
    CouldNotFindPinOnComponentInSubcircuitForPort,
    [Diagnostic(SeverityLevel.Error, null, "Could not find the property or variant '{0}' for {1}")]
    CouldNotFindPropertyOrVariantFor,
    [Diagnostic(SeverityLevel.Error, null, "Could not find property '{0}'")]
    CouldNotFindProperty,
    [Diagnostic(SeverityLevel.Error, null, "Could not find a section with the name '{0}'")]
    CouldNotFindSectionWithName,
    [Diagnostic(SeverityLevel.Error, null, "Could not find a variable with the name'{0}'")]
    CouldNotFindVariable,
    [Diagnostic(SeverityLevel.Error, null, "Could not {0} for arguments of type '{1}' and '{2}'")]
    CouldNotOperateForArgumenttype,
    [Diagnostic(SeverityLevel.Error, null, "Could not {0} for argument of type '{1}'")]
    CouldNotOperateForArgumentType,
    [Diagnostic(SeverityLevel.Error, null, "Could not recognize bracket '{0}' and '{1}'")]
    CouldNotRecognizeBracket,
    [Diagnostic(SeverityLevel.Error, null, "Could not recognize control statement '.{0}'")]
    CouldNotRecognizeControlStatement,
    [Diagnostic(SeverityLevel.Error, null, "Could not recognize drawing command '{0}'")]
    CouldNotRecognizeDrawingCommand,
    [Diagnostic(SeverityLevel.Error, null, "Could not recognize path command '{0}'")]
    CouldNotRecognizePathCommand,
    [Diagnostic(SeverityLevel.Error, null, "Could not recognize statement")]
    CouldNotRecognizeStatement,
    [Diagnostic(SeverityLevel.Error, null, "Could not resolve fixed offset {0:g} for '{1}'")]
    CouldNotResolveFixedOffsetFor,
    [Diagnostic(SeverityLevel.Error, null, "Could not resolve a name for '{0}'")]
    CouldNotResolveName,
    [Diagnostic(SeverityLevel.Warning, null, "Could not satisfy a minimum distance for '{0}'")]
    CouldNotSatisfyMinimumDistance,

    [Diagnostic(SeverityLevel.Error, null, "The component '{0}' does not have pins")]
    DoesNotHavePins,
    [Diagnostic(SeverityLevel.Error, null, "Duplicate pin name '{0}' for symbol {1}.")]
    DuplicateSymbolPinName,

    [Diagnostic(SeverityLevel.Error, null, "Expected attribute {0} on {1}")]
    ExpectedAttributeOn,
    [Diagnostic(SeverityLevel.Error, null, "Expected an anonymous component key, but got '{0}'")]
    ExpectedAnonymousKey,
    [Diagnostic(SeverityLevel.Error, null, "Expected between {0} and {1} arguments for method '{2}'")]
    ExpectedBetweenArgumentsFor,
    [Diagnostic(SeverityLevel.Warning, null, "Expected coordinate for attribute {0} on {1}, but was '{2}'")]
    ExpectedCoordinateForOnButWas,
    [Diagnostic(SeverityLevel.Warning, null, "Expected coordinate, but was '{0}'")]
    ExpectedCoordinateButWas,
    [Diagnostic(SeverityLevel.Error, null, "Expected a double")]
    ExpectedDouble,
    [Diagnostic(SeverityLevel.Error, null, "Expected the end value")]
    ExpectedEndValue,
    [Diagnostic(SeverityLevel.Error, null, "Expected an expression")]
    ExpectedExpression,
    [Diagnostic(SeverityLevel.Error, null, "Expected a filename")]
    ExpectedFilename,
    [Diagnostic(SeverityLevel.Error, null, "Expected a for-loop definition")]
    ExpectedForLoopDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected '{0}'")]
    ExpectedGeneric,
    [Diagnostic(SeverityLevel.Error, null, "Expected if-else statement")]
    ExpectedIfElseStatement,
    [Diagnostic(SeverityLevel.Error, null, "Expected the increment")]
    ExpectedIncrementValue,
    [Diagnostic(SeverityLevel.Error, null, "Expected an integer")]
    ExpectedInteger,
    [Diagnostic(SeverityLevel.Error, null, "Expected a literal")]
    ExpectedLiteral,
    [Diagnostic(SeverityLevel.Error, null, "Expected the end of a line")]
    ExpectedNewline,
    [Diagnostic(SeverityLevel.Error, null, "Expected a parameter value")]
    ExpectedParameterValue,
    [Diagnostic(SeverityLevel.Error, null, "Expected a pin name")]
    ExpectedPinName,
    [Diagnostic(SeverityLevel.Error, null, "Expected a property assignment")]
    ExpectedPropertyAssignment,
    [Diagnostic(SeverityLevel.Error, null, "Expected a property name")]
    ExpectedPropertyName,
    [Diagnostic(SeverityLevel.Error, null, "Expected a property value")]
    ExpectedPropertyValue,
    [Diagnostic(SeverityLevel.Error, null, "Expected a scope definition")]
    ExpectedScopeDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected a section definition")]
    ExpectedSectionDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected a section name")]
    ExpectedSectionName,
    [Diagnostic(SeverityLevel.Error, null, "Expected the start value")]
    ExpectedStartValue,
    [Diagnostic(SeverityLevel.Error, null, "Expected a string")]
    ExpectedString,
    [Diagnostic(SeverityLevel.Error, null, "Expected a subcircuit definition")]
    ExpectedSubcircuitDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected a subcircuit port")]
    ExpectedSubcircuitPort,
    [Diagnostic(SeverityLevel.Error, null, "Expected a symbol definition")]
    ExpectedSymbolDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected a symbol key")]
    ExpectedSymbolKey,
    [Diagnostic(SeverityLevel.Error, null, "Expected a theme definition")]
    ExpectedThemeDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected a theme name")]
    ExpectedThemeName,
    [Diagnostic(SeverityLevel.Error, null, "Expected a value or an expression")]
    ExpectedValueOrExpression,
    [Diagnostic(SeverityLevel.Error, null, "Expected a variable name")]
    ExpectedVariableName,
    [Diagnostic(SeverityLevel.Error, null, "Expected a variant name")]
    ExpectedVariantName,
    [Diagnostic(SeverityLevel.Error, null, "Could not convert {0} arguments to a known type")]
    ExpectedVector,
    [Diagnostic(SeverityLevel.Error, null, "Expected a wire definition")]
    ExpectedWireDefinition,
    [Diagnostic(SeverityLevel.Error, null, "Expected a wire segment angle value")]
    ExpectedWireSegmentAngle,

    [Diagnostic(SeverityLevel.Error, null, "The file '{0}' does not exist")]
    FileDoesNotExist,
    [Diagnostic(SeverityLevel.Error, null, "For-loop increment is too small ({0})")]
    ForLoopIncrementTooSmall,

    [Diagnostic(SeverityLevel.Error, null, "Invalid black box pin orientation on '{0}'")]
    InvalidBlackBoxPinDirection,
    [Diagnostic(SeverityLevel.Warning, null, "Invalid marker '{0}'")]
    InvalidMarker,
    [Diagnostic(SeverityLevel.Error, null, "The name '{0}' is invalid")]
    InvalidName,
    [Diagnostic(SeverityLevel.Error, null, "The symbol key '{0}' is invalid. Only words are allowed.")]
    InvalidSymbolKey,
    [Diagnostic(SeverityLevel.Error, null, "Invalid text orientation type '{0}'")]
    InvalidTextOrientationType,
    [Diagnostic(SeverityLevel.Error, null, "Invalid coordinate '{0}' for XML tag {1}.")]
    InvalidXmlCoordinate,

    [Diagnostic(SeverityLevel.Warning, null, "Symbol '{0}' is missing a 'drawing' tag")]
    MissingSymbolDrawing,
    [Diagnostic(SeverityLevel.Warning, null, "One or more symbol keys are missing from the library.")]
    MissingSymbolKey,
    [Diagnostic(SeverityLevel.Warning, null, "Symbol '{0}' is missing a 'pins' tag")]
    MissingSymbolPins,
    [Diagnostic(SeverityLevel.Warning, null, "Multiple base themes specified")]
    MultipleBaseThemes,
    [Diagnostic(SeverityLevel.Warning, null, "The pin '{0}' on component '{1}' is used multiple times as a subcircuit port, causing duplicate port to be removed")]
    MultipleSubcircuitInstancePinPort,

    [Diagnostic(SeverityLevel.Error, null, "A theme definnition should be at root level")]
    NotRootLevelTheme,
    [Diagnostic(SeverityLevel.Error, null, "A subcircuit definition should be at root level")]
    NotRootLevelSubckt,
    [Diagnostic(SeverityLevel.Error, null, "A symbol definition should be at root level")]
    NotRootLevelSymbol,
    [Diagnostic(SeverityLevel.Info, null, "No unknowns to solve for")]
    NoUnknownsToSolve,

    [Diagnostic(SeverityLevel.Error, null, "Undefined wire segments are only allowed for the first and last segment in the wire")]
    UndefinedWireSegment,
    [Diagnostic(SeverityLevel.Error, null, "Unrecognized function '{0}'")]
    UnrecognizedFunction,
    [Diagnostic(SeverityLevel.Error, null, "Unrecognized escape sequence")]
    UnrecognizedEscapeSequence,

    [Diagnostic(SeverityLevel.Error, null, "Quote mismatch")]
    QuoteMismatch,

    [Diagnostic(SeverityLevel.Error, null, "XML Error: {0}")]
    XmlError,
}
