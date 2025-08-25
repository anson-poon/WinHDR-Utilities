// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using WindowsHDRSliderTrayApp;
using WinRT.Interop;

namespace WinUIGallery.Helpers;

// Helper class to allow the app to find the Window that contains an
// arbitrary UIElement (GetWindowForElement).  To do this, we keep track
// of all active Windows.  The app code must call WindowHelper.CreateWindow
// rather than "new Window" so we can keep track of all the relevant
// windows.  In the future, we would like to support this in platform APIs.
public partial class WindowHelper
{
    static public Window CreateWindow()
    {
        MainWindow newWindow = new MainWindow();
        TrackWindow(newWindow);
        return newWindow;
    }

    static public void TrackWindow(Window window)
    {
        window.Closed += (sender, args) =>
        {
            _activeWindows.Remove(window);
        };
        _activeWindows.Add(window);
    }

    static public Window GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot != null)
        {
            foreach (Window window in _activeWindows)
            {
                if (element.XamlRoot == window.Content.XamlRoot)
                {
                    return window;
                }
            }
        }
        return null;
    }
    // get dpi for an element
    static public double GetRasterizationScaleForElement(UIElement element)
    {
        if (element.XamlRoot != null)
        {
            foreach (Window window in _activeWindows)
            {
                if (element.XamlRoot == window.Content.XamlRoot)
                {
                    return element.XamlRoot.RasterizationScale;
                }
            }
        }
        return 0.0;
    }

    static public List<Window> ActiveWindows { get { return _activeWindows; } }

    static private List<Window> _activeWindows = new List<Window>();

    //static public StorageFolder GetAppLocalFolder()
    //{
    //    StorageFolder localFolder;
    //    if (!NativeMethods.IsAppPackaged)
    //    {
    //        localFolder = Task.Run(async () => await StorageFolder.GetFolderFromPathAsync(System.AppContext.BaseDirectory)).Result;
    //    }
    //    else
    //    {
    //        localFolder = ApplicationData.Current.LocalFolder;
    //    }
    //    return localFolder;
    //}

    // Custom helpers
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int left, top, right, bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, out Rect pvParam, uint fWinIni);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static AppWindow? GetAppWindow(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    public static AppWindow ConfigureBaseWindow(Window window, string title, SizeInt32 size)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(wndId);

        appWindow.Title = title;
        appWindow.Resize(size);
        appWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        return appWindow;
    }

    public static void ConfigurePresenter(OverlappedPresenter? presenter)
    {
        if (presenter == null) return;

        presenter.IsAlwaysOnTop = true;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.IsResizable = false;
        presenter.SetBorderAndTitleBar(true, false);
    }

    public static void HideWindowIfNotForeground(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        var appWindow = GetAppWindow(window);
        if (hwnd != GetForegroundWindow() && appWindow != null)
        {
            appWindow.Hide();
        }
    }

    public static void BringWindowToFront(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        SetForegroundWindow(hwnd);
    }

    public static void MoveWindowToTray(Window window)
    {
        var appWindow = GetAppWindow(window);
        if (appWindow is null) return;

        var workArea = GetPrimaryMonitorWorkArea();

        int windowWidth = (int)appWindow.Size.Width;
        int windowHeight = (int)appWindow.Size.Height;

        int x = workArea.Width - windowWidth - 10; // offset from right
        int y = workArea.Height - windowHeight - 70; // offset from bottom

        appWindow.Move(new PointInt32(x, y));
    }

    private static RectInt32 GetPrimaryMonitorWorkArea()
    {
        Rect workArea;
        SystemParametersInfo(0x0030, 0, out workArea, 0); // SPI_GETWORKAREA
        return new RectInt32
        {
            X = workArea.left,
            Y = workArea.top,
            Width = workArea.right - workArea.left,
            Height = workArea.bottom - workArea.top
        };
    }

    public static void CenterWindow(Window window)
    {
        var appWindow = GetAppWindow(window);
        if (appWindow is null) return;

        var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
        if (area == null) return;

        var newX = (area.Value.Width - appWindow.Size.Width) / 2;
        var newY = (area.Value.Height - appWindow.Size.Height) / 2;

        appWindow.Move(new PointInt32(newX, newY));
    }

    public static void BringWindowAboveMain(Window window)
    {
        var mainWindow = ActiveWindows.OfType<MainWindow>().FirstOrDefault();
        var appWindow = GetAppWindow(window);

        if (mainWindow != null && appWindow != null)
        {
            var mainBounds = mainWindow.AppWindow.Position;
            var mainSize = mainWindow.AppWindow.Size;

            // Place window above the main window
            var newX = mainBounds.X + (mainSize.Width - 800) / 2;
            var newY = mainBounds.Y - 510;

            if (newY < 0) newX = 0;

            appWindow.Move(new PointInt32(newX, newY));
        }
        else
        {
            CenterWindow(window);   //fallback
        }
    }
}