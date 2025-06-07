using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleCircuitOnline.Shared
{
    public partial class DemoList
    {
        private Dictionary<string, List<Demo>> _demos = [];

        /// <inheritdoc />
        protected override Task OnInitializedAsync()
        {
            _demos.Clear();
            foreach (var demo in Demo.Demos)
            {
                if (!_demos.TryGetValue(demo.Category, out var list))
                {
                    list = [];
                    _demos.Add(demo.Category, list);
                }
                list.Add(demo);
            }
            return base.OnInitializedAsync();
        }
    }
}
