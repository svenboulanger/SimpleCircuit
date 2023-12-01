using Microsoft.AspNetCore.Components.Forms;
using SimpleCircuit.Components;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;

namespace SimpleCircuitOnline.Shared
{
    public partial class ComponentList
    {
        private string _filterString = string.Empty;
        private bool _expandAll = false;
        private readonly HashSet<string> _searchTerms = [];
        private readonly Dictionary<string, List<(DrawableMetadata, IDrawableFactory)>> _categories = [];

        private bool IsFiltered((DrawableMetadata Metadata, IDrawableFactory Factory) item)
        {
            int count = 0;
            foreach (var term in _searchTerms)
            {
                if (StringComparer.CurrentCultureIgnoreCase.Equals(item.Metadata.Key, term) ||
                    item.Metadata.Keywords.Contains(term) ||
                    item.Metadata.Description.Contains(term, StringComparison.CurrentCultureIgnoreCase))
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

        /// <summary>
        /// Updates the list with the given parsing context.
        /// </summary>
        /// <param name="context"></param>
        public void Update(ParsingContext context)
        {
            _categories.Clear();
            foreach (var pair in context.Factory.Factories)
            {
                // Let's add the metadata and a component of it for each category
                var metadata = pair.Value.GetMetadata(pair.Key);
                if (!_categories.TryGetValue(metadata.Category, out var list))
                {
                    list = [];
                    _categories.Add(metadata.Category, list);
                }

                // Add our description
                list.Add((metadata, pair.Value));
            }
            StateHasChanged();
        }
    }
}
