// Define the keywords
const wireKeywords = [
    ["n", "A wire segment to the north."],
    ["u", "A wire segment upward."],
    ["s", "A wire segment to the south."],
    ["d", "A wire segment downward."],
    ["w", "A wire segment to the west."],
    ["l", "A wire segment to the left."],
    ["e", "A wire segment to the east."],
    ["r", "A wire segment to the right."],
    ["ne", "A wire segment to the north-east."],
    ["nw", "A wire segment to the north-west."],
    ["se", "A wire segment to the south-east."],
    ["sw", "A wire segment to the south-west."],
    ["a", "An angled wire segment."],
    ["x", "A queued anonymous point."],
    ["hidden", "Hides the whole wire."],
    ["visible", "Shows the whole wire."],
    ["nojump", "Avoids jumping over wire intersections."],
    ["jump", "Makes the wire jump over previously drawn wires."],
    ["dotted", "Makes the wire dotted."],
    ["dashed", "Makes the wire dashed."],
    ["arrow", "An arrow marker."],
    ["rarrow", "A reversed arrow marker."],
    ["dot", "A dot marker."],
    ["slash", "A slash marker."],
    ["plus", "A plus sign marker."],
    ["plusb", "A plus sign marker on the other side."],
    ["minus", "A minus sign marker."],
    ["minusb", "A minus sign marker on the other side."],
    ["one", "An ERD-style \"one\" marker."],
    ["onlyone", "An ERD-style \"one and only one\" marker."],
    ["many", "An ERD-style \"many\" marker."],
    ["zeroone", "An ERD-style \"zero or one\" marker."],
    ["onemany", "An ERD-style \"one or many\" marker."],
    ["zeromany", "An ERD-style \"zero or many\" marker"]
];
var componentKeywords = [];

function createCompletionItems(model, position) {
    var word = model.getWordUntilPosition(position);
    var range = {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endColumn: word.endColumn
    };

    var line = model.getValueInRange({
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: 1,
        endColumn: position.column
    });

    // Check if we are inside a wire definition
    var wire = 0;
    for (var i = 0; i < line.length; i++) {
        if (line[i] == '<')
            wire++;
        else if (line[i] == '>')
            wire--;
    }

    var list = [];
    if (wire <= 0)
        list = componentKeywords;
    else
        list = wireKeywords;

    // Return component keywords suggestions
    var suggestions = [];
    for (var i = 0; i < list.length; i++) {
        var keyword = list[i];
        suggestions.push({
            label: keyword[0],
            insertText: keyword[0],
            detail: keyword[1],
            range: range,
            kind: monaco.languages.CompletionItemKind.keyword
        });
    }
    return { suggestions: suggestions };
}

