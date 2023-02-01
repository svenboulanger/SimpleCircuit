using BlazorMonaco;
using Microsoft.JSInterop;
using SimpleCircuit;
using SimpleCircuitOnline.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private bool _updateDynamic = false;
        private bool _shrinkX = true, _shrinkY = true, _renderBounds = true;
        private string _filename = null;

        private async Task SetCurrentScript(string script, string style = null)
        {
            // Let us strip a few characters that might accumulate when storing inside XML for example
            if (script != null)
                script = script.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;
            if (style != null)
                style = style.Trim(' ', '\t', '\r', '\n') + Environment.NewLine;

            // Temporarily suspend any dynamic updates
            _updateDynamic = false;
            Update(null);

            // Update the script
            if (!string.IsNullOrWhiteSpace(script))
            {
                await _scriptEditor.SetValue(script);
                if (!string.IsNullOrWhiteSpace(style))
                    await _styleEditor.SetValue(style);

                // Update the preview
                _svg = await ComputeXml(false, _renderBounds);
                _loading = 0;
                StateHasChanged();
            }

            // Enable dynamic updates again
            _updateDynamic = true;
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

        private void Update(ModelContentChangedEvent e)
        {
            if (_updateDynamic)
            {
                _timer.Stop();
                _timer.Start();
                if (_loading == 0)
                    _loading = 1;
            }
            else
            {
                // Just stop
                _loading = 0;
                _timer.Stop();
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
                bool hasScript = false;
                if (_localStore != null)
                {
                    string script = await _localStore.GetItemAsStringAsync("last_script");
                    string style = await _localStore.GetItemAsStringAsync("last_style");
                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        if (!string.IsNullOrWhiteSpace(style))
                            await SetCurrentScript(script, style);
                        else
                            await SetCurrentScript(script, GraphicalCircuit.DefaultStyle);
                        hasScript = true;
                    }
                }

                // Give the user an initial demo
                if (!hasScript)
                    await SetCurrentScript(Demo.Demos[0].Code, GraphicalCircuit.DefaultStyle);
            }
        }

        protected async Task UploadFile(UploadEventArgs args)
        {
            _errors = args.Errors;
            _warnings = args.Warnings;
            await SetCurrentScript(args.Script, args.Style);
        }

        public async Task DownloadFile(DownloadEventArgs args)
        {
            _errors = null;
            _warnings = null;

            // Decide on the filename
            string filename = _filename;
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

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // If another task is still running, restart the timer
            if (_loading > 1)
            {
                _timer.Stop();
                _timer.Start();
            }
            else
            {
                _loading = 2;
                _errors = null;
                _warnings = null;
                StateHasChanged();
                _timer.Stop();
                Task.Run(async () => _svg = await ComputeXml(false, _renderBounds))
                    .ContinueWith(task => { _loading = 0; StateHasChanged(); });
            }
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
                var lexer = SimpleCircuit.Parser.SimpleCircuitLexer.FromString(code);
                SimpleCircuit.Parser.Parser.Parse(lexer, context);
                var ckt = context.Circuit;

                // Include XML data
                ckt.Style = style;
                ckt.RenderBounds = includeBounds;
                if (includeScript)
                    ckt.Metadata.Add("script", code);

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
    }
}
