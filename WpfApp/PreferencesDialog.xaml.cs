using MahApps.Metro.Controls;
using MSFSPopoutPanelManager.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MSFSPopoutPanelManager.WpfApp
{
    /// <summary>
    /// Interaction logic for AddProfileDialog.xaml
    /// </summary>
    public partial class PreferencesDialog : MetroWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PreferencesDialog(AppSetting appSetting)
        {
            InitializeComponent();
            AppSetting = appSetting;

            this.DataContext = this;

            ApplicationSettingsVisibility = Visibility.Visible;
            PopOutSettingsVisibility = Visibility.Collapsed;
            AutoPopOutSettingsVisibility = Visibility.Collapsed;
        }

        public AppSetting AppSetting { get; set; }

        public Visibility ApplicationSettingsVisibility { get; set; }

        public Visibility PopOutSettingsVisibility { get; set; }

        public Visibility AutoPopOutSettingsVisibility { get; set; }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            var treeViewItem = (TreeViewItem)e.Source;

            ApplicationSettingsVisibility = Visibility.Collapsed;
            PopOutSettingsVisibility = Visibility.Collapsed;
            AutoPopOutSettingsVisibility = Visibility.Collapsed;

            if (treeViewItem.Header.ToString() == "Application Settings")
            {
                ApplicationSettingsVisibility = Visibility.Visible;
            }
            else if(treeViewItem.Header.ToString() == "Pop Out Settings")
            {
                PopOutSettingsVisibility = Visibility.Visible;
            }
            else if (treeViewItem.Header.ToString() == "Auto Pop Out Panel Settings")
            {
                AutoPopOutSettingsVisibility = Visibility.Visible;
            }
        }
    }
}
