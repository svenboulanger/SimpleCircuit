using Microsoft.AspNetCore.Components.Forms;
using SimpleCircuit;
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
        private Dictionary<string, List<Utility.ComponentDescription>> _categories = new Dictionary<string, List<Utility.ComponentDescription>>();

        private bool IsFiltered(Utility.ComponentDescription description)
        {
            int count = 0;
            foreach (var term in _searchTerms)
            {
                if (description.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                    description.Key.Contains(term, StringComparison.CurrentCultureIgnoreCase))
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

            // Find the categories
            foreach (var description in Utility.Components(typeof(SimpleCircuit.Parser.Parser).Assembly).OrderBy(d => d.Key))
            {
                string category = description.Category ?? "General";
                if (!_categories.TryGetValue(category, out var list))
                {
                    list = new List<Utility.ComponentDescription>();
                    _categories.Add(category, list);
                }
                list.Add(description);
            }
        }
    }
}
