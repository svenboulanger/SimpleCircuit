using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
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
        public bool Bounds { get; set; }

        [Parameter]
        public EventCallback<bool> BoundsChanged { get; set; }

        [Parameter]
        public bool AutoUpdate { get; set; }

        [Parameter]
        public EventCallback<bool> AutoUpdateChanged { get; set; }

        [Parameter]
        public EventCallback RefreshRequested { get; set; }

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
        private async Task ToggleBounds(bool value)
        {
            Bounds = value;
            await BoundsChanged.InvokeAsync(value);
        }
        private async Task ToggleAutoUpdate(bool value)
        {
            AutoUpdate = value;
            await AutoUpdateChanged.InvokeAsync(value);
        }
        private async Task RequestRefresh() => await RefreshRequested.InvokeAsync();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Load the JS file
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/dropzone.js");

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

            var doc = new XmlDocument();
            var args = new UploadEventArgs();
            try
            {
                doc.LoadXml(content);
            }
            catch (XmlException ex)
            {
                args.Errors.Add("Invalid XML data found in uploaded SVG file.<br />" + ex.Message);
                await Upload.InvokeAsync(args);
                return;
            }

            // Search for the script
            StringWriter swScript = new();
            foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("sc:script"))
            {
                if (node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlCDataSection cdata)
                    swScript.WriteLine(cdata.Data);
                else
                    swScript.WriteLine(node.InnerText);
            }

            // Search for the style
            StringWriter swStyle = new();
            foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("style"))
                swStyle.WriteLine(node.InnerText);

            // Search for the version
            var nodes = doc.DocumentElement.GetElementsByTagName("sc:version");
            if (nodes.Count > 0)
                args.Version = nodes[0].InnerText;

            // Search for the file name
            args.Filename = Path.GetFileNameWithoutExtension(e.File.Name);

            // Call the event
            args.Script = swScript.ToString();
            args.Style = swStyle.ToString();
            if (args.Script.Length == 0)
                args.Errors.Add("No SimpleCircuit script metadata found in uploaded SVG file.");
            else if (args.Style.Length == 0)
                args.Warnings.Add("No styling information found in uploaded SVG file.");

            InternalFilename = args.Filename;
            await Upload.InvokeAsync(args);
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
