using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.Orchestration;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace MSFSPopoutPanelManager.MainApp.ViewModel
{
    public class PanelConfigFieldViewModel : BaseViewModel
    {
        public PanelConfig DataItem { get; set; }

        public string BindingPath { get; set; }

        public RoutedEvent SourceUpdatedEvent { get; set; }

        public DelegateCommand<object> PlusMinusCommand { get; set; }

        public ICommand DataUpdatedCommand { get; set; }

        public PanelConfigFieldViewModel(MainOrchestrator orchestrator) : base(orchestrator)
        {
            PlusMinusCommand = new DelegateCommand<object>(OnPlusMinus);
            DataUpdatedCommand = new DelegateCommand(OnDataUpdated);
        }

        private void OnPlusMinus(object param)
        {
            if (DataItem == null || BindingPath == null || param == null)
                return;

            var bindingPathProperty = typeof(PanelConfig).GetProperty(BindingPath);

            if (bindingPathProperty != null)
            {
                var value = Convert.ToInt32(bindingPathProperty.GetValue(DataItem, null));
                bindingPathProperty.SetValue(DataItem, Convert.ToInt32(param) + value);

                OnDataUpdated();
            }
        }

        private void OnDataUpdated()
        {
            if (DataItem != null)
                Orchestrator.PanelConfiguration.PanelConfigPropertyUpdated(DataItem.PanelHandle, (PanelConfigPropertyName)Enum.Parse(typeof(PanelConfigPropertyName), BindingPath));
        }
    }
}
