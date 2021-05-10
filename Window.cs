using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Database
{
    class Window
    {
        public static void SnapLeft(Process process)
        {
            Rect rect = GetWorkAreaRect();
            rect.Right /= 2;
            MoveWindow(process, rect);
        }

        public static void SnapRight(Process process)
        {
            Rect rect = GetWorkAreaRect();
            rect.Left = rect.Right / 2;
            MoveWindow(process, rect);
        }

        private static void MoveWindow(Process process, Rect rect)
        {
            if (!MoveWindow(process.MainWindowHandle,
                rect.Left, rect.Top,
                rect.Width, rect.Height,
                true))
            {
                throw new Exception("MoveWindow failed with error: " + GetLastError());
            }
        }

        private static Rect GetWorkAreaRect()
        {
            MonitorInfoEx monitorInfoEx = new MonitorInfoEx();

            bool MonitorEnumCallBack(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                monitorInfoEx.Size = Marshal.SizeOf(monitorInfoEx);
                GetMonitorInfo(hMonitor, ref monitorInfoEx);
                return true;
            }

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumCallBack, IntPtr.Zero);

            Rect rect = monitorInfoEx.WorkArea;
            if (rect.Right > 3000)
            {
                // For ultra wide monitors work area should be only on the left side of the screen,
                //   where right side is reserved for the Visual Studio instance.
                //
                rect.Right /= 2;
            }

            return rect;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner

            public int Width => Right - Left;
            public int Height => Bottom - Top;

            public override string ToString() =>
                "Left: " + Left + " " +
                "Top: " + Top + " " +
                "Right: " + Right + " " +
                "Bottom: " + Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        // size of a device name string
        private const int CCHDEVICENAME = 32;

        /// <summary>
        /// The MONITORINFOEX structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
        /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name
        /// for the display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfoEx
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public Rect Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public Rect WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            ///
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
            public uint Flags;

            /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();
    }
}
