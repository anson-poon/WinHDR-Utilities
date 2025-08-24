using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WinUIGallery.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WindowsHDRSliderTrayApp
{
    public sealed partial class MainWindow : Window
    {
        private delegate void DwmpSDRToHDRBoostPtr(IntPtr monitor, double brightness);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, int ordinal);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left, top, right, bottom;
        }

        private List<IntPtr> _monitorHandles = new();

        private DwmpSDRToHDRBoostPtr _setBrightness;

        public double CurrentSliderValue => BrightnessSlider.Value;

        private IntPtr hWnd = IntPtr.Zero;
        private AppWindow appW = null;
        private OverlappedPresenter presenter = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBrightnessDelegate();
            EnumerateMonitors();

            AppWindow.Title = "WinHDR Utilities";
            AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 250));
            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
            MoveWindowToTray();

            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            appW = AppWindow.GetFromWindowId(wndId);
            presenter = appW.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.IsResizable = false;
                presenter.SetBorderAndTitleBar(true, false);
            }

            ExtendsContentIntoTitleBar = true;
        }

        private void InitializeBrightnessDelegate()
        {
            var dwmapi = LoadLibrary("dwmapi.dll");
            if (dwmapi == IntPtr.Zero)
            {
                throw new Exception("Failed to load dwmapi.dll");
            }

            var proc = GetProcAddress(dwmapi, 171); // ordinal 171 is the HDR brightness function
            if (proc == IntPtr.Zero)
            {
                throw new Exception("Failed to get function address for HDR brightness.");
            }

            _setBrightness = Marshal.GetDelegateForFunctionPointer<DwmpSDRToHDRBoostPtr>(proc);
        }

        private void EnumerateMonitors()
        {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
                {
                    _monitorHandles.Add(hMonitor);
                    return true;
                }, IntPtr.Zero);
        }

        private void MoveWindowToTray()
        {
            
        }

        public void ApplyBrightness(double sliderValue)
        {
            if (_setBrightness == null) return;

            double minBrightness = 1.0;
            double maxBrightness = App.BoostMaxBrightness ? 12.0 : 6.0;

            double brightnessValue = minBrightness + (sliderValue / 100.0) * (maxBrightness - minBrightness);

            foreach (var monitor in _monitorHandles)
            {
                try
                {
                    _setBrightness(monitor, brightnessValue);
                }
                catch
                {
                    throw new Exception("Failed to set brightness for monitor.");
                }
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ApplyBrightness(e.NewValue);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var existingWindow = WindowHelper.ActiveWindows
                .OfType<SettingsWindow>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();  // Bring the existing window to front
                return;
            }

            var settingsWindow = new SettingsWindow();  // Create a new instance
            WindowHelper.TrackWindow(settingsWindow);
            settingsWindow.Activate();
        }

        public void SetBrightnessOrientation(string orientation)
        {
            if (orientation == "Vertical")
            {
                StackPanel1.Orientation = Orientation.Vertical;
                StackPanel1.HorizontalAlignment = HorizontalAlignment.Center;
                StackPanel1.VerticalAlignment = VerticalAlignment.Center;

                BrightnessIcon.Width = 35;

                BrightnessSlider.Orientation = Orientation.Vertical;
                BrightnessSlider.Width = 35;
                BrightnessSlider.Height = 300;

                SettingIcon.Width = 35;

                AppWindow.Resize(new Windows.Graphics.SizeInt32(250, 800));
            }
            else
            {
                StackPanel1.Orientation = Orientation.Horizontal;
                StackPanel1.HorizontalAlignment = HorizontalAlignment.Center;
                StackPanel1.VerticalAlignment = VerticalAlignment.Center;

                BrightnessSlider.Orientation = Orientation.Horizontal;
                BrightnessSlider.Width = 300;
                BrightnessSlider.Height = Double.NaN;

                AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 250));
            }
        }
    }
}