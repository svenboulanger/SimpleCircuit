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

        public class ShrinkToSizeEventArgs : EventArgs
        {
            public bool ShrinkToWidth { get; set; }
            public bool ShrinkToHeight { get; set; }
        }

        [Parameter]
        public string Class { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback<UploadEventArgs> Upload { get; set; }

        [Parameter]
        public EventCallback<DownloadEventArgs> Download { get; set; }

        [Parameter]
        public EventCallback<ShrinkToSizeEventArgs> ShrinkToSizeChanged { get; set; }

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

        ElementReference dropZoneElement;
        InputFile inputFile;

        IJSObjectReference _module;
        IJSObjectReference _dropZoneInstance;
        bool _shrinkToWidth = true, _shrinkToHeight = true;

        protected async Task OnShrinkToWidthChanged()
        {
            _shrinkToWidth = !_shrinkToWidth;
            StateHasChanged();
            await ShrinkToSizeChanged.InvokeAsync(
                new ShrinkToSizeEventArgs()
                {
                    ShrinkToWidth = _shrinkToWidth,
                    ShrinkToHeight = _shrinkToHeight,
                });
        }

        protected async Task OnShrinkToHeightChanged()
        {
            _shrinkToHeight = !_shrinkToHeight;
            StateHasChanged();
            await ShrinkToSizeChanged.InvokeAsync(
                new ShrinkToSizeEventArgs()
                {
                    ShrinkToWidth = _shrinkToWidth,
                    ShrinkToHeight = _shrinkToHeight,
                });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Load the JS file
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/dropzone.js");

                // Initialize the drop zone
                _dropZoneInstance = await _module.InvokeAsync<IJSObjectReference>("initializeFileDropZone", dropZoneElement, inputFile.Element);
            }
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
            catch (XmlException)
            {
                args.Errors = "Invalid XML data found in uploaded SVG file.";
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

            // Call the event
            args.Script = swScript.ToString();
            args.Style = swStyle.ToString();
            if (args.Script.Length == 0)
                args.Errors = "No SimpleCircuit script metadata found in uploaded SVG file.";
            else if (args.Style.Length == 0)
                args.Warnings = "No styling information found in uploaded SVG file.";
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
        public async ValueTask DisposeAsync()
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
        }
    }
}
