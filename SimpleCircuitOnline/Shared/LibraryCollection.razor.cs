using Microsoft.AspNetCore.Components;
using SimpleCircuit;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuitOnline.Shared;

public partial class LibraryCollection
{
    public const string Storage = "libraries";
    public const string LibraryPrefix = "lib:";
    public const string DefaultLibraryLoadedKey = ":def:loaded";

    /// <summary>
    /// Gets or sets whether the default library should be included.
    /// </summary>
    public bool DefaultLibraryLoaded { get; set; } = true;

    public class LibraryItem(bool isLoaded, string encoded, XmlDocument document = null)
    {
        private XmlDocument _doc = document;
        public string Encoded { get; set; } = encoded;
        public bool IsLoaded { get; set; } = isLoaded;
        public XmlDocument Library
        {
            get
            {
                if (_doc == null)
                {
                    var bytes = Convert.FromBase64String(Encoded);
                    _doc = new XmlDocument();
                    using var inputStream = new MemoryStream(bytes);
                    using System.IO.Compression.GZipStream gzip = new(inputStream, System.IO.Compression.CompressionMode.Decompress);
                    _doc.Load(gzip);
                }
                return _doc;
            }
            set => _doc = value;
        }
    }

    /// <summary>
    /// Gets the list of libraries
    /// </summary>
    public Dictionary<string, LibraryItem> Libraries { get; } = [];

    /// <summary>
    /// Called when a library state changes, or when a library is added.
    /// </summary>
    [Parameter]
    public EventCallback LibrariesChanged { get; set; }

    /// <summary>
    /// Builds an evaluation context.
    /// </summary>
    /// <param name="diagnostics">The diagnostics handler.</param>
    /// <returns>Returns the evaluation context.</returns>
    public EvaluationContext BuildContext(IDiagnosticHandler diagnostics)
    {
        // Get the context
        var evalContext = new EvaluationContext(DefaultLibraryLoaded, new Style(), _textFormatter)
        {
            Diagnostics = diagnostics
        };

        // Add the necessary libraries
        foreach (var library in Libraries.OrderBy(lib => lib.Key))
        {
            if (library.Value.IsLoaded)
                evalContext.Factory.Load(library.Value.Library, diagnostics);
        }
        return evalContext;
    }

    /// <summary>
    /// Builds a circuit from a script.
    /// </summary>
    /// <param name="script">The script.</param>
    /// <param name="source">The source.</param>
    /// <param name="options">The options.</param>
    /// <param name="evalContext">The evaluation context, if you already have one.</param>
    /// <returns>Returns the circuit and themes.</returns>
    public (GraphicalCircuit Circuit, Dictionary<string, Dictionary<string, string>> Themes) BuildCircuit(string script, string source, Options options, EvaluationContext evalContext)
    {
        // Parse the script
        var parsingContext = new ParsingContext()
        {
            Diagnostics = evalContext.Diagnostics
        };
        var lexer = SimpleCircuitLexer.FromString(script, source);
        if (!SimpleCircuitParser.Parse(lexer, parsingContext, out var statements))
            return (null, null);

        // Apply any global options
        StatementEvaluator.EvaluateOptions(parsingContext.GlobalOptions, evalContext);

        // Evaluate the script
        StatementEvaluator.Evaluate(statements, evalContext);

        // Return the results
        return (evalContext.Circuit, evalContext.Themes);
    }

    /// <summary>
    /// Loads the libraries from local storage.
    /// </summary>
    /// <returns></returns>
    public async Task LoadLibraries()
    {
        // Get the state of the default library
        string defLoaded = await _localStore.GetItemAsStringAsync(DefaultLibraryLoadedKey);
        if (!string.IsNullOrEmpty(defLoaded))
            DefaultLibraryLoaded = defLoaded == "1";
        else
            DefaultLibraryLoaded = true;

        // Get a list of libraries
        string storage = await _localStore.GetItemAsStringAsync(Storage);
        if (!string.IsNullOrWhiteSpace(storage))
        {
            string[] libraries = storage.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (string name in libraries)
            {
                string encoded = await _localStore.GetItemAsStringAsync($"{name}:xml");
                string strLoaded = await _localStore.GetItemAsStringAsync($"{name}:loaded");
                bool loaded;
                if (!string.IsNullOrEmpty(strLoaded))
                    loaded = strLoaded == "1";
                else
                    loaded = true;
                Libraries.Add(name, new LibraryItem(loaded, encoded));
            }
        }
        StateHasChanged();
    }

    /// <summary>
    /// Toggles the loaded status of the default library.
    /// </summary>
    /// <returns></returns>
    protected async Task ToggleDefault()
    {
        DefaultLibraryLoaded = !DefaultLibraryLoaded;
        await _localStore.SetItemAsStringAsync(DefaultLibraryLoadedKey, DefaultLibraryLoaded ? "1" : "0");
        StateHasChanged();
        await LibrariesChanged.InvokeAsync(this);
    }

    /// <summary>
    /// Toggles the loaded status of the library item.
    /// </summary>
    /// <param name="item">The item.</param>
    protected async Task Toggle(string name, LibraryItem item)
    {
        item.IsLoaded = !item.IsLoaded;
        await _localStore.SetItemAsStringAsync($"{name}:loaded", item.IsLoaded ? "1" : "0");
        StateHasChanged();
        await LibrariesChanged.InvokeAsync(this);
    }

    /// <summary>
    /// Clears all libraries from the local memory.
    /// </summary>
    public async Task Clear()
    {
        // We will clear any entries that end with ":xml" or ":loaded"
        foreach (string key in await _localStore.KeysAsync())
        {
            if (key.EndsWith(":loaded") || key.EndsWith(":xml"))
                await _localStore.RemoveItemAsync(key);
        }
        await _localStore.RemoveItemAsync(Storage);
        Libraries.Clear();
        StateHasChanged();
        await LibrariesChanged.InvokeAsync(this);
    }

    /// <summary>
    /// Adds a library to the local memory.
    /// </summary>
    /// <param name="name">The name of the library.</param>
    /// <param name="doc">The library content document.</param>
    public async void Add(string name, XmlDocument doc)
    {
        // Compress the library contents for later use
        using MemoryStream output = new();
        using (System.IO.Compression.GZipStream gzip = new(output, System.IO.Compression.CompressionLevel.SmallestSize))
        {
            doc.Save(gzip);
        }
        string encoded = Convert.ToBase64String(output.ToArray());

        // Store this information in the local storage memory
        await _localStore.SetItemAsStringAsync($"{name}:loaded", "1");
        await _localStore.SetItemAsStringAsync($"{name}:xml", encoded);

        if (Libraries.TryGetValue(name, out var existing))
        {
            existing.IsLoaded = true;
            existing.Encoded = encoded;
            existing.Library = doc;
        }
        else
        {
            // Add it to the list of entries in our total storage
            Libraries.Add(name, new LibraryItem(true, encoded, doc));
            await _localStore.SetItemAsStringAsync(Storage, string.Join(";", Libraries.Keys));
        }
        StateHasChanged();
        await LibrariesChanged.InvokeAsync(this);
    }

    /// <summary>
    /// Removes a library item from the list.
    /// </summary>
    /// <param name="name">The name of the library.</param>
    public async void Remove(string name)
    {
        if (Libraries.Remove(name))
        {
            await _localStore.RemoveItemAsync($"{name}:loaded");
            await _localStore.RemoveItemAsync($"{name}:xml");
            await _localStore.SetItemAsStringAsync(Storage, string.Join(";", Libraries.Keys));
            StateHasChanged();
            await LibrariesChanged.InvokeAsync(this);
        }
    }
}
