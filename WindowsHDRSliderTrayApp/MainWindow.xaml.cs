using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            InitializeBrightnessDelegate();
            EnumerateMonitors();
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
            var settingsWindow = new SettingsWindow();
            WindowHelper.TrackWindow(settingsWindow);
            settingsWindow.Activate();
        }

        public void SetBrightnessOrientation(string orientation)
        {
            if (orientation == "Vertical")
            {
                BrightnessSlider.Orientation = Orientation.Vertical;
                BrightnessSlider.Width = 50;
                BrightnessSlider.Height = 200;
            }
            else
            {
                BrightnessSlider.Orientation = Orientation.Horizontal;
                BrightnessSlider.Width = 200;
                BrightnessSlider.Height = 50;
            }
        }
    }
}