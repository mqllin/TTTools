using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TTTools
{
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }

        // 导入user32.dll中的FindWindow和FindWindowEx函数
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        // 导入user32.dll中的GetWindowThreadProcessId函数
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        // 定义一个静态方法，通过类名Galaxy2DEngine获取所有的进程ID
        public static List<int> GetProcessIdsByClassName(string className)
        {
            List<int> processIds = new List<int>();

            IntPtr hWnd = FindWindow(className, null);
            while (hWnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                processIds.Add((int)processId);

                hWnd = FindWindowEx(IntPtr.Zero, hWnd, className, null);
            }

            return processIds;
        }

        // 定义一个静态方法，通过窗口句柄获取进程ID
        public static int GetProcessIdByWindowHandle(decimal windowHandle)
        {
            // 将十进制窗口句柄转换为IntPtr
            IntPtr hWnd = new IntPtr((int)windowHandle);

            // 调用GetWindowThreadProcessId函数获取进程ID
            GetWindowThreadProcessId(hWnd, out uint processId);

            return (int)processId;
        }

        // 新方法: 通过16进制窗口句柄获取进程ID
        public static int GetProcessIdByHexWindowHandle(string hexWindowHandle)
        {
            if (!int.TryParse(hexWindowHandle, System.Globalization.NumberStyles.HexNumber, null, out int windowHandleInt))
                throw new ArgumentException("Invalid hex format", nameof(hexWindowHandle));

            // 将16进制窗口句柄转换为10进制，然后再转换为IntPtr
            IntPtr hWnd = new IntPtr(windowHandleInt);

            // 调用GetWindowThreadProcessId函数获取进程ID
            GetWindowThreadProcessId(hWnd, out uint processId);

            return (int)processId;
        }


    }

    
}
