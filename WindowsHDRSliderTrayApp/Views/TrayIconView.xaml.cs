using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using WinRT.Interop;
using WinUIGallery.Helpers;

namespace WindowsHDRSliderTrayApp.Views;

public partial class TrayIconView : UserControl
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

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
        {
            window.Hide();
        }
        else
        {
            window.Show();
            WindowHelper.BringWindowToFront(window);
        }

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