namespace SimpleCircuit.Diagnostics
{
    /// <summary>
    /// The error codes used by SimpleCircuit.
    /// </summary>
    public enum ErrorCodes
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        [Diagnostic(SeverityLevel.Error, "PE002", "The component '{0}' does not have pins")]
        DoesNotHavePins,

        [Diagnostic(SeverityLevel.Error, "PE003", "Expected a component name")]
        ExpectedComponentName,

        [Diagnostic(SeverityLevel.Error, "PE004", "Could not recognize or create a component for '{0}'")]
        CouldNotRecognizeOrCreateComponent,

        [Diagnostic(SeverityLevel.Warning, "PW005", "Labeling is not possible for '{0}'")]
        LabelingNotPossible,

        [Diagnostic(SeverityLevel.Error, "PE006", "Expected a variant")]
        ExpectedVariant,

        [Diagnostic(SeverityLevel.Warning, "PW007", "Could not recognize variant '{0}' for component '{1}'")]
        CouldNotRecognizeVariant,

        [Diagnostic(SeverityLevel.Error, "PE008", "Expected label or variant for '{0}'")]
        ExpectedLabelOrVariant,

        [Diagnostic(SeverityLevel.Error, "PE009", "Bracket mismatch, '{0}' expected")]
        BracketMismatch,

        [Diagnostic(SeverityLevel.Error, "PE010", "Expected a wire")]
        ExpectedWire,

        [Diagnostic(SeverityLevel.Error, "PE011", "Could not recognize direction '{0}'")]
        CouldNotRecognizeDirection,

        [Diagnostic(SeverityLevel.Error, "PE012", "Expected a pin")]
        ExpectedPin,

        [Diagnostic(SeverityLevel.Error, "PE013", "Could not recognize option '{0}'")]
        CouldNotRecognizeOption,

        [Diagnostic(SeverityLevel.Error, "PE014", "Expected a subcircuit definition")]
        ExpectedSubcircuit,

        [Diagnostic(SeverityLevel.Error, "PE015", "Unexpected end of code")]
        UnexpectedEndOfCode,

        [Diagnostic(SeverityLevel.Error, "PE016", "Expected a section name")]
        ExpectedSectionName,

        [Diagnostic(SeverityLevel.Error, "PE017", "Could not recognize global option '{0}'")]
        CouldNotRecognizeGlobalOption,

        [Diagnostic(SeverityLevel.Warning, "PW018", "Expected assignment")]
        ExpectedAssignment,

        [Diagnostic(SeverityLevel.Error, "PE019", "Could not recognize the type of {0}")]
        CouldNotRecognizeType,

        [Diagnostic(SeverityLevel.Error, "PE020", "Unexpected end of line")]
        UnexpectedEndOfLine,

        [Diagnostic(SeverityLevel.Error, "PE021", "Expected a virtual chain")]
        ExpectedVirtualChain,

        [Diagnostic(SeverityLevel.Error, "PE022", "Expected a virtual chain direction")]
        ExpectedVirtualChainDirection,

        [Diagnostic(SeverityLevel.Error, "PE023", "Could not recognize virtual chain direction '{0}'")]
        CouldNotRecognizeVirtualChainDirection,

        [Diagnostic(SeverityLevel.Error, "PE024", "Expected a property assignment")]
        ExpectedPropertyAssignment,

        [Diagnostic(SeverityLevel.Error, "PE025", "Expected a '.'")]
        ExpectedDot,

        [Diagnostic(SeverityLevel.Error, "PE026", "Expected a property")]
        ExpectedProperty,

        [Diagnostic(SeverityLevel.Error, "PE027", "Could not find the property or variant '{0}' for {1}")]
        CouldNotFindPropertyOrVariant,

        [Diagnostic(SeverityLevel.Error, "PE028", "Expected boolean")]
        ExpectedBoolean,

        [Diagnostic(SeverityLevel.Error, "PE029", "Expected a number")]
        ExpectedNumber,

        [Diagnostic(SeverityLevel.Error, "PE030", "Expected a string")]
        ExpectedString,

        [Diagnostic(SeverityLevel.Error, "PE031", "XML Error: {0}")]
        XMLError,

        [Diagnostic(SeverityLevel.Error, "PE032", "Component {0} does not have a location")]
        ComponentWithoutLocation,

        [Diagnostic(SeverityLevel.Warning, "PW033", "Cannot find any component matching '{0}' for virtual chain")]
        VirtualChainComponentNotFound,

