using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Graphics;
using Windows.Management.Core;
using Windows.Storage;
using Windows.UI.WindowManagement;
using WinUIGallery.Helpers;

namespace WindowsHDRSliderTrayApp;

public sealed partial class SettingsWindow : Window
{
    private MainWindow? _mainWindow;
    private readonly ApplicationDataContainer localSettings = ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).LocalSettings;



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

        OrientationComboBox.SelectedIndex = localSettings.Values.TryGetValue("WindowOrientation", out var woValue) && woValue is int index ? index : 0;
        BoostMaxBrightnessToggleSwitch.IsOn = localSettings.Values.TryGetValue("BoostMaxBrightness", out var bmbValue) && bmbValue is bool boost ? boost : false;
        //TODO: add brightness value local settings
    }

    private void OrientationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var orientationIndex = OrientationComboBox.SelectedIndex;
        localSettings.Values["WindowOrientation"] = orientationIndex;

        if (_mainWindow != null)
        {
            _mainWindow.SetWindowOrientation(orientationIndex);
            WindowHelper.MoveWindowToTray(_mainWindow);
            WindowHelper.BringWindowAboveMain(this);
        }
    }

    private void BoostMaxBrightnessToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        localSettings.Values["BoostMaxBrightness"] = BoostMaxBrightnessToggleSwitch.IsOn;

        App.BoostMaxBrightness = BoostMaxBrightnessToggleSwitch.IsOn;
        if (_mainWindow != null)
        {
            _mainWindow.ApplyBrightness(_mainWindow.CurrentSliderValue);
        }
    }
}