function registerLanguage(keywords) {

    // Register a new language
    monaco.languages.register({ id: 'simpleCircuit' });

    // Register a tokens provider for the language
    fmt = '\\b(?:';
    for (let i = 0; i < keywords.length; i++) {
        if (i > 0)
            fmt += '|';
        fmt += keywords[i][0];
    }
    fmt += ')[\\w\\d]*\\b(?![\\.\\/])'
    keywordRegex = new RegExp(fmt);
    sectionRegex = new RegExp('\\b\\w[\\w\\d]*\\b(?!/)');
    monaco.languages.setMonarchTokensProvider('simpleCircuit', {
        defaultToken: 'invalid',
        includeLF: true,
        tokenizer: {
            root: [
                { include: '@line_comment' },
                [/^[\s\t]*-/, { token: 'dash.assignment', next: '@assignment' }],
                [/^[\s\t]*\(/, { token: 'bracket.virtual', bracket: '@open', next: '@virtual' }],
                [/^[\s\t]*\./, { token: 'dot.command', bracket: '@open', next: '@command' }],
                { include: '@component_chain' },
            ],
            line_comment: [
                [/\/\/.*\n/, 'comment'],
                [/^\s*\*.*\n/, 'comment']
            ],
            component_chain: [
                { include: '@whitespace' },
                [keywordRegex, { token: 'component.$S0' }],
                [/\w+/, { token: 'word.$S0' }],
                [/\//, { token: 'separator.$S0' }],
                [/[\u2190\u2191\u2192\u2193\u2196\u2197\u2198\u2199]+/, { token: 'wire.$S0' }],
                [/\</, { token: 'bracket.wire.$S0', bracket: '@open', next: '@wire.$S0' }],
                [/\[/, { token: 'bracket.pin.$S0', bracket: '@open', next: '@pin_block' }],
                [/\(/, { token: 'bracket.label.$S0', bracket: '@open', next: '@label_block' }],
                [/(\|)([\s\t]*)(\w+)/, [{ token: 'pipe.boxannotation' }, { token: 'white' }, { token: 'word.boxannotation.$S0', bracket: '@open', next: '@boxannotation' }]],
                [/(\|)([^\|]*)(\|)/, [{ token: 'pipe.boxannotation' }, { token: 'pipe.comment' }, { token: 'pipe.boxannotation' }]],
            ],
            virtual: [
                { include: '@line_comment' },
                { include: '@component_chain' },
                [/\)/, { token: 'bracket.$S0', bracket: '@close', next: '@pop' }],
            ],
            assignment: [
                { include: '@line_comment' },
                [/\n/, { token: 'newline', next: '@pop' }],
                { include: '@whitespace' },
                { include: '@number' },
                { include: '@string' },
                { include: '@boolean' },
                [/\=/, 'equals.assignment'],
                [/\w+/, 'word.assignment'],
                [/\./, 'dot.assignment'],
            ],
            wire: [
                { include: '@line_comment' },
                { include: '@whitespace' },
                [/\b([lurdneswa]|ne|nw|se|sw|hidden|nojump|nojmp|n?jmp|dotted|dashed|arrow|rarrow|dot|slash|plusb?|minusb?|one|onlyone|many|zeroone|onemany|zeromany)\b|\?/, { token: 'pindirection.$S0' }],
                [/\b[xX]\b/, { token: 'queuedpoint.$S0', }],
                [/\>/, { token: 'bracket.$S0', bracket: '@close', next: '@pop' }],
                { include: '@number' },
                [/\+/, { token: 'operator.$S0' }],
            ],
            command: [
                { include: '@line_comment' },
                { include: '@whitespace' },
                { include: '@string' },
                [/^[\s\t]*\./, { token: 'dot.command' }],
                [/\b(symbol|SYMBOL)(\s+)(\w+)/, [{ token: 'word' }, { token: 'white' }, { token: 'word', bracket: '@open', next: '@command_symbol', nextEmbedded: 'xml' }]],
                [/\b(css|CSS)\b/, { token: 'word', bracket: '@open', next: '@command_css', nextEmbedded: 'css' }],
                [/\b\w+\b/, { token: 'word' }],
                [/\n/, { token: 'newline', next: '@pop' }],
            ],
            command_symbol: [
                [/^\.(ends|ENDS|endsymbol|ENDSYMBOL)/, { token: '@rematch', next: '@pop', nextEmbedded: '@pop' }],
            ],
            command_css: [
                [/^\.(endc|ENDC|endcss|ENDCSS)/, { token: '@rematch', next: '@pop', nextEmbedded: '@pop' }],
            ],
            boxannotation: [
                { include: '@variants_and_properties' },
                [/\|/, { token: 'pipe.boxannotation', bracket: '@close', next: '@pop' }],
            ],
            pin_block: [
                { include: '@line_comment' },
                { include: '@whitespace' },
                [/\w+/, { token: 'word.pin' }],
                [/\]/, { token: 'bracket.pin', bracket: '@close', next: '@pop' }],
            ],
            label_block: [
                { include: '@variants_and_properties' },
                [/\)/, { token: 'bracket.label', bracket: '@close', next: '@pop' }],
            ],
            variants_and_properties: [
                [/(\w+)(\s*)(=)/, [{ token: 'word.property.$S0' }, { token: 'white' }, { token: 'equals.assignment.$S0' }]],
                [/\w+/, { token: 'variant.$S0' }],
                [/[\+\-]/, { token: 'operator.$S0' }],
                { include: '@number' },
                { include: '@boolean' },
                { include: '@string' },
                { include: '@line_comment' },
                { include: '@whitespace' },
                [/,/, { token: 'comma.$S0' }],
            ],
            string: [
                [/"([^"]|\\.)*"/, { token: 'string.$S0' }],
                [/'([^']|\\.)*'/, { token: 'string.$S0' }],
            ],
            whitespace: [
                [/[ \t]+/, 'white'],
            ],
            number: [
                [/[-]?\d+(\.d+)?/, { token: 'number.$S0' }],
            ],
            boolean: [
                [/true|false/, { token: 'boolean.$S0' }],
            ]
        }
    });

    // Define a new theme that contains only rules that match this language
    monaco.editor.defineTheme('simpleCircuitTheme', {
        base: 'vs',
        inherit: true,
        colors: {
            'editor.foreground': '#000000',
            'editorCursor.foreground': '#0066aa'
            },
        rules: [
            { token: 'word', foreground: '0000ff', fontStyle: 'bold' },
            { token: 'word.property', foreground: '0033ff', fontStyle: 'bold' },
            { token: 'word.virtual', foreground: '9999ff' },
            { token: 'word.pin', foreground: '7a92cf' },
            { token: 'bracket', foreground: 'ff0000' },
            { token: 'bracket.virtual', foreground: 'ff9999' },
            { token: 'bracket.wire', foreground: 'cc9966' },
            { token: 'wire', foreground: 'cc9966', fontStyle: 'bold' },
            { token: 'bracket.wire.virtual', foreground: 'cc9999' },
            { token: 'bracket.pin', foreground: '7a92cf' },
            { token: 'number', foreground: 'a0a0a0' },
            { token: 'number.wire', foreground: 'cc6666' },
            { token: 'number.wire.virtual', foreground: 'cc9999' },
            { token: 'comment', foreground: '00a000' },
            { token: 'component', foreground: '0000aa', fontStyle: 'bold' },
            { token: 'component.virtual', foreground: '6666cc' },
            { token: 'pindirection', foreground: 'cc9966' },
            { token: 'queued', foreground: '6699cc' },
            { token: 'pindirection.wire.virtual', foreground: 'cc9966' },
            { token: 'string', foreground: 'a633f2' },
            { token: 'operator', foreground: '660000' },
            { token: 'operator.wire', foreground: 'cc0000' },
            { token: 'operator.wire.virtual', foreground: '996666' },
            { token: 'equals', foreground: 'cc0000', fontStyle: 'bold' },
            { token: 'dot', foreground: '0000aa' },
            { token: 'dash', foreground: '0000aa', fontStyle: 'bold' },
            { token: 'word.assignment', foreground: '6666ff', fontStyle: 'bold' },
            { token: 'dot.assignment', foreground: '666666' },
            { token: 'equals.assignment', foreground: '666666' },
            { token: 'boolean', foreground: 'a0a0a0' },
            { token: 'separator', foreground: '0000ff' },
            { token: 'variant', foreground: 'cc9999' },
            { token: 'word.label-block', foreground: '9999cc' },
            { token: 'equals.assignment.label-block', foreground: 'cccccc' },
            { token: 'operator.label-block', foreground: 'cc9999' },
            { token: 'tag.xml', foreground: '990000' },
            { token: 'delimiter.xml', foreground: '990000' },
            { token: 'tag.attribute', foreground: 'AA0000' },
            { token: 'attribute.value.xml', foreground: 'a633f2' },
            { token: 'pipe.boxannotation', foreground: 'ff6633' },
            { token: 'pipe.comment', foreground: 'ee9966' },
            { token: 'word.boxannotation', foreground: 'ff6633' },
            { token: 'word.property.boxannotation', foreground: 'ee9966' },
            { token: 'equals.assignment.boxannotation', foreground: 'ee9966' },
            { token: 'comma.boxannotation', foreground: 'ee9966' },
            { token: 'string.boxannotation', foreground: 'ee6666' },
            { token: 'variant.boxannotation', foreground: 'ee9966' },
        ]
    });

    // Define auto-completion items
    componentKeywords = keywords;
    monaco.languages.registerCompletionItemProvider("simpleCircuit", {
        provideCompletionItems: createCompletionItems
    });
}

const svg_style = document.getElementById('svg-style');
const canvas_measure = document.getElementById('canvas');
const context = canvas_measure.getContext('2d');
const div_decode = document.getElementById('div_decode');

function measureText(text, fontfamily, isbold, size) {
    if (isbold) {
        context.font = 'bold ' + size + 'pt ' + fontfamily;
    } else {
        context.font = size + 'pt ' + fontfamily;
    }
    div_decode.innerHTML = text;
    var metrics = context.measureText(div_decode.innerText);
    div_decode.innerText = '';
    return {
        l: 0,
        t: -metrics.actualBoundingBoxAscent,
        r: metrics.actualBoundingBoxRight,
        b: metrics.actualBoundingBoxDescent,
        a: metrics.width
    };
}

function updateStyle(style) {
    svg_style.innerHTML = style;
}

function stopEventPropagation(e) {
    console.log(e);
    e.stopPropagation();
    return e;
}

function copyToClipboard(text) {
    navigator.clipboard.writeText(text);
}

function apply_splitter() {
    window.Split({
        columnGutters: [{
            track: 2,
            element: document.querySelector('.gutter-col'),
        }],
    })
    console.log("Done")
}