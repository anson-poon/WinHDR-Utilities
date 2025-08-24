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
        AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 500));
        AppWindow.Move(new Windows.Graphics.PointInt32(50, 50));
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
        //// Set the taskbar icon (displayed in the taskbar)
        //AppWindow.SetTaskbarIcon("Assets/Tiles/GalleryIcon.ico");
        //// Set the title bar icon (displayed in the window's title bar)
        //AppWindow.SetTitleBarIcon("Assets/Tiles/GalleryIcon.ico");

        _mainWindow = WindowHelper.ActiveWindows.OfType<MainWindow>().FirstOrDefault();

        if (_mainWindow != null)
        {
            var mainBounds = _mainWindow.AppWindow.Position;
            var mainSize = _mainWindow.AppWindow.Size;

            // Place settings window above the main window
            var settingsX = mainBounds.X + (mainSize.Width - 800) / 2;
            var settingsY = mainBounds.Y - 510;

            if (settingsY < 0) settingsY = 0;

            AppWindow.Move(new Windows.Graphics.PointInt32(settingsX, settingsY));
        }
        else
        {
            CenterWindow();
        }
       
    }

    // Centers the given AppWindow on the screen based on the available display area.
    private void CenterWindow()
    {
        var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
        if (area == null) return;
        AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
    }

    private void OrientationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OrientationComboBox.SelectedItem is ComboBoxItem item)
        {
            var orientation = item.Content?.ToString();
            if (_mainWindow != null && orientation != null)
            {
                _mainWindow.SetBrightnessOrientation(orientation);
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