        [Diagnostic(SeverityLevel.Warning, "PW034", "Arrows can only appear at the start or end of a wire definition")]
        ArrowWireStartEnd,

        [Diagnostic(SeverityLevel.Warning, "PW035", "Slashes can only appear at the start or end of a wire definition")]
        SlashesWireStartEnd,

        [Diagnostic(SeverityLevel.Warning, "PW036", "Dots can only appear at the start or end of a wire definition")]
        DotWireStartEnd,

        [Diagnostic(SeverityLevel.Error, "PE037", "Cannot create a component with a wildcard character ('*')")]
        NoWildcardCharacter,

        [Diagnostic(SeverityLevel.Error, "PE038", "Expected a component key.")]
        ExpectedComponentKey,

        [Diagnostic(SeverityLevel.Error, "PE039", "'{0}' is not a component key.")]
        NotAKey,

        [Diagnostic(SeverityLevel.Error, "PE040", "Cannot find the component '{0}'")]
        CouldNotFindDrawable,

        [Diagnostic(SeverityLevel.Error, "PE040", "Cannot find the wire '{0}'")]
        CouldNotFindWire,

        [Diagnostic(SeverityLevel.Error, "PE041", "Cannot find the pin '{0}' on component {1}")]
        CouldNotFindPin,

        [Diagnostic(SeverityLevel.Error, "PE042", "Could not constrain pin '{0}' on component {1}")]
        UnconstrainedPin,

        [Diagnostic(SeverityLevel.Error, "PE043", "The pin '{0}' on component {1} cannot be resolved to a unique orientation")]
        AmbiguousOrientation,

        [Diagnostic(SeverityLevel.Error, "PE044", "Undefined wire segments are only allowed for the first and last segment in the wire")]
        UndefinedWireSegment,

        [Diagnostic(SeverityLevel.Warning, "PW045", "The scale of symbol '{0}' is not a number")]
        SymbolScaleNotANumber,

        [Diagnostic(SeverityLevel.Error, "PE046", "Expected a property value")]
        ExpectedPropertyValue,

        [Diagnostic(SeverityLevel.Error, "PE047", "Could not constrain orientation of {0}")]
        CouldNotConstrainOrientation,

        [Diagnostic(SeverityLevel.Warning, "PW048", "Too many labels are specified")]
        TooManyLabels,

        [Diagnostic(SeverityLevel.Error, "LE001", "Quote mismatch")]
        QuoteMismatch,

        [Diagnostic(SeverityLevel.Error, "PE049", "Duplicate section name '{0}'")]
        DuplicateSection,

        [Diagnostic(SeverityLevel.Error, "PE050", "Could not find section template for '{0}'")]
        UnknownSectionTemplate,

        [Diagnostic(SeverityLevel.Error, "PE051", "Virtual wires cannot contain unconstrained segments")]
        VirtualWireUnconstrainedSegment,

        [Diagnostic(SeverityLevel.Error, "PE052", "Virtual wires cannot contain unknown segment orientations")]
        VirtualWireUnknownSegment,

        [Diagnostic(SeverityLevel.Info, "SOL001", "No unknowns to solve for")]
        NoUnknownsToSolve,

        [Diagnostic(SeverityLevel.Error, "DE001", "Could not recognize drawing command '{0}'")]
        CouldNotRecognizeDrawingCommand,

        [Diagnostic(SeverityLevel.Error, "DE002", "No polygon data given")]
        NoPolygonData,

        [Diagnostic(SeverityLevel.Error, "DE003", "No polyline data given")]
        NoPolylineData,

        [Diagnostic(SeverityLevel.Error, "DE004", "Could not recognize path command '{0}'")]
        CouldNotRecognizePathCommand,

        [Diagnostic(SeverityLevel.Error, "DE005", "No path data given")]
        NoPathData,

        [Diagnostic(SeverityLevel.Error, "DE006", "Invalid scale '{0}' for XML tag {1}.")]
        InvalidXmlScale,

        [Diagnostic(SeverityLevel.Error, "DE007", "Invalid rotation '{0}' for XML tag {1}.")]
        InvalidXmlRotation,

        [Diagnostic(SeverityLevel.Error, "DE008", "Unrecognized attribute '{0}' for XML tag {1}.")]
        UnrecognizedXmlAttribute,

