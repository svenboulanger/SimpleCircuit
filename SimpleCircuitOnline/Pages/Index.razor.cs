using BlazorMonaco.Editor;
using Microsoft.JSInterop;
using SimpleCircuit;
using SimpleCircuit.Diagnostics;
using SimpleCircuit.Drawing.Styles;
using SimpleCircuit.Evaluator;
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

namespace SimpleCircuitOnline.Pages;

public partial class Index
{
    private Task _currentSolver = null;
    private readonly Logger _logger = new();
    private string _simpleCircuitVersion;
    private List<(string Theme, XmlDocument Document, string Background)> _svg = null;
    private readonly Timer _timer = new(750) { Enabled = false, AutoReset = true };
    private readonly System.Threading.Lock _lock = new();
    private int _updates = 0;
    private int _loading;

    private StandaloneCodeEditor _scriptEditor;
    private LibraryCollection _libraries;
    private TabMenu _tabs;
    private Settings _settings = new();
    private bool _viewMode = false;
    private ComponentList _componentList;

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

            // Update documentation
            await _libraries.LoadLibraries();
            var context = _libraries.BuildContext(_logger);
            await UpdateKeywords(context);
            _componentList.Update(context);

            var model = await _scriptEditor.GetModel();
            await Global.SetModelLanguage(_js, model, "simpleCircuit");
            await Global.SetTheme(_js, "simpleCircuitTheme");

