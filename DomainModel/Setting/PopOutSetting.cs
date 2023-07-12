using MSFSPopoutPanelManager.Shared;

namespace MSFSPopoutPanelManager.DomainModel.Setting
{
    public class PopOutSetting : ObservableObject
    {
        public PopOutSetting()
        {
            MinimizeDuringPopOut = true;
            MinimizeAfterPopOut = false;
            UseLeftRightControlToPopOut = false;
            AutoActivePause = false;

            AfterPopOutCameraView = new AfterPopOutCameraView();
            AutoPanning = new AutoPanning();
            PopOutTitleBarCustomization = new PopOutTitleBarCustomization();
            EnablePanelResetWhenLocked = true;

            InitializeChildPropertyChangeBinding();
        }

        public AutoPanning AutoPanning { get; set; }

        public AfterPopOutCameraView AfterPopOutCameraView { get; set; }

        public bool MinimizeDuringPopOut { get; set; }

        public bool MinimizeAfterPopOut { get; set; }

        public bool UseLeftRightControlToPopOut { get; set; }

        public bool EnablePanelResetWhenLocked { get; set; }

        public bool AutoActivePause { get; set; }

        public PopOutTitleBarCustomization PopOutTitleBarCustomization { get; set; }
    };
}
