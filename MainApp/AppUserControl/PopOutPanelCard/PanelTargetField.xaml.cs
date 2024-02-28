using MSFSPopoutPanelManager.DomainModel.Profile;
using MSFSPopoutPanelManager.MainApp.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.MainApp.AppUserControl.PopOutPanelCard
{
    /// <summary>
    /// Interaction logic for PanelTargetField.xaml
    /// </summary>
    public partial class PanelTargetField
    {
        public PanelTargetField()
        {
            InitializeComponent();
            this.Loaded += PanelTargetField_Loaded;
        }

        private void PanelTargetField_Loaded(object sender, RoutedEventArgs e)
        {
            var dataContext = ((PopOutPanelSourceCardViewModel) this.DataContext);

            if (dataContext == null)
                return;
            
            dataContext.FixedCameraConfigs.CollectionChanged += (_, _) =>
            {
                var items = dataContext.FixedCameraConfigs;

                this.ComboBoxCameraSelection.ItemsSource = items;
                this.ComboBoxCameraSelection.DisplayMemberPath = "Name";

                var index = items.ToList().FindIndex(x => x.Id == dataContext.DataItem.FixedCameraConfig.Id);

                if (index == -1)
                    return;
                
                this.ComboBoxCameraSelection.SelectedIndex = index;
            };
        }

        private void PopupBoxCameraSelectionPrev_Clicked(object sender, RoutedEventArgs e)
        {
            var dataContext = ((PopOutPanelSourceCardViewModel)this.DataContext);
            var selectedItem = dataContext.DataItem.FixedCameraConfig;

            // rebinding the camera list with erase the selected item, need the next line to set the selectedItem
            var items = dataContext.FixedCameraConfigs;

            if (selectedItem == null) 
                return;

            var index = items.ToList().FindIndex(x => x.Name == selectedItem.Name);

            if (index == -1)
                return;

            if (index == 0)
                index = items.Count - 1;
            else
                index -= 1;

            this.ComboBoxCameraSelection.SelectedIndex = index;
            dataContext.DataItem.FixedCameraConfig = items[index];    // assign and save item

            //dataContext.SetCamera();
        }

        private void PopupBoxCameraSelectionNext_Clicked(object sender, RoutedEventArgs e)
        {
            var dataContext = ((PopOutPanelSourceCardViewModel)this.DataContext);
            var selectedItem = dataContext.DataItem.FixedCameraConfig;

            // rebinding the camera list with erase the selected item, need the next line to set the selectedItem
            var items = dataContext.FixedCameraConfigs;

            if (selectedItem == null)
                return;

            var index = items.ToList().FindIndex(x => x.Name == selectedItem.Name);

            if (index == -1)
                index = 0;

            if (index == items.Count - 1)
                index = 0;
            else
                index += 1;

            this.ComboBoxCameraSelection.SelectedIndex = index;
            dataContext.DataItem.FixedCameraConfig = items[index];    // assign and save item

            //dataContext.SetCamera();
        }

        private void ComboBoxCameraSelection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataContext = ((PopOutPanelSourceCardViewModel)this.DataContext);

            if (e.AddedItems.Count <= 0) 
                return;
            
            dataContext.DataItem.FixedCameraConfig = (FixedCameraConfig)e.AddedItems[0];
            dataContext.SetCamera();
        }
    }
}
