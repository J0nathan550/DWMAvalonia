using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace DWMAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            // Apply default settings on load
            ApplyDwmSettings();
        }

        private void OnUpdateClick(object sender, RoutedEventArgs e)
        {
            ApplyDwmSettings();
        }

        private void ApplyDwmSettings()
        {
            var handle = this.TryGetPlatformHandle();
            if (handle != null && handle.HandleDescriptor == "HWND")
            {
                IntPtr hwnd = handle.Handle;

                int backdropType = BackdropComboBox.SelectedIndex; // Matches DWMSBT constants perfectly
                string hexColor = TitleBarColorBox.Text;
                bool isDark = DarkModeCheck.IsChecked ?? false;

                DwmInterop.EnableMicaAndTitleBar(hwnd, backdropType, hexColor, isDark);
            }
        }
    }
}