using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;

namespace Speechabler
{
    public class ThemeWindow : Window
    {
        static ThemeWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemeWindow), new FrameworkPropertyMetadata(typeof(ThemeWindow)));
            WindowStyleProperty.OverrideMetadata(typeof(ThemeWindow), new FrameworkPropertyMetadata(WindowStyle.None));
            WindowStateProperty.OverrideMetadata(typeof(ThemeWindow), new FrameworkPropertyMetadata(OnWindowStateChanged));
            ResizeModeProperty.OverrideMetadata(typeof(ThemeWindow), new FrameworkPropertyMetadata(OnResizeModeChanged));
        }


        private static void OnWindowStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ThemeWindow)?.UpdateByWindowStateChanged((WindowState)e.NewValue);
        }

        private static void OnResizeModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ThemeWindow)?.UpdateByResizeModeChanged((ResizeMode)e.NewValue);
        }

        private void UpdateByWindowStateChanged(WindowState windowState)
        {
            if (windowFrame != null)
            {
                switch (windowState)
                {
                    case WindowState.Maximized:
                        windowFrame.Margin = new Thickness(-2);
                        WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(0);
                        maximizeWindowIcon.Visibility = Visibility.Collapsed;
                        normalWindowIcon.Visibility = Visibility.Visible;
                        break;
                    case WindowState.Normal:
                        windowFrame.Margin = new Thickness(0);
                        WindowChrome.GetWindowChrome(this).ResizeBorderThickness = new Thickness(3);
                        normalWindowIcon.Visibility = Visibility.Collapsed;
                        maximizeWindowIcon.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void UpdateByResizeModeChanged(ResizeMode resizeMode)
        {
            if (windowFrame != null)
            {
                switch (resizeMode)
                {
                    case ResizeMode.NoResize:
                        maximizeButton.Visibility = Visibility.Collapsed;
                        minimizeButton.Visibility = Visibility.Collapsed;
                        break;
                    case ResizeMode.CanMinimize:
                        maximizeButton.Visibility = Visibility.Collapsed;
                        minimizeButton.Visibility = Visibility.Visible;
                        break;
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                        maximizeButton.Visibility = Visibility.Visible;
                        minimizeButton.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        public ThemeWindow()
        {
            SourceInitialized += Window_SourceInitialized;
            Loaded += ThemeWindow_Loaded;
        }

        private void ThemeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateByResizeModeChanged(ResizeMode);
            UpdateByWindowStateChanged(WindowState);
        }

        private Button minimizeButton;
        private Button maximizeButton;
        private Button closeButton;
        private FrameworkElement windowFrame;
        private FrameworkElement maximizeWindowIcon;
        private FrameworkElement normalWindowIcon;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            windowFrame = Template.FindName("PART_WindowFrame", this) as FrameworkElement;
            maximizeWindowIcon = Template.FindName("PART_MaximizeWindowIcon", this) as FrameworkElement;
            normalWindowIcon = Template.FindName("PART_NormalWindowIcon", this) as FrameworkElement;

            if (Template.FindName("PART_MinimizeButton", this) is Button minimizeButton)
            {
                this.minimizeButton = minimizeButton;
                this.minimizeButton.Click += Minimize_Click;
            }
            if (Template.FindName("PART_MaximizeButton", this) is Button maximizeButton)
            {
                this.maximizeButton = maximizeButton;
                this.maximizeButton.Click += Maximize_Click;
            }
            if (Template.FindName("PART_CloseButton", this) is Button closeButton)
            {
                this.closeButton = closeButton;
                this.closeButton.Click += Close_Click;
            }

        }

        void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    Native.WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        static class Native
        {

            [DllImport("user32")]
            internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

            [DllImport("user32")]
            internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

            public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, int minWidth, int minHeight)
            {
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                int MONITOR_DEFAULTTONEAREST = 0x00000002;
                IntPtr monitor = Native.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {

                    Native.MONITORINFO monitorInfo = new Native.MONITORINFO();
                    Native.GetMonitorInfo(monitor, monitorInfo);
                    Native.RECT rcWorkArea = monitorInfo.rcWork;
                    Native.RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                    mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                    mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                    mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                    mmi.ptMinTrackSize.x = minWidth;
                    mmi.ptMinTrackSize.y = minHeight;
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }


            /// <summary>
            /// POINT aka POINTAPI
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                /// <summary>
                /// x coordinate of point.
                /// </summary>
                public int x;
                /// <summary>
                /// y coordinate of point.
                /// </summary>
                public int y;

                /// <summary>
                /// Construct a point of coordinates (x,y).
                /// </summary>
                public POINT(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MINMAXINFO
            {
                public POINT ptReserved;
                public POINT ptMaxSize;
                public POINT ptMaxPosition;
                public POINT ptMinTrackSize;
                public POINT ptMaxTrackSize;
            };

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MONITORINFO
            {
                /// <summary>
                /// </summary>            
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

                /// <summary>
                /// </summary>            
                public RECT rcMonitor = new RECT();

                /// <summary>
                /// </summary>            
                public RECT rcWork = new RECT();

                /// <summary>
                /// </summary>            
                public int dwFlags = 0;
            }


            /// <summary> Win32 </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct RECT
            {
                /// <summary> Win32 </summary>
                public int left;
                /// <summary> Win32 </summary>
                public int top;
                /// <summary> Win32 </summary>
                public int right;
                /// <summary> Win32 </summary>
                public int bottom;

                /// <summary> Win32 </summary>
                public static readonly RECT Empty = new RECT();

                /// <summary> Win32 </summary>
                public int Width
                {
                    get { return Math.Abs(right - left); }  // Abs needed for BIDI OS
                }
                /// <summary> Win32 </summary>
                public int Height
                {
                    get { return bottom - top; }
                }

                /// <summary> Win32 </summary>
                public RECT(int left, int top, int right, int bottom)
                {
                    this.left = left;
                    this.top = top;
                    this.right = right;
                    this.bottom = bottom;
                }


                /// <summary> Win32 </summary>
                public RECT(RECT rcSrc)
                {
                    this.left = rcSrc.left;
                    this.top = rcSrc.top;
                    this.right = rcSrc.right;
                    this.bottom = rcSrc.bottom;
                }

                /// <summary> Win32 </summary>
                public bool IsEmpty
                {
                    get
                    {
                        // BUGBUG : On Bidi OS (hebrew arabic) left > right
                        return left >= right || top >= bottom;
                    }
                }
                /// <summary> Return a user friendly representation of this struct </summary>
                public override string ToString()
                {
                    if (this == RECT.Empty) { return "RECT {Empty}"; }
                    return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
                }

                /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
                public override bool Equals(object obj)
                {
                    if (!(obj is Rect)) { return false; }
                    return (this == (RECT)obj);
                }

                /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
                public override int GetHashCode()
                {
                    return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
                }


                /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
                public static bool operator ==(RECT rect1, RECT rect2)
                {
                    return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
                }

                /// <summary> Determine if 2 RECT are different(deep compare)</summary>
                public static bool operator !=(RECT rect1, RECT rect2)
                {
                    return !(rect1 == rect2);
                }
            }
        }

    }
}
