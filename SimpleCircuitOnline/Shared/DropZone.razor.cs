﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SimpleCircuit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleCircuitOnline.Shared
{
    public partial class DropZone
    {
        private ElementReference _dropZoneElement;
        private InputFile _inputFile;
        private IJSObjectReference _module;
        private IJSObjectReference _dropZoneInstance;
        private bool _optionsVisible;

        protected string InternalFilename
        {
            get => Filename;
            set
            {
                if (Filename != value)
                {
                    Filename = value;
                    FilenameChanged.InvokeAsync(value);
                }
            }
        }

        [Parameter]
        public string Filename { get; set; }

        [Parameter]
        public EventCallback<string> FilenameChanged { get; set; }

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback<UploadEventArgs> Upload { get; set; }

        [Parameter]
        public EventCallback<DownloadEventArgs> Download { get; set; }

        [Parameter]
        public bool ShrinkX { get; set; }

        [Parameter]
        public EventCallback<bool> ShrinkXChanged { get; set; }

        [Parameter]
        public bool ShrinkY { get; set; }

        [Parameter]
        public EventCallback<bool> ShrinkYChanged { get; set; }

        [Parameter]
        public bool AutoUpdate { get; set; }

        [Parameter]
        public EventCallback<bool> AutoUpdateChanged { get; set; }

        [Parameter]
        public EventCallback RefreshRequested { get; set; }

        [Parameter]
        public bool ExportLight { get; set; }

        [Parameter]
        public EventCallback<bool> ExportLightChanged { get; set; }

        [Parameter]
        public bool ExportDark { get; set; }

        [Parameter]
        public EventCallback<bool> ExportDarkChanged { get; set; }

        protected string ContainerClasses
        {
            get
            {
                var set = new HashSet<string>() { "dropzone" };
                if (!string.IsNullOrWhiteSpace(Class))
                {
                    foreach (string nc in Class.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        set.Add(nc);
                }
                return string.Join(' ', set);
            }
        }

        private async Task ToggleShrinkX(bool value)
        {
            ShrinkX = value;
            await ShrinkXChanged.InvokeAsync(value);
        }
        private async Task ToggleShrinkY(bool value)
        {
            ShrinkY = value;
            await ShrinkYChanged.InvokeAsync(value);
        }

        private async Task ToggleAutoUpdate(bool value)
        {
            AutoUpdate = value;
            await AutoUpdateChanged.InvokeAsync(value);
        }

        private async Task ToggleExportLight(bool value)
        {
            ExportLight = value;
            await ExportLightChanged.InvokeAsync(value);
        }

        private async Task ToggleExportDark(bool value)
        {
            ExportDark = value;
            await ExportDarkChanged.InvokeAsync(value);
        }

        private async Task RequestRefresh() => await RefreshRequested.InvokeAsync();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Load the JS file
                var uri = new Uri(_navigation.Uri).GetLeftPart(UriPartial.Path);
                _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", Path.Combine(uri, "js/dropzone.js"));

                // Initialize the drop zone
                _dropZoneInstance = await _module.InvokeAsync<IJSObjectReference>("initializeFileDropZone", _dropZoneElement, _inputFile.Element);
            }
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
            StateHasChanged();
        }

        // Called when a new file is uploaded
        protected async Task OnChange(InputFileChangeEventArgs e)
        {
            using var stream = e.File.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            string content = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            UploadEventArgs result = null;

            // Load the XML document
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
            }
            catch (XmlException ex)
            {
                var args = new UploadSvgEventArgs();
                args.Messages.Add(
                    new DiagnosticMessage(SeverityLevel.Error, null,
                    "Invalid XML data found in uploaded SVG file.<br />" + ex.Message));
                await Upload.InvokeAsync(args);
                return;
            }

            if (doc.DocumentElement.Name.ToLower() == "svg")
            {
                var args = new UploadSvgEventArgs();

                // Search for the script
                StringWriter swScript = new();
                foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("sc:script"))
                {
                    if (node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlCDataSection cdata)
                        swScript.WriteLine(cdata.Data);
                    else
                        swScript.WriteLine(node.InnerText);
                }

                // Search for the version
                var nodes = doc.DocumentElement.GetElementsByTagName("sc:version");
                if (nodes.Count > 0)
                    args.Version = nodes[0].InnerText;

                // Call the event
                args.Script = swScript.ToString();
                if (args.Script.Length == 0)
                {
                    args.Messages.Add(
                        new DiagnosticMessage(SeverityLevel.Error, null,
                        "No SimpleCircuit script metadata found in uploaded SVG file."));
                }

                result = args;
            }
            else if (doc.DocumentElement.Name.ToLower() == "symbols")
            {
                var args = new UploadLibraryEventArgs
                {
                    Document = doc
                };
                result = args;
            }

            // Search for the file name
            result.Filename = Path.GetFileNameWithoutExtension(e.File.Name);
            if (result.Filename.EndsWith("_dark"))
                result.Filename = result.Filename.Substring(0, result.Filename.Length - 5);
            else if (result.Filename.EndsWith("_light"))
                result.Filename = result.Filename.Substring(0, result.Filename.Length - 6);
            InternalFilename = result.Filename;
            await Upload.InvokeAsync(result);
        }

        protected async Task DownloadSVG()
        {
            var args = new DownloadEventArgs(DownloadEventArgs.Types.Svg);
            await Download.InvokeAsync(args);
        }
        protected async Task DownloadPNG()
        {
            var args = new DownloadEventArgs(DownloadEventArgs.Types.Png);
            await Download.InvokeAsync(args);
        }
        protected async Task DownloadJPG()
        {
            var args = new DownloadEventArgs(DownloadEventArgs.Types.Jpeg);
            await Download.InvokeAsync(args);
        }
        protected async Task Share()
        {
            var args = new DownloadEventArgs(DownloadEventArgs.Types.Link);
            await Download.InvokeAsync(args);
        }

        // Unregister the drop zone events
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (_dropZoneInstance != null)
            {
                await _dropZoneInstance.InvokeVoidAsync("dispose");
                await _dropZoneInstance.DisposeAsync();
            }

            if (_module != null)
            {
                await _module.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
