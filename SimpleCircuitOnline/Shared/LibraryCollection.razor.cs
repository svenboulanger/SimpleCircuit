using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class LibraryCollection
    {
        public const string Storage = "libraries";
        public const string LibraryPrefix = "lib:";

        public class LibraryItem
        {
            private XmlDocument _doc = null;

            public string Encoded { get; set; }

            public bool IsLoaded { get; set; }

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
            public LibraryItem(bool isLoaded, string encoded, XmlDocument document = null)
            {
                IsLoaded = isLoaded;
                Encoded = encoded;
                _doc = document;
            }
        }

        /// <summary>
        /// Gets the list of libraries
        /// </summary>
        public Dictionary<string, LibraryItem> Libraries { get; } = new Dictionary<string, LibraryItem>();

        /// <summary>
        /// Called when a library state changes, or when a library is added.
        /// </summary>
        [Parameter]
        public EventCallback LibrariesChanged { get; set; }

        /// <summary>
        /// Loads the libraries from local storage.
        /// </summary>
        /// <returns></returns>
        public async Task LoadLibraries()
        {
            // Get a list of libraries
            string storage = await _localStore.GetItemAsStringAsync(Storage);
            if (!string.IsNullOrWhiteSpace(storage))
            {
                string[] libraries = storage.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (string name in libraries)
                {
                    string encoded = await _localStore.GetItemAsStringAsync($"{name}:xml");
                    bool loaded = await _localStore.GetItemAsStringAsync($"{name}:loaded") == "1";
                    Libraries.Add(name, new LibraryItem(loaded, encoded));
                }
                StateHasChanged();
            }
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
}