        [Diagnostic(SeverityLevel.Error, "DE009", "Invalid coordinate '{0}' for XML tag {1}.")]
        InvalidXmlCoordinate,

        [Diagnostic(SeverityLevel.Error, "DE010", "Duplicate pin name '{0}' for symbol {1}.")]
        DuplicateSymbolPinName,

        [Diagnostic(SeverityLevel.Error, "PE053", "Could not recognize start of statement '{0}'")]
        CouldNotRecognizeStatementStart,

        [Diagnostic(SeverityLevel.Error, "PE054", "Expected control statement type")]
        ExpectedControlStatementType,

        [Diagnostic(SeverityLevel.Error, "PE055", "Expected the symbol name")]
        ExpectedSymbolName,

        [Diagnostic(SeverityLevel.Error, "PE056", "Expected attribute {0} on {1}")]
        ExpectedAttributeOn,

        [Diagnostic(SeverityLevel.Warning, "DW057", "Expected coordinate for attribute {0} on {1}, but was '{2}'")]
        ExpectedCoordinateForOnButWas,

        [Diagnostic(SeverityLevel.Warning, "DW058", "Expected coordinate, but was '{0}'")]
        ExpectedCoordinateButWas,

        [Diagnostic(SeverityLevel.Error, "SE059", "Cannot resolve fixed offset {0:g} for '{1}'")]
        CannotResolveFixedOffsetFor,

        [Diagnostic(SeverityLevel.Error, "SE059", "Cannot align '{0}' and '{1}' along the X-axis")]
        CannotAlignAlongX,

        [Diagnostic(SeverityLevel.Error, "SE059", "Cannot align '{0}' and '{1}' along the Y-axis")]
        CannotAlignAlongY,

        [Diagnostic(SeverityLevel.Warning, "SW060", "Could not satisfy a minimum distance of {0} in the X-direction for '{1}'")]
        CouldNotSatisfyMinimumOfForInX,

        [Diagnostic(SeverityLevel.Warning, "SW061", "Could not satisfy a minimum distance of {0} in the Y-direction for '{1}'")]
        CouldNotSatisfyMinimumOfForInY,

        [Diagnostic(SeverityLevel.Warning, "PW062", "An anonymous queued point is not available")]
        NotEnoughAnonymousPoints,

        [Diagnostic(SeverityLevel.Warning, "PW063", "The anonymous queued point is not being used")]
        LeftOverAnonymousPoints,

        [Diagnostic(SeverityLevel.Error, "PE064", "Expected an annotation type")]
        ExpectedAnnotationType,

        [Diagnostic(SeverityLevel.Error, "PE065", "Expected an annotation name")]
        ExpectedAnnotationName,

        [Diagnostic(SeverityLevel.Error, "PE066", "Cannot create annotation with the name '{0}' as a component already exists with the same name")]
        AnnotationComponentAlreadyExists,

        [Diagnostic(SeverityLevel.Error, "PE067", "Expected a pipe '|' character")]
        ExpectedPipe,

        [Diagnostic(SeverityLevel.Error, "PE068", "Annotation marker mismatch. Could not find matching annotation start")]
        AnnotationMismatch,

        [Diagnostic(SeverityLevel.Error, "PE069", "expected start of annotation")]
        ExpectedAnnotationStart,

        [Diagnostic(SeverityLevel.Error, "PE070", "Expected end of annotation")]
        ExpectedAnnotationEnd,

        [Diagnostic(SeverityLevel.Error, "PE071", "Queued anonymous points are not allowed inside virtual chains")]
        VirtualChainAnonymousPoints,

        [Diagnostic(SeverityLevel.Error, "PE072", "Expected a comma vector separator")]
        ExpectedComma,

        [Diagnostic(SeverityLevel.Warning, "PW073", "Symbol '{0}' is missing a 'pins' tag")]
        MissingSymbolPins,

        [Diagnostic(SeverityLevel.Warning, "PW074", "Symbol '{0}' is missing a 'drawing' tag")]
        MissingSymbolDrawing,

        [Diagnostic(SeverityLevel.Warning, "PW075", "Invalid marker '{0}'")]
        InvalidMarker,

        [Diagnostic(SeverityLevel.Warning, "PW076", "One or more symbol keys are missing from the library.")]
        MissingSymbolKey,

        [Diagnostic(SeverityLevel.Error, "PE077", "The symbol key '{0}' is invalid. Only words are allowed.")]
        InvalidSymbolKey,
    }
}
