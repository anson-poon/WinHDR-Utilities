using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.UI.Xaml;
using WindowsHDRSliderTrayApp.Views;
using WinUIGallery.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindowsHDRSliderTrayApp
{
    public partial class App : Application
    {
        #region Properties

        public static Window? MainWindow { get; set; }
        public static bool HandleClosedEvents { get; set; } = true;
        public static bool BoostMaxBrightness { get; set; } = false;
        public static string OrientationSetting { get; set; } = "Horizontal";
        public static bool IsWindowVisible { get; set; } = true;

        #endregion

        #region Constructors

        public App()
        {
            InitializeComponent();
        }

        #endregion

        #region EventHandlers

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = (MainWindow)WindowHelper.CreateWindow();

            MainWindow.Closed += (sender, args) =>
            {
                if (HandleClosedEvents)
                {
                    args.Handled = true;
                    MainWindow.Hide();
                }
            };
            MainWindow.Activate();
        }

        #endregion
    }
}