using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WindowsHDRSliderTrayApp.Views;

public partial class TrayIconView : UserControl
{
    public TrayIconView()
    {
        InitializeComponent();
    }

    public void ShowHideWindow_Click(object sender, RoutedEventArgs e)
    {
        var window = App.MainWindow;
        if (window == null)
        {
            return;
        }

        if (window.Visible)
        {
            window.Hide();
        }
        else
        {
            window.Show();
        }
        App.IsWindowVisible = window.Visible;
    }

    public void ExitApplication_Click(object sender, RoutedEventArgs e)
    {
        App.HandleClosedEvents = false;
        TrayIcon.Dispose();
        App.MainWindow?.Close();
    }
}