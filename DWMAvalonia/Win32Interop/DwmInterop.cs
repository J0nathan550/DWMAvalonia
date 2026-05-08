using System;
using System.Runtime.InteropServices;

namespace DWMAvalonia
{
    public static class DwmInterop
    {
        // --- Constants ---
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_CAPTION_COLOR = 35;
        public const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        public const int DWMWA_MICA_EFFECT = 1029;

        public const int DWMSBT_AUTO = 0;
        public const int DWMSBT_DISABLE = 1;
        public const int DWMSBT_MAINWINDOW = 2;       // Mica
        public const int DWMSBT_TRANSIENTWINDOW = 3;  // Acrylic
        public const int DWMSBT_TABBEDWINDOW = 4;     // Mica Alt

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RTL_OSVERSIONINFOW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
        }

        // --- P/Invokes ---
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int RtlGetVersion(ref RTL_OSVERSIONINFOW versionInfo);

        // --- Application Logic ---
        public static void EnableMicaAndTitleBar(IntPtr hwnd, int backdropType, string hexColor, bool isDark)
        {
            if (hwnd == IntPtr.Zero) return;

            // 1. Extend frame (Required for Mica/Acrylic to render correctly)
            MARGINS margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);

            // 2. Get Windows Build Version
            RTL_OSVERSIONINFOW rovi = new RTL_OSVERSIONINFOW();
            rovi.dwOSVersionInfoSize = (uint)Marshal.SizeOf(rovi);
            uint build = 0;

            if (RtlGetVersion(ref rovi) == 0)
            {
                build = rovi.dwBuildNumber;
            }

            // 3. Apply Backdrop
            if (build >= 22523) // Windows 11 22H2+
            {
                DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, Marshal.SizeOf(typeof(int)));
            }
            else if (build >= 22000) // Windows 11 21H2 (Undocumented Old Method)
            {
                int mica = (backdropType == DWMSBT_MAINWINDOW) ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_MICA_EFFECT, ref mica, Marshal.SizeOf(typeof(int)));
            }

            // 4. Apply Dark Mode
            int darkMode = isDark ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, Marshal.SizeOf(typeof(int)));

            // 5. Apply Title Bar Color (Win32 COLORREF format is 0x00bbggrr)
            if (!string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    var c = Avalonia.Media.Color.Parse(hexColor);
                    int colorRef = (c.B << 16) | (c.G << 8) | c.R;
                    DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, Marshal.SizeOf(typeof(int)));
                }
                catch { /* Ignore invalid hex strings */ }
            }
        }
    }
}