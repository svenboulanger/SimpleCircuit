﻿using BlazorMonaco;
using Microsoft.JSInterop;
using SimpleCircuit;
using SimpleCircuitOnline.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private Task _task = null;
        private int _loading;
        private MonacoEditor _scriptEditor, _styleEditor;
        private TextFormatter _jsTextFormatter;
        private bool _updateDynamic = false;

        private async Task SetCurrentScript(string script, string style = null)
        {
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
                await RenderPreview();
                _loading = 0;
                StateHasChanged();
            }

            // Enable dynamic updates again
            _updateDynamic = true;
        }

        private StandaloneEditorConstructionOptions GetStyleOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Language = "text/css",
                WordWrap = "on",
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

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // This text formatter will invoke a JavaScript method that uses getBBox() to measure a text size
            _jsTextFormatter = new TextFormatter(_js);
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
                List<string> keys = new();
                foreach (var component in Utility.Components(typeof(Utility).Assembly))
                    keys.Add(component.Key);
                await _js.InvokeVoidAsync("registerLanguage", new object[] { keys.ToArray() });
                var model = await _scriptEditor.GetModel();
                await MonacoEditor.SetModelLanguage(model, "simpleCircuit");
                await MonacoEditorBase.SetTheme("simpleCircuitTheme");

                // Give the user an initial demo
                await SetCurrentScript(Demo.Demos[0].Code, GraphicalCircuit.DefaultStyle);
            }
        }

        protected async Task UploadFile(DropZone.UploadEventArgs args)
        {
            _errors = args.Errors;
            _warnings = args.Warnings;
            await SetCurrentScript(args.Script, args.Style);
        }

        public async Task DownloadFile()
        {
            _errors = null;
            _warnings = null;
            var doc = await ComputeXml(includeScript: true);
            using var sw = new StringWriter();
            using (var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false }))
                doc.WriteTo(xml);
            byte[] file = System.Text.Encoding.UTF8.GetBytes(sw.ToString());
            await _js.InvokeVoidAsync("BlazorDownloadFile", "circuit.svg", "text/plain", file);
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
                _task =
                    Task.Run(RenderPreview)
                    .ContinueWith(task => { _loading = 0; StateHasChanged(); });
            }
        }

        private async Task RenderPreview()
        {
            _svg = await ComputeXml(includeScript: false);
        }

        private async Task<XmlDocument> ComputeXml(bool includeScript)
        {
            _errors = null;
            _warnings = null;
            var logger = new Logger();
            XmlDocument doc = null;
            try
            {

                var code = await _scriptEditor.GetValue();
                var context = new SimpleCircuit.Parser.ParsingContext();
                context.Diagnostics = logger;

                // Parse the script
                var lexer = new SimpleCircuit.Parser.Lexer(code);
                SimpleCircuit.Parser.Parser.Parse(lexer, context);
                var ckt = context.Circuit;

                // Include XML data
                ckt.Style = await _styleEditor.GetValue();
                if (includeScript)
                    ckt.Metadata.Add("script", code);

                // Share it with the rest
                ((IJSInProcessRuntime)_js).InvokeVoid("updateStyle", ModifyCSS(ckt.Style));

                if (ckt.Count > 0)
                {
                    doc = ckt.Render(logger, _jsTextFormatter);
                }
            }
            catch (Exception ex)
            {
                logger.Post(new SimpleCircuit.Diagnostics.DiagnosticMessage(SimpleCircuit.Diagnostics.SeverityLevel.Error,
                    "?", ex.Message));
            }

            // Add our errors and warnings
            string errors = logger.Error.ToString();
            if (!string.IsNullOrWhiteSpace(errors))
                _errors = (_errors == null) ? errors : _errors + Environment.NewLine + errors;
            string warnings = logger.Warning.ToString();
            if (!string.IsNullOrWhiteSpace(warnings))
                _warnings = (_warnings == null) ? warnings : _warnings + Environment.NewLine + warnings;
            return doc;
        }

        private string ModifyCSS(string style)
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