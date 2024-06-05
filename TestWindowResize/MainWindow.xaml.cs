using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using static TestWindowResize.MainWindow;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TestWindowResize
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public const UInt32 WM_DESTROY = 0x0002;
        public const UInt32 WM_SIZE = 0x0005;
        public const UInt32 WM_GETMINMAXINFO = 0x0024;
        public const int MinWidth = 400;
        public const int MinHeight = 200;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        public MainWindow()
        {
            this.InitializeComponent();
            m_hwnd = WindowNative.GetWindowHandle(this);
            m_newWndProc = mainWindowSubclassWndProc;
            m_oldWndProc = SetWindowLongPtrW(m_hwnd, /*GWLP_WNDPROC*/-4, Marshal.GetFunctionPointerForDelegate<WndProc>(m_newWndProc));
        }

        private IntPtr mainWindowSubclassWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch(msg)
            {
                case WM_DESTROY:
                    SetWindowLongPtrW(hWnd, /*GWLP_WNDPROC*/-4, m_oldWndProc);
                    m_oldWndProc = IntPtr.Zero;
                    return 0;
                case WM_GETMINMAXINFO:
                    MINMAXINFO minMaxInfo = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                    minMaxInfo.ptMinTrackSize.x = MinWidth; // Minimum width
                    minMaxInfo.ptMinTrackSize.y = MinHeight; // Minimum height
                    Marshal.StructureToPtr(minMaxInfo, lParam, false);
                    return 0;
            }
            return CallWindowProcW(m_oldWndProc, hWnd, msg, wParam, lParam);
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        private IntPtr m_oldWndProc;
        private IntPtr m_hwnd;
        private WndProc m_newWndProc;
    }
}
