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

        [Diagnostic(SeverityLevel.Error, "PE001", "Cannot find pin '{0}' for component '{1}'")]
        CannotFindPin,

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
    }
}
