using Microsoft.AspNetCore.Components.Forms;
using SimpleCircuit;
using SimpleCircuit.Components;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCircuitOnline.Shared
{
    public partial class ComponentList
    {
        private string _filterString = string.Empty;
        private bool _expandAll = false;
        private readonly HashSet<string> _searchTerms = new();
        private Dictionary<string, List<(DrawableMetadata, IDrawable, HashSet<string>)>> _categories = new();

        private bool IsFiltered((DrawableMetadata Metadata, IDrawable Drawable, HashSet<string> Variants) argument)
        {
            int count = 0;
            foreach (var term in _searchTerms)
            {
                if (argument.Metadata.Description.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                    argument.Metadata.Keys.Any(key => key.Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
                    argument.Variants.Contains(term))
                    count++;
            }
            return count == _searchTerms.Count;
        }

        private void UpdateFilter(EditContext context)
        {
            _searchTerms.Clear();
            if (string.IsNullOrWhiteSpace(_filterString))
            {
                _expandAll = false;
            }
            else
            {
                foreach (string term in _filterString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    _searchTerms.Add(term);
                _expandAll = true;
            }
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Find all the different components
            var context = new ParsingContext();
            var drawing = new SvgDrawing();
            foreach (var factory in context.Factory.Factories)
            {
                foreach (var metadata in factory.Metadata)
                {
                    // Let's add the metadata and a component of it for each category
                    foreach (string category in metadata.Categories)
                    {
                        if (!_categories.TryGetValue(category, out var list))
                        {
                            list = new();
                            _categories.Add(category, list);
                        }

                        // Add our description
                        var drawable = factory.Create(metadata.Keys[0], metadata.Keys[0], context.Options, context.Diagnostics);
                        drawable.Render(drawing);
                        var variants = drawable.Variants.Branches.ToHashSet(StringComparer.OrdinalIgnoreCase);
                        list.Add((metadata, drawable, variants));
                    }
                }
            }
        }
    }
}
