function registerLanguage(keywords) {

    // Register a new language
    monaco.languages.register({ id: 'simpleCircuit' });

    // Register a tokens provider for the language
    keywordRegex = new RegExp('\\b(?:' + keywords.join("|") + ')[\\w\\d]*\\b(?![\\.\\/])');
    sectionRegex = new RegExp('\\b\\w[\\w\\d]*\\b(?!/)');
    monaco.languages.setMonarchTokensProvider('simpleCircuit', {
        defaultToken: 'invalid',
        includeLF: true,
        lineComment: /\/\/.*\n/,
        tokenizer: {
            root: [
                ['@lineComment', 'comment'],
                [/^[\s\t]*-/, { token: 'dash.assignment', next: '@assignment' }],
                [/^[\s\t]*\(/, { token: 'bracket.virtual', bracket: '@open', next: '@virtual' }],
                [/^[\s\t]*\./, { token: 'dot.command', bracket: '@open', next: '@command' }],
                { include: '@component_chain' },
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
            ],
            virtual: [
                [ '@lineComment', 'comment', '@pop'],
                { include: '@component_chain' },
                [/\)/, { token: 'bracket.$S0', bracket: '@close', next: '@pop' }],
            ],
            assignment: [
                [ '@lineComment', 'comment', '@pop' ],
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
                [ '@lineComment', 'comment', '@pop' ],
                { include: '@whitespace' },
                [/\b([lurdneswa]|ne|nw|se|sw|hidden|nojump|nojmp|n?jmp|dotted|dashed|arrow|rarrow|dot)\b/, { token: 'pindirection.$S0', log: 'wire:$S0 $S1 $S2 $S3' }],
                [/\>/, { token: 'bracket.$S0', bracket: '@close', next: '@pop' }],
                { include: '@number' },
                [/\+/, { token: 'operator.$S0' }],
            ],
            command: [
                [ '@lineComment', 'comment', '@pop' ],
                [/\b\w+\b/, { token: 'word' }],
                [/\n/, { token: 'newline', next: '@pop' }],
            ],
            pin_block: [
                [ '@lineComment', 'comment', '@popall' ],
                { include: '@whitespace' },
                [/\w+/, { token: 'word.pin' }],
                [/\]/, { token: 'bracket.pin', bracket: '@close', next: '@pop' }],
            ],
            label_block: [
                [ '@lineComment', 'comment', '@popall'],
                { include: '@whitespace' },
                [/\)/, { token: 'bracket.label', bracket: '@close', next: '@pop' }],
                { include: '@string' },
            ],
            string: [
                [/"([^"]|\\.)+"/, 'string'],
                [/'([^']|\\.)+'/, 'string'],
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
            { token: 'component', foreground: '9a47ff', fontStyle: 'bold' },
            { token: 'component.virtual', foreground: 'c799ff' },
            { token: 'pindirection', foreground: 'cc9966' },
            { token: 'pindirection.wire.virtual', foreground: 'cc9966' },
            { token: 'string', foreground: 'a633f2' },
            { token: 'operator', foreground: '660000' },
            { token: 'operator.wire', foreground: 'cc0000' },
            { token: 'operator.wire.virtual', foreground: '996666' },
            { token: 'equals', foreground: 'cc0000', fontStyle: 'bold' },
            { token: 'dot', foreground: '0000cc' },
            { token: 'dash', foreground: '0000cc', fontStyle: 'bold' },
            { token: 'word.assignment', foreground: '6666ff', fontStyle: 'bold' },
            { token: 'dot.assignment', foreground: '666666' },
            { token: 'equals.assignment', foreground: '666666' },
            { token: 'boolean', foreground: 'a0a0a0' },
            { token: 'separator', foreground: '0000ff' },
        ]
    });

    monaco.languages.registerCompletionItemProvider('simpleCircuit', {
        provideCompletionItems: (model, position) => {
            var word = model.getWordUntilPosition(position);
            var range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn
            };
            var suggestions = [];
            if (word.word.length > 0) {
                for (var i = 0; i < keywords.length; i++) {
                    keyword = keywords[i];
                    suggestions.push({
                        label: keyword[0],
                        insertText: keyword[0],
                        detail: keyword[1],
                        range: range,
                        kind: monaco.languages.CompletionItemKind.keyword
                    });
                }
            }
            return { suggestions: suggestions };
        }
    });
}

const div_measure = document.getElementById('div_measure');
const svg_style = document.getElementById('svg-style');

function calculateBounds(element) {
    // We simply parse the XML and return the bounds
    var parser = new DOMParser();
    var e = parser.parseFromString(element, "image/svg+xml").documentElement;
    div_measure.appendChild(e);
    var b = e.getBBox();
    div_measure.removeChild(e);
    return {
        x: b.x,
        y: b.y,
        width: b.width,
        height: b.height
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