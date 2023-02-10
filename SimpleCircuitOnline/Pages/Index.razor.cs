using BlazorMonaco;
using Microsoft.JSInterop;
using SimpleCircuit;
using SimpleCircuitOnline.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace SimpleCircuitOnline.Pages
{
    public partial class Index
    {
        private string _errors, _warnings;
        private XmlDocument _svg;
        private Timer _timer;
        private int _loading;
        private MonacoEditor _scriptEditor, _styleEditor;
        private Settings _settings = new();

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
        protected bool RenderBounds
        {
            get => _settings.RenderBounds;
            set
            {
                if (_settings.RenderBounds != value)
                {
                    _settings.RenderBounds = value;
                    Task.Run(() => Update());
                    Task.Run(SaveSettings);
                }
            }
        }
        protected bool CurrentAutoUpdate
        {
            get => _settings.AutoUpdate;
            set
            {
                if (_settings.AutoUpdate != value)
                {
                    _settings.AutoUpdate = value;
                    if (value)
                        Task.Run(UpdateNow);
                    Task.Run(SaveSettings);
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                // Create a timer that will determine how quickly the preview updates after changes
                _timer = new Timer(750);
                _timer.Elapsed += OnTimerElapsed;
                _timer.AutoReset = false;

                // Register our own language keywords
                List<string[]> keys = new();
                var context = new SimpleCircuit.Parser.ParsingContext();
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
                await MonacoEditorBase.SetModelLanguage(model, "simpleCircuit");
                await MonacoEditorBase.SetTheme("simpleCircuitTheme");

                // Try to find the last saved script
                string script = null, style = null;
                if (_localStore != null)
                {
                    script = await _localStore.GetItemAsStringAsync("last_script");
                    style = await _localStore.GetItemAsStringAsync("last_style");
                    string settings = await _localStore.GetItemAsStringAsync("settings");

                    // Load settings
                    if (!string.IsNullOrWhiteSpace(settings))
                    {
                        _settings = JsonSerializer.Deserialize<Settings>(settings);
                        StateHasChanged();
                    }
                }
                if (string.IsNullOrWhiteSpace(script))
                    script = Demo.Demos[0].Code;
                if (string.IsNullOrWhiteSpace(style))
                    style = GraphicalCircuit.DefaultStyle;
                await SetCurrentScript(script, style);
            }
        }

        protected async Task UploadFile(UploadEventArgs args)
        {
            _errors = args.Errors;
            _warnings = args.Warnings;
            await SetCurrentScript(DecodeScript(args.Script), args.Style);
        }

        protected async Task DownloadFile(DownloadEventArgs args)
        {
            _errors = null;
            _warnings = null;

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

                default:
                    Console.WriteLine("Unrecognized download format");
                    break;
            }
        }

        private async Task SetCurrentScript(string script, string style = null)
        {
            // Let us strip a few characters that might accumulate when storing inside XML for example
            if (script != null)
                script = script.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;
            if (style != null)
                style = style.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;

            // Temporarily suspend any dynamic updates
            _timer.Stop();

            // Update the script
            if (!string.IsNullOrWhiteSpace(script))
            {
                await _scriptEditor.SetValue(script);
                if (!string.IsNullOrWhiteSpace(style))
                    await _styleEditor.SetValue(style);
                UpdateNow();
            }
        }
        private static StandaloneEditorConstructionOptions GetStyleOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "text/css",
                WordWrap = "on",
                Value = "",
                WordBasedSuggestions = false,
            };
        }
        private void Update()
        {
            if (_settings.AutoUpdate)
            {
                _timer.Stop();
                _timer.Start();
                if (_loading == 0)
                    _loading = 1;
            }
            else
                _timer.Stop();
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // If another SVG is still rendering, wait for another rerender
            if (_loading > 1)
            {
                _timer.Stop();
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                Task.Run(UpdateNow);
            }
        }
        
        private void UpdateNow()
        {
            // Notify for running the script
            _errors = null;
            _warnings = null;
            _loading = 2;

            // Actual loading
            Task.Run(async () => _svg = await ComputeXml(false, _settings.RenderBounds)).ContinueWith(task => { _loading = 0; StateHasChanged(); });
        }
        private async Task<XmlDocument> ComputeXml(bool includeScript, bool includeBounds = false)
        {
            _errors = null;
            _warnings = null;
            var logger = new Logger();
            XmlDocument doc = null;
            try
            {
                var code = await _scriptEditor.GetValue();
                var style = await _styleEditor.GetValue();
                var context = new SimpleCircuit.Parser.ParsingContext
                {
                    Diagnostics = logger
                };

                // Store the script and style for next time
                await _localStore.SetItemAsStringAsync("last_script", code);
                await _localStore.SetItemAsStringAsync("last_style", style);
                await _js.InvokeVoidAsync("updateStyle", ModifyCSS(style));

                // Parse the script
                var lexer = SimpleCircuit.Parser.SimpleCircuitLexer.FromString(code.AsMemory());
                SimpleCircuit.Parser.Parser.Parse(lexer, context);
                var ckt = context.Circuit;

                // Include XML data
                ckt.Style = style;
                ckt.RenderBounds = includeBounds;
                if (includeScript)
                    ckt.Metadata.Add("script", EncodeScript(code));

                // We now need the last things to have executed
                if (ckt.Count > 0 && logger.ErrorCount == 0)
                    doc = ckt.Render(logger, _jsTextFormatter);
            }
            catch (Exception ex)
            {
                logger.Post(new SimpleCircuit.Diagnostics.DiagnosticMessage(SimpleCircuit.Diagnostics.SeverityLevel.Error,
                    "Exception", ex.Message));
            }

            // Add our errors and warnings
            string errors = logger.Error.ToString();
            if (!string.IsNullOrWhiteSpace(errors))
                _errors = (_errors == null) ? errors : _errors + Environment.NewLine + errors;
            string warnings = logger.Warning.ToString();
            if (!string.IsNullOrWhiteSpace(warnings))
                _warnings = (_warnings == null) ? warnings : _warnings + Environment.NewLine + warnings;
            if (logger.ErrorCount == 0)
                return doc;
            else
                return null;
        }
        private static string EncodeScript(string script)
        {
            // We can encounter arrows in our script, so let's encode them in HTML code
            script = NonUtf8Code().Replace(script, match => $"&#{(int)match.Groups[0].Value[0]};");
            return script;
        }
        private static string DecodeScript(string script)
        {
            script = Utf8Encoded().Replace(script, match =>
            {
                // Convert the resulting ASCI character
                int value = int.Parse(match.Groups["value"].Value);
                return ((char)value).ToString();
            });
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

        [GeneratedRegex("[\u0100-\uffff]")]
        private static partial Regex NonUtf8Code();
        [GeneratedRegex("\\&\\#(?<value>[0-9]+);")]
        private static partial Regex Utf8Encoded();

        private bool _arrowMode = false;
        private void KeyDown(KeyboardEvent e)
        {
            if (!_arrowMode)
            {
                if (e.CtrlKey && e.KeyCode == KeyCode.US_DOT)
                    _arrowMode = true;
            }
            else
            {
                if (e.KeyCode == KeyCode.Escape)
                    _arrowMode = false;
            }
        }
        private async Task KeyUp(KeyboardEvent e)
        {
            if (_arrowMode)
            {
                // Replace the last character
                string insertCharacter = null;
                switch (e.KeyCode)
                {
                    case KeyCode.Ctrl:
                    case KeyCode.Shift:
                    case KeyCode.Alt:
                    case KeyCode.US_DOT:
                        break;

                    case KeyCode.KEY_L:
                    case KeyCode.KEY_W:
                    case KeyCode.NUMPAD_4:
                        insertCharacter = "\u2190";
                        break;

                    case KeyCode.KEY_U:
                    case KeyCode.KEY_N:
                    case KeyCode.NUMPAD_8:
                        insertCharacter = "\u2191";
                        break;

                    case KeyCode.KEY_R:
                    case KeyCode.KEY_E:
                    case KeyCode.NUMPAD_6:
                        insertCharacter = "\u2192";
                        break;

                    case KeyCode.KEY_D:
                    case KeyCode.KEY_S:
                    case KeyCode.NUMPAD_2:
                        insertCharacter = "\u2193";
                        break;

                    case KeyCode.NUMPAD_7:
                        insertCharacter = "\u2196";
                        break;

                    case KeyCode.NUMPAD_9:
                        insertCharacter = "\u2197";
                        break;

                    case KeyCode.NUMPAD_3:
                        insertCharacter = "\u2198";
                        break;

                    case KeyCode.NUMPAD_1:
                        insertCharacter = "\u2199";
                        break;

                    default:
                        // Break off arrow mode and return to regular behavior
                        _arrowMode = false;
                        return;
                }

                // Insert the text character instead of the key character
                if (insertCharacter != null)
                {
                    var selection = await _scriptEditor.GetSelection();
                    var model = await _scriptEditor.GetModel();
                    List<IdentifiedSingleEditOperation> edits = new()
                    {
                        new IdentifiedSingleEditOperation()
                        {
                            ForceMoveMarkers = true,
                            Range = new Selection()
                            {
                                StartColumn = selection.StartColumn - 1,
                                EndColumn = selection.EndColumn,
                                StartLineNumber = selection.StartLineNumber,
                                EndLineNumber = selection.EndLineNumber
                            },
                            Text = insertCharacter
                        }
                    };
                    await model.ApplyEdits(edits);
                }
            }
        }
    }
}
