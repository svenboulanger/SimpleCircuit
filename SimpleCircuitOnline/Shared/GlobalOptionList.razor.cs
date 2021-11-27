using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using SimpleCircuit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleCircuitOnline.Shared
{
    public partial class GlobalOptionList
    {
        private List<(PropertyInfo, string)> _options = new();

        [Parameter]
        public MonacoEditor Editor { get; set; }

        protected async Task Insert(string name)
        {
            if (Editor == null)
                return;
            var selection = await Editor.GetSelection();
            selection.StartColumn = 0;
            selection.EndColumn = 0;
            selection.EndLineNumber = selection.StartLineNumber;
            List<IdentifiedSingleEditOperation> ops = new();
            List<Selection> ends = new();

            string cmd = $".options {name} = ";
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
                var attribute = property.GetCustomAttribute<DescriptionAttribute>(true);
                _options.Add((property, attribute.Description));
            }
        }

        private string GetTypeName(PropertyInfo property)
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
