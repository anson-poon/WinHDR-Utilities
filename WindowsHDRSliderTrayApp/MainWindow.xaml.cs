using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_setBrightness == null) return;

            double sliderValue = e.NewValue;
            double minBrightness = 1;
            double maxBrightness = 6;

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
                brightnessSlider.Orientation = Orientation.Vertical;
                brightnessSlider.Width = 50;
                brightnessSlider.Height = 200;
            }
            else
            {
                brightnessSlider.Orientation = Orientation.Horizontal;
                brightnessSlider.Width = 200;
                brightnessSlider.Height = 50;
            }
        }


    }
}