            // Try to find a script from the query parameters
            var query = HttpUtility.ParseQueryString(new Uri(_navigation.Uri).Query);
            string script = query["script"];
            if (string.IsNullOrWhiteSpace(script))
                await ReloadLastScript();
            else
            {
                if (!string.IsNullOrWhiteSpace(script))
                {
                    try
                    {
                        // Decode the script as a base64 string
                        byte[] bytes = Convert.FromBase64String(script);

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
                        script = DemoCategory.MainDemo.Code;
                    }
                }
                else
                    script = DemoCategory.MainDemo.Code;
                _viewMode = true;
                await SetCurrentScript(script);

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
            bool tryFixScript = false;
            if (!string.IsNullOrEmpty(svgArgs.Script) &&
                (svgArgs.Version == null || StringComparer.Ordinal.Compare(svgArgs.Version, _simpleCircuitVersion) < 0))
            {
                if (svgArgs.Version == null)
                {
                    _logger.Messages.Add(
                        new DiagnosticMessage(SeverityLevel.Warning, null,
                        "The script was generated by an older version. Some changes to the script may be necessary."));
                    tryFixScript = true;
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
                            $"The script was generated by an older version ({svgArgs.Version}). Some changes to the script may be necessary."));
                        tryFixScript = true;
                    }
                }
            }

            // Attempt to fix the script if required
            string script = svgArgs.Script;
            if (tryFixScript)
            {
                // Replace legacy property assignments
                var regex = LegacyProperty();
                bool modified = false;
                script = regex.Replace(script, match =>
                {
                    modified = true;
                    return $"{match.Groups["name"].Value}({match.Groups["property"].Value}={match.Groups["value"].Value})";
                });

                // Give a warning message if we made changes to the script
                if (modified)
                {
                    _logger.Messages.Add(
                        new DiagnosticMessage(SeverityLevel.Warning, null,
                        $"The script was modified for backward compatibility."));
                }
            }
            _viewMode = true;
            await SetCurrentScript(DecodeScript(script));
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
                    async void ExportSvg(string exportFilename, XmlDocument doc)
                    {
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
                        await _js.InvokeVoidAsync("BlazorDownloadFile", exportFilename, "image/svg+xml;", file);
                    }

                    // Build the circuit
                    var docs = await ComputeXml(includeScript: true);

                    // Export
                    foreach (var (theme, doc, _) in docs)
                    {
                        string exportFilename = docs.Count > 1 ? $"{filename}_{theme}.svg" : $"{filename}.svg";
                        ExportSvg(exportFilename, doc);
                    }
                }
                break;

            case DownloadEventArgs.Types.Png:
                {
                    async void ExportPng(string exportFilename, XmlDocument doc)
                    {
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
                        await _js.InvokeVoidAsync("BlazorExportImage", exportFilename, "image/png", url, (int)w, (int)h);
                    }

                    // Build the circuit
                    var docs = await ComputeXml(includeScript: true);

                    // Export
                    foreach (var (theme, doc, _) in docs)
                    {
                        string exportFilename = docs.Count > 1 ? $"{filename}_{theme}.png" : $"{filename}.png";
                        ExportPng(exportFilename, doc);
                    }
                }
                break;

            case DownloadEventArgs.Types.Jpeg:
                {
                    async void ExportJpg(string exportFilename, XmlDocument doc, string backgroundColor)
                    {
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
                        await _js.InvokeVoidAsync("BlazorExportImage", exportFilename, "image/jpeg", url, (int)w, (int)h, backgroundColor);
                    }

                    // Build the circuit
                    var docs = await ComputeXml(includeScript: true);

                    // Export
                    foreach (var (theme, doc, background) in docs)
                    {
                        string exportFilename = docs.Count > 1 ? $"{filename}_{theme}.jpg" : $"{filename}.jpg";
                        ExportJpg(exportFilename, doc, background);
                    }
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

                    // Build the URI
                    string b = new Uri(_navigation.Uri).GetLeftPart(UriPartial.Path);
                    string uri = $"{b}?{query}";
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
            _logger.Clear();
            _viewMode = false;

            // Allow loading an initial script if none was stored
            if (string.IsNullOrWhiteSpace(script))
                script = DemoCategory.MainDemo.Code;

            await SetCurrentScript(script);
        }
    }
    private async Task ReportMessageClicked(TextLocation location)
    {
        await _scriptEditor.SetPosition(new Position() { LineNumber = location.Line, Column = location.Column }, "warning");
        _tabs.Select(0);
        await _scriptEditor.Focus();
    }
    private async Task SetCurrentScript(string script)
    {
        // Let us strip a few characters that might accumulate when storing inside XML for example
        if (script != null)
            script = script.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;

        // Update the script
        lock (_lock)
            _updates = int.MinValue; // This will avoid triggers to happen before having updated
        if (!string.IsNullOrWhiteSpace(script))
        {
            await _scriptEditor.SetValue(script);
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
        _svg = await ComputeXml(false);
        _loading = 0;
        StateHasChanged();
    }
    private async Task<List<(string Theme, XmlDocument Document, string Background)>> ComputeXml(bool includeScript)
    {
        List<(string Theme, XmlDocument Document, string Background)> result = [];
        try
        {
            string code = await _scriptEditor.GetValue();

            if (!_viewMode)
            {
                // Store the script for next time
                await _localStore.SetItemAsStringAsync("last_script", code);
            }
            else
            {
                _logger.Messages.Add(new ViewModeDiagnosticMessage());
            }

            // Parse the script
            var context = _libraries.BuildContext(_logger);
            var (ckt, themes) = _libraries.BuildCircuit(code, "editor", new(), context);
            if (ckt is not null)
            {
                // Include XML data
                if (includeScript)
                {
                    ckt.Metadata.Add("script", EncodeScript(code));
                    if (_simpleCircuitVersion != null)
                        ckt.Metadata.Add("version", _simpleCircuitVersion);
                }

                // We now need the last things to have executed
                if (ckt.Count > 0 && _logger.Errors == 0)
                {
                    if (themes.Count == 0)
                        themes.Add("light", Style.DefaultThemes["light"]);
                    foreach (var theme in themes)
                    {
                        if (ckt.Style is Style style)
                        {
                            style.Variables.Clear();
                            foreach (var pair in theme.Value)
                                style.Variables[pair.Key] = pair.Value;
                        }
                        ckt.Metadata["theme"] = theme.Key;
                        if (!theme.Value.TryGetValue(Style.OpaqueBackground, out var bgColor))
                            bgColor = "white";
                        result.Add((theme.Key, ckt.Render(_logger), bgColor));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Post(new DiagnosticMessage(SeverityLevel.Error, "Exception", ex.Message));
        }

        // Add our errors and warnings
        if (_logger.Errors == 0)
            return result;
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
        });
        return script;
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
    private async Task UpdateKeywords(EvaluationContext context)
    {
        List<string[]> keys = [];
        foreach (var pair in context.Factory.Factories)
        {
            keys.Add([pair.Key, pair.Value.GetMetadata(pair.Key)?.Description ?? "?"]);
        }
        await _js.InvokeVoidAsync("registerLanguage", [.. keys]);
    }
    private async Task LibrariesUpdated()
    {
        // Update documentation
        _componentList.Update(_libraries.BuildContext(null));

        // Reuse the context for updating
        _logger.Clear();
        await UpdateNow();
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

    [GeneratedRegex(@"^[\t\s]*-[\t\s]*(?<name>\w+)\.(?<property>[^\s\t\=]+)[\t\s]*=[\t\s]*(?<value>[^\r\n]+)$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex LegacyProperty();
}
