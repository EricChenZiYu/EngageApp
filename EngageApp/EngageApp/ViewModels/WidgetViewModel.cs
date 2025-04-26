using Prism.Mvvm;
using Prism.Events;
using EngageApp.Core.Events;

namespace EngageApp.ViewModels
{
    public class WidgetViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public WidgetViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
    }
} 