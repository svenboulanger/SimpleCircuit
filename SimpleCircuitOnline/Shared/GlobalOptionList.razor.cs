using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using SimpleCircuit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Selection = BlazorMonaco.Selection;

namespace SimpleCircuitOnline.Shared
{
    public partial class GlobalOptionList
    {
        private readonly List<(PropertyInfo, string, object)> _options = [];
        private readonly Options _defaultOptions = new();

        [Parameter]
        public StandaloneCodeEditor Editor { get; set; }

        protected async Task Insert(string name, object defValue)
        {
            if (Editor == null)
                return;
            var selection = await Editor.GetSelection();
            selection.StartColumn = 0;
            selection.EndColumn = 0;
            selection.EndLineNumber = selection.StartLineNumber;
            List<IdentifiedSingleEditOperation> ops = [];
            List<Selection> ends = [];

            string strDefValue;
            if (defValue is bool b)
                strDefValue = b ? "true" : "false";
            else if (defValue is string str)
                strDefValue = $"\"{str}\"";
            else
                strDefValue = defValue?.ToString() ?? "?";

            string cmd = $".options {name} = {strDefValue}";
            ops.Add(new()
            {
                Range = selection,
                Text = cmd + Environment.NewLine,
                ForceMoveMarkers = true,
            });
            ends.Add(new Selection()
            {
                StartLineNumber = selection.StartLineNumber,
                EndLineNumber = selection.StartLineNumber,
                StartColumn = cmd.Length,
                EndColumn = cmd.Length
            });
            await Editor.ExecuteEdits("code", ops, ends);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Find the categories
            foreach (var property in typeof(Options).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.CanWrite && property.CanRead)
                {
                    var attribute = property.GetCustomAttribute<DescriptionAttribute>(true);
                    if (attribute != null)
                    {
                        var defValue = property.GetValue(_defaultOptions);
                        _options.Add((property, attribute.Description, defValue));
                    }
                }
            }
        }

        private static string GetTypeName(PropertyInfo property)
        {
            string typeName = "?";
            if (property.PropertyType == typeof(double))
                typeName = "number";
            else if (property.PropertyType == typeof(bool))
                typeName = "boolean";
            else if (property.PropertyType == typeof(string))
                typeName = "string";
            return typeName;
        }
    }
}
