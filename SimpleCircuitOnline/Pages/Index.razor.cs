using BlazorMonaco.Editor;
using Microsoft.JSInterop;
using SimpleCircuit;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using SimpleCircuitOnline.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Xml;
using Position = BlazorMonaco.Position;

namespace SimpleCircuitOnline.Pages
{
    public partial class Index
    {
        private Task _currentSolver = null;
        private readonly Logger _logger = new();
        private string _simpleCircuitVersion;
        private XmlDocument _svg;
        private readonly Timer _timer = new(750) { Enabled = false, AutoReset = true };
        private readonly object _lock = new();
        private int _updates = 0;
        private int _loading;
        private StandaloneCodeEditor _scriptEditor, _styleEditor;
        private LibraryCollection _libraries;
        private TabMenu _tabs;
        private Settings _settings = new();
        private bool _viewMode = false;

        private const string StandardStyle = "/* #STDSTYLE# */";

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        protected string Filename
        {
            get => _settings.Filename;
            set
            {
                if (_settings.Filename != value)
                {
                    _settings.Filename = value;
                    Task.Run(SaveSettings);
                }
            }
        }
        
        /// <summary>
        /// Gets whether the output should be shrunk to fix the width of the output window.
        /// </summary>
        protected bool ShrinkX
        {
            get => _settings.ShrinkX;
            set
            {
                if (_settings.ShrinkX != value)
                {
                    _settings.ShrinkX = value;
                    Task.Run(SaveSettings);
                }
            }
        }

        /// <summary>
        /// Gets whether the output should be shrunk to fix the height of the output window.
        /// </summary>
        protected bool ShrinkY
        {
            get => _settings.ShrinkY;
            set
            {
                if (_settings.ShrinkY != value)
                {
                    _settings.ShrinkY = value;
                    Task.Run(SaveSettings);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the component bounds should be rendered.
        /// </summary>
        protected bool RenderBounds
        {
            get => _settings.RenderBounds;
            set
            {
                if (_settings.RenderBounds != value)
                {
                    _settings.RenderBounds = value;
                    Update();
                    Task.Run(SaveSettings);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether changes in the editor should cause an automatic update of the output.
        /// </summary>
        protected bool AutoUpdate
        {
            get => _settings.AutoUpdate;
            set
            {
                if (_settings.AutoUpdate != value)
                {
                    _settings.AutoUpdate = value;
                    if (value)
                    {
                        _timer.Start();
                        lock (_lock)
                            _updates = 0;
                        Task.Run(UpdateNow);
                    }
                    else
                        _timer.Stop();
                    Task.Run(SaveSettings);
                }
            }
        }

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                // Determine the version
                _simpleCircuitVersion = typeof(GraphicalCircuit).Assembly.GetName().Version?.ToString() ?? "?";

                // Register our own language keywords
                List<string[]> keys = new();
                var context = new ParsingContext();
                foreach (var factory in context.Factory.Factories)
                {
                    foreach (var metadata in factory.Metadata)
                    {
                        foreach (string key in metadata.Keys)
                            keys.Add(new string[] { key, metadata.Description });
                    }
                }
                await _js.InvokeVoidAsync("registerLanguage", new object[] { keys.ToArray() });
                var model = await _scriptEditor.GetModel();
                await Global.SetModelLanguage(_js, model, "simpleCircuit");
                await Global.SetTheme(_js, "simpleCircuitTheme");

                // Try to find a script and/or style from the query parameters
                var query = HttpUtility.ParseQueryString(new Uri(_navigation.Uri).Query);
                string script = query["script"], style = query["style"];
                if (string.IsNullOrWhiteSpace(script) && string.IsNullOrWhiteSpace(style))
                    await ReloadLastScript();
                else
                {
                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        try
                        {
                            // Decode the script as a base64 string
                            var bytes = Convert.FromBase64String(script);

                            // Use GZip decompression
                            using var inputStream = new MemoryStream(bytes);
                            using var outputStream = new MemoryStream();
                            using (System.IO.Compression.GZipStream gzip = new(inputStream, System.IO.Compression.CompressionMode.Decompress))
                            {
                                await gzip.CopyToAsync(outputStream);
                            }
                            script = DecodeScript(Encoding.UTF8.GetString(outputStream.ToArray()));
                        }
                        catch (Exception ex)
                        {
                            _logger.Messages.Add(new DiagnosticMessage(SeverityLevel.Error,
                                null, $"Loading script failed: {ex.Message}"));
                            script = Demo.Demos[0].Code;
                        }
                    }
                    else
                        script = Demo.Demos[0].Code;
                    if (!string.IsNullOrWhiteSpace(style))
                    {
                        try
                        {
                            // Decode the script as a base64 string
                            var bytes = Convert.FromBase64String(style);

                            // Use GZip decompression
                            using var inputStream = new MemoryStream(bytes);
                            using var outputStream = new MemoryStream();
                            using (System.IO.Compression.GZipStream gzip = new(inputStream,
                                System.IO.Compression.CompressionMode.Decompress))
                            {
                                await gzip.CopyToAsync(outputStream);
                            }
                            style = DecodeScript(Encoding.UTF8.GetString(outputStream.ToArray()));
                        }
                        catch (Exception ex)
                        {
                            _logger.Messages.Add(new DiagnosticMessage(SeverityLevel.Error,
                                null, $"Loading style failed: {ex.Message}"));
                            style = GraphicalCircuit.DefaultStyle;
                        }
                    }
                    else
                        style = GraphicalCircuit.DefaultStyle;
                    _viewMode = true;
                    await SetCurrentScript(new(script, style));

                    string url = new Uri(_navigation.Uri).GetLeftPart(UriPartial.Path);
                    _navigation.NavigateTo(url);
                }
                await LoadSettings();

                // Setup the timer
                _timer.Elapsed += OnTimerElapsed;
                if (_settings.AutoUpdate)
                    _timer.Start();

                // Apply the plitter
                await _js.InvokeVoidAsync("apply_splitter");
            }
        }

        /// <summary>
        /// Called when a new file is uploaded.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A task.</returns>
        protected async Task UploadFile(UploadEventArgs args)
        {
            _logger.Clear();
            if (args.Messages != null)
                _logger.Messages.AddRange(args.Messages);

            // Check the version of uploaded files
            if (args is UploadSvgEventArgs svgArgs)
            {
                if (!string.IsNullOrEmpty(svgArgs.Script) && (svgArgs.Version == null || StringComparer.Ordinal.Compare(svgArgs.Version, _simpleCircuitVersion) < 0))
                {
                    if (svgArgs.Version == null)
                    {
                        _logger.Messages.Add(
                            new DiagnosticMessage(SeverityLevel.Warning, null,
                            "The script and style were generated by an older version. If the circuit looks wrong, consider pressing the reset button in the Style tab."));
                    }
                    else
                    {
                        // Only check major and minor version
                        var m1 = MajorVersion().Match(svgArgs.Version);
                        var m2 = MajorVersion().Match(_simpleCircuitVersion);
                        if (!m1.Success || !m2.Success || StringComparer.Ordinal.Compare(m1.Value, m2.Value) < 0)
                        {
                            _logger.Messages.Add(
                                new DiagnosticMessage(SeverityLevel.Warning, null,
                                $"The script and style were generated by an older version ({svgArgs.Version}). If the circuit looks wrong, consider pressing the reset button in the Style tab."));
                        }
                    }
                }

                _viewMode = true;
                await SetCurrentScript(new(DecodeScript(svgArgs.Script), DecodeScript(svgArgs.Style)));
            }
            else if (args is UploadLibraryEventArgs libArgs)
            {
                _libraries.Add(libArgs.Filename, libArgs.Document);
            }
        }

        /// <summary>
        /// Called when a download request happens.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A task.</returns>
        protected async Task DownloadFile(DownloadEventArgs args)
        {
            // Clear the logger of error messages (we will be re-generating anyway)
            _logger.Clear();
            if (_viewMode)
                _logger.Messages.Add(new ViewModeDiagnosticMessage());

            // Decide on the filename
            string filename = Filename;
            if (string.IsNullOrWhiteSpace(filename))
                filename = "circuit";

            switch (args.Type)
            {
                case DownloadEventArgs.Types.Svg:
                    {
                        var doc = await ComputeXml(includeScript: true);
                        
                        string result;
                        using (var sw = new Utf8StringWriter())
                        using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                        {
                            doc.WriteTo(xml);
                            xml.Flush();
                            sw.Flush();
                            result = sw.ToString();
                        }
                        byte[] file = Encoding.UTF8.GetBytes(result);
                        await _js.InvokeVoidAsync("BlazorDownloadFile", $"{filename}.svg", "image/svg+xml;", file);
                    }
                    break;

                case DownloadEventArgs.Types.Png:
                    {
                        var doc = await ComputeXml(includeScript: true);

                        // Compute the width and height to compute the scale of the image
                        if (!double.TryParse(doc.DocumentElement.GetAttribute("width"), out double w))
                            w = 10.0;
                        if (!double.TryParse(doc.DocumentElement.GetAttribute("height"), out double h))
                            h = 10.0;

                        string result;
                        using (var sw = new StringWriter())
                        using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                        {
                            doc.WriteTo(xml);
                            xml.Flush();
                            sw.Flush();
                            result = sw.ToString();
                        }
                        string url = $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(result))}";
                        await _js.InvokeVoidAsync("BlazorExportImage", $"{filename}.png", "image/png", url, (int)w, (int)h);
                    }
                    break;

                case DownloadEventArgs.Types.Jpeg:
                    {
                        var doc = await ComputeXml(includeScript: true);

                        // Compute the width and height to compute the scale of the image
                        if (!double.TryParse(doc.DocumentElement.GetAttribute("width"), out double w))
                            w = 10.0;
                        if (!double.TryParse(doc.DocumentElement.GetAttribute("height"), out double h))
                            h = 10.0;

                        string result;
                        using (var sw = new StringWriter())
                        using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                        {
                            doc.WriteTo(xml);
                            xml.Flush();
                            sw.Flush();
                            result = sw.ToString();
                        }
                        string url = $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(result))}";
                        await _js.InvokeVoidAsync("BlazorExportImage", $"{filename}.jpg", "image/jpeg", url, (int)w, (int)h, "white");
                    }
                    break;

                case DownloadEventArgs.Types.Link:
                    {
                        var query = HttpUtility.ParseQueryString(string.Empty);

                        // Extract the script
                        string script = await _scriptEditor.GetValue();
                        script = script.Trim('\r', '\n', '\t', ' ');
                        if (!string.IsNullOrWhiteSpace(script))
                        {
                            using MemoryStream output = new();
                            using (System.IO.Compression.GZipStream gzip = new(output, System.IO.Compression.CompressionLevel.SmallestSize))
                            {
                                await gzip.WriteAsync(Encoding.UTF8.GetBytes(EncodeScript(script)));
                            }
                            query.Add("script", Convert.ToBase64String(output.ToArray()));
                        }

                        // Extract the style (we will try to replace the default style to reduce size)
                        string style = await _styleEditor.GetValue();
                        style = style.Replace(GraphicalCircuit.DefaultStyle, StandardStyle).Trim('\r', '\n', '\t', ' ');
                        if (!string.IsNullOrWhiteSpace(style) && !StringComparer.Ordinal.Equals(style, StandardStyle))
                        {
                            using MemoryStream output = new();
                            using (System.IO.Compression.GZipStream gzip = new(output, System.IO.Compression.CompressionLevel.SmallestSize))
                            {
                                await gzip.WriteAsync(Encoding.UTF8.GetBytes(EncodeScript(style)));
                            }
                            query.Add("style", Convert.ToBase64String(output.ToArray()));
                        }

                        // Build the URI
                        var b = new Uri(_navigation.Uri).GetLeftPart(UriPartial.Path);
                        var uri = $"{b}?{query}";
                        if (uri.Length <= 2048)
                        {
                            await _js.InvokeVoidAsync("copyToClipboard", uri);
                            _logger.Clear();
                            _logger.Messages.Add(new DiagnosticMessage(SeverityLevel.Info, null, "URL copied to clipboard"));
                        }
                        else
                        {
                            _logger.Clear();
                            _logger.Messages.Add(new DiagnosticMessage(SeverityLevel.Error, null, "The script is too large to encode as an URL! Share the SVG file instead"));
                        }
                        StateHasChanged();
                    }
                    break;

                default:
                    _logger.Messages.Add(new DiagnosticMessage(SeverityLevel.Error,
                        null, $"Could not recognize download format '{args.Type}'"));
                    break;
            }
        }

