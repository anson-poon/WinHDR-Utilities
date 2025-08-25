using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using WinUIGallery.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindowsHDRSliderTrayApp;

public sealed partial class SettingsWindow : Window
{
    private MainWindow? _mainWindow;

    public SettingsWindow()
    {
        InitializeComponent();

        AppWindow.Title = "Settings";
        AppWindow.Resize(new SizeInt32(800, 500));
        AppWindow.Move(new PointInt32(50, 50));
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
        //// Set the taskbar icon (displayed in the taskbar)
        //AppWindow.SetTaskbarIcon("Assets/Tiles/GalleryIcon.ico");
        //// Set the title bar icon (displayed in the window's title bar)
        //AppWindow.SetTitleBarIcon("Assets/Tiles/GalleryIcon.ico");
        _mainWindow = WindowHelper.ActiveWindows.OfType<MainWindow>().FirstOrDefault();
        WindowHelper.BringWindowAboveMain(this); 
    }

    private void OrientationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OrientationComboBox.SelectedItem is ComboBoxItem item)
        {
            var orientation = item.Content?.ToString();
            if (_mainWindow != null && orientation != null)
            {
                _mainWindow.SetBrightnessOrientation(orientation);
                WindowHelper.MoveWindowToTray(_mainWindow);
                WindowHelper.BringWindowAboveMain(this);
            }
        }
    }

    private void BoostMaxBrightnessToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        App.BoostMaxBrightness = BoostMaxBrightnessToggleSwitch.IsOn;
        if (_mainWindow != null)
        {
            _mainWindow.ApplyBrightness(_mainWindow.CurrentSliderValue);
        }
    }
}
