using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class LibraryCollection
    {
        public const string Storage = "libraries";

        public class LibraryItem
        {
            public XmlDocument Library { get; }
            public bool IsLoaded { get; set; }
            public LibraryItem(bool isLoaded, XmlDocument library)
            {
                IsLoaded = isLoaded;
                Library = library;
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

        /// <inheritdoc />
        protected override async void OnInitialized()
        {
            base.OnInitialized();

            string libraries = await _localStore.GetItemAsStringAsync(Storage);
            if (!string.IsNullOrWhiteSpace(libraries))
            {
                string[] entries = libraries.Split(';');
                if (entries.Length % 3 == 0)
                {
                    for (int i = 0; i < entries.Length; i += 3)
                    {
                        if (string.IsNullOrWhiteSpace(entries[i]) ||
                            string.IsNullOrWhiteSpace(entries[i + 1]) ||
                            string.IsNullOrWhiteSpace(entries[i + 2]))
                            continue;

                        // Decode the library, and parse it as an XmlDocument
                        // Use GZip decompression
                        var bytes = Convert.FromBase64String(entries[i + 2]);
                        var doc = new XmlDocument();
                        using var inputStream = new MemoryStream(bytes);
                        using (System.IO.Compression.GZipStream gzip = new(inputStream,
                            System.IO.Compression.CompressionMode.Decompress))
                        {
                            doc.Load(gzip);
                        }

                        // Add the document
                        Libraries.Add(entries[i], new LibraryItem(entries[i + 1] == "1", doc));
                    }
                }
                StateHasChanged();
            }
        }

        /// <summary>
        /// Toggles the loaded status of the library item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected async void Toggle(string name, LibraryItem item)
        {
            item.IsLoaded = !item.IsLoaded;

            string libraries = await _localStore.GetItemAsStringAsync(Storage);
            if (!string.IsNullOrWhiteSpace(libraries))
            {
                string key = $"{name};";
                int index = libraries.IndexOf($"{name};");
                if (index >= 0)
                {
                    index += key.Length;
                    libraries = libraries.Substring(0, index) + (item.IsLoaded ? "1" : "0") + libraries.Substring(index + 1);
                    await _localStore.SetItemAsStringAsync(Storage, libraries);
                }
            }

            StateHasChanged();
            await LibrariesChanged.InvokeAsync(this);
        }

        /// <summary>
        /// Clears all libraries from the local memory.
        /// </summary>
        public async void Clear()
        {
            if (Libraries.Count == 0)
                return;

            Libraries.Clear();
            await _localStore.RemoveItemAsync(Storage);
            StateHasChanged();
            await LibrariesChanged.InvokeAsync(this);
        }

        /// <summary>
        /// Adds a library to the local memory.
        /// </summary>
        /// <param name="name">The name of the library.</param>
        /// <param name="doc">The library contents.</param>
        public async void Add(string name, XmlDocument doc)
        {
            
            // Append this library to the previously stored library
            string libraries = await _localStore.GetItemAsStringAsync(Storage);
            bool isLoaded = true;

            if (Libraries.TryGetValue(name, out var existing))
            {
                isLoaded = existing.IsLoaded;
                Remove(name);
            }

            // Add the library to our documents
            Libraries.Add(name, new LibraryItem(isLoaded, doc));

            // Store the library in local memory
            using MemoryStream output = new();
            using (System.IO.Compression.GZipStream gzip = new(output, System.IO.Compression.CompressionLevel.SmallestSize))
            {
                doc.Save(gzip);
            }
            if (string.IsNullOrWhiteSpace(libraries))
                libraries = name + ";1;" + Convert.ToBase64String(output.ToArray());
            else
                libraries += ";" + name + ";1;" + Convert.ToBase64String(output.ToArray());

            // Save
            await _localStore.SetItemAsStringAsync(Storage, libraries);
            StateHasChanged();
            await LibrariesChanged.InvokeAsync(this);
        }

        public async void Remove(string name)
        {
            if (Libraries.Remove(name))
            {
                // Load the in-memory libraries
                string libraries = await _localStore.GetItemAsStringAsync(Storage);
                if (string.IsNullOrWhiteSpace(libraries))
                    return;

                // Cut out the deleted portion
                var entries = libraries.Split(';').ToList();
                for (int i = 0; i < entries.Count; i += 3)
                {
                    if (entries[i] == name)
                    {
                        entries.RemoveRange(i, 3);
                        break;
                    }
                }

                // Store back in memory
                await _localStore.SetItemAsStringAsync(Storage, string.Join(";", entries));
                StateHasChanged();

                await LibrariesChanged.InvokeAsync(this);
            }
        }
    }
}