        private async Task ReloadLastScript()
        {
            if (_localStore != null)
            {
                string script = await _localStore.GetItemAsStringAsync("last_script");
                string style = await _localStore.GetItemAsStringAsync("last_style");
                _logger.Clear();
                _viewMode = false;

                // Allow loading an initial script if none was stored
                if (string.IsNullOrWhiteSpace(style))
                    style = GraphicalCircuit.DefaultStyle;
                if (string.IsNullOrWhiteSpace(script))
                    script = Demo.Demos[0].Code;

                await SetCurrentScript(new(script, style));
            }
        }
        private async Task ReportMessageClicked(Token token)
        {
            await _scriptEditor.SetPosition(new Position() { LineNumber = token.Location.Line, Column = token.Location.Column }, "warning");
            _tabs.Select(0);
            await _scriptEditor.Focus();
        }
        private async Task SetCurrentScript(Netlist netlist)
        {
            // Let us strip a few characters that might accumulate when storing inside XML for example
            string script = netlist.Script;
            string style = netlist.Style;
            if (script != null)
                script = script.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;
            if (style != null)
                style = style.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;

            // Update the script
            lock (_lock)
                _updates = int.MinValue; // This will avoid triggers to happen before having updated
            if (!string.IsNullOrWhiteSpace(script))
            {
                await _scriptEditor.SetValue(script);
                if (!string.IsNullOrWhiteSpace(style))
                    await _styleEditor.SetValue(style);
                await UpdateNow();
            }
            lock (_lock)
                _updates = 0;
        }
        private static StandaloneEditorConstructionOptions GetStyleOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "css",
                WordWrap = "off",
                Value = "",
                WordBasedSuggestions = false,
            };
        }
        private void Update()
        {
            if (_settings.AutoUpdate)
            {
                lock (_lock)
                {
                    _updates++;

                    // Something has changed
                    if (_updates > 0)
                        _loading = 1;
                }
            }
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                if (_updates == 1 && (_currentSolver == null || _currentSolver.IsCompleted))
                {
                    _updates = 0; // Restart tracking of updates made in the editor

                    // Updating happens asynchronously
                    _logger.Clear();
                    _viewMode = false;
                    _currentSolver = Task.Run(UpdateNow);
                }
                else if (_updates > 1)
                {
                    // More than one updates have been made, we should avoid regenerating
                    // the circuit because more updates are likely to happen
                    _updates = 1;
                }
            }
        }
        private async Task UpdateNow()
        {
            _loading = 2;
            _svg = await ComputeXml(false, _settings.RenderBounds);
            _loading = 0;
            StateHasChanged();
        }
        private async Task<XmlDocument> ComputeXml(bool includeScript, bool includeBounds = false)
        {
            XmlDocument doc = null;
            try
            {
                var code = await _scriptEditor.GetValue();
                var style = await _styleEditor.GetValue();
                var context = new ParsingContext
                {
                    Diagnostics = _logger
                };

                // Add the necessary libraries
                foreach (var library in _libraries.Libraries.Values)
                {
                    if (library.IsLoaded)
                        context.Factory.Load(library.Library, _logger);
                }

                if (!_viewMode)
                {
                    // Store the script and style for next time
                    await _localStore.SetItemAsStringAsync("last_script", code);
                    await _localStore.SetItemAsStringAsync("last_style", style);
                }
                else
                {
                    _logger.Messages.Add(new ViewModeDiagnosticMessage());
                }
                await _js.InvokeVoidAsync("updateStyle", ModifyCSS(style));

                // Parse the script
                var lexer = SimpleCircuitLexer.FromString(code.AsMemory());
                Parser.Parse(lexer, context);
                var ckt = context.Circuit;

                // Include XML data
                ckt.Style = style;
                ckt.RenderBounds = includeBounds;
                if (includeScript)
                {
                    ckt.Metadata.Add("script", EncodeScript(code));
                    if (_simpleCircuitVersion != null)
                        ckt.Metadata.Add("version", _simpleCircuitVersion);
                }

                // We now need the last things to have executed
                if (ckt.Count > 0 && _logger.Errors == 0)
                    doc = ckt.Render(_logger, _jsTextFormatter);
            }
            catch (Exception ex)
            {
                _logger.Post(new DiagnosticMessage(SeverityLevel.Error,
                    "Exception", ex.Message));
            }

            // Add our errors and warnings
            if (_logger.Errors == 0)
                return doc;
            else
                return null;
        }
        private static string EncodeScript(string script)
        {
            if (script is null)
                return script;

            // We can encounter arrows in our script, so let's encode them in HTML code
            script = NonUtf8Code().Replace(script, match => $"&#{(int)match.Groups[0].Value[0]};");
            return script;
        }
        private static string DecodeScript(string script)
        {
            if (script is null)
                return script;
            script = Utf8Encoded().Replace(script, match =>
            {
                // Convert the resulting ASCI character
                int value = int.Parse(match.Groups["value"].Value);
                return ((char)value).ToString();
            }).Replace(StandardStyle, GraphicalCircuit.DefaultStyle); // Put the default style back
            return script;
        }
        private static string ModifyCSS(string style)
        {
            int level = 0;
            StringBuilder sb = new(style.Length);
            bool start = true;
            for (int i = 0; i < style.Length; i++)
            {
                char c = style[i];
                switch (c)
                {
                    case '{': level++; break;
                    case '}': level--; start = true; break;
                    case ',': start = true; break;
                    default:
                        if (level == 0 && start && !(char.IsWhiteSpace(c) || c == '\r' || c == '\n'))
                        {
                            // This is a first character of a selector, so let's splice in our own selector here!
                            sb.Append(".simplecircuit ");
                            start = false;
                        }
                        break;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
        private async Task SaveSettings()
        {
            string settings = JsonSerializer.Serialize(_settings);
            await _localStore.SetItemAsStringAsync("settings", settings);
        }
        private async Task LoadSettings()
        {
            if (_localStore != null)
            {
                string settings = await _localStore.GetItemAsStringAsync("settings");

                // Load settings
                if (!string.IsNullOrWhiteSpace(settings))
                {
                    _settings = JsonSerializer.Deserialize<Settings>(settings);
                    StateHasChanged();
                }
            }
        }

        [GeneratedRegex("[\u0100-\uffff]")]
        private static partial Regex NonUtf8Code();
        [GeneratedRegex(@"\&\#(?<value>[0-9]+);")]
        private static partial Regex Utf8Encoded();
        [GeneratedRegex(@"^(\d+)\.(\d+)")]
        private static partial Regex MajorVersion();

        private void KeyUp()
        {
            // Extend the updates
            lock(_lock)
            {
                if (_updates > 0)
                    _updates++;
            }
        }
    }
}
