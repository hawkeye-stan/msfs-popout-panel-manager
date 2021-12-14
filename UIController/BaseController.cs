using MSFSPopoutPanelManager.Shared;
using System;
using System.ComponentModel;

namespace MSFSPopoutPanelManager.UIController
{
    public class BaseController : INotifyPropertyChanged
    {
        // Need this for PropertyChanged.Fody plugin to add dynamic notification for binding for default SET property in controllers
        public event PropertyChangedEventHandler PropertyChanged;

        public static UserProfileData ActiveUserPlaneProfile { get; set; }

        #region Shared Events

        public static event EventHandler OnPopOutCompleted;

        public static event EventHandler OnRestart;

        public void PopOutCompleted()
        {
            OnPopOutCompleted?.Invoke(this, null);
        }

        public void Restart()
        {
            ActiveUserPlaneProfile = null;
            OnRestart?.Invoke(this, null);
        }

        #endregion
    }
}
