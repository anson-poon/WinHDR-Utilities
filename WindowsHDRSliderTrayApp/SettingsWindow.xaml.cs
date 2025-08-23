using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.Graphics;
using WinUIGallery.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindowsHDRSliderTrayApp;

public sealed partial class SettingsWindow : Window
{
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

        CenterWindow();

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
            var mainWindow = WindowHelper.ActiveWindows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.SetBrightnessOrientation(orientation);
            }
        }
    }
}
