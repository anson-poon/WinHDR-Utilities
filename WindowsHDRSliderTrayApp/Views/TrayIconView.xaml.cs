using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml.Controls;

namespace WindowsHDRSliderTrayApp.Views;

public partial class TrayIconView : UserControl
{
    public TrayIconView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    public void ShowHideWindow()
    {
        var window = App.MainWindow;
        if (window == null)
            return;

        if (window.Visible)
            window.Hide();
        else
            window.Show();

        App.IsWindowVisible = window.Visible;
    }

    [RelayCommand]
    public void ExitApplication()
    {
        App.HandleClosedEvents = false;
        TrayIcon.Dispose();
        App.MainWindow?.Close();
    }
}