using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class PopOutSetting : ObservableObject
    {
        public PopOutSetting()
        {
            InitializeChildPropertyChangeBinding();
        }

        public AutoPanning AutoPanning { get; set; } = new();

        public AfterPopOutCameraView AfterPopOutCameraView { get; set; } = new();

        public bool MinimizeDuringPopOut { get; set; } = true;

        public bool MinimizeAfterPopOut { get; set; } = false;

        public bool UseLeftRightControlToPopOut { get; set; } = false;

        public bool EnablePanelResetWhenLocked { get; set; } = true;

        public bool EnablePopOutMessages { get; set; } = true;

        public bool AutoActivePause { get; set; } = false;

        public PopOutTitleBarCustomization PopOutTitleBarCustomization { get; set; } = new();
    };
}
