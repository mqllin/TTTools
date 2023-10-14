using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TTTools
{
    public  class WindowUtilities
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowText(IntPtr hWnd, string lpString);

        private static Dictionary<IntPtr, string> originalTitles = new Dictionary<IntPtr, string>();

        public  List<WindowInfo> UpdateWindowTitles(string className)
        {
            List<WindowInfo> windowList = new List<WindowInfo>();
            IntPtr hwnd = IntPtr.Zero;
            int index = 1;

            while (true)
            {
                hwnd = FindWindowEx(IntPtr.Zero, hwnd, className, null);
                if (hwnd == IntPtr.Zero)
                    break;

                StringBuilder windowText = new StringBuilder(256);
                GetWindowText(hwnd, windowText, 256);

                originalTitles[hwnd] = windowText.ToString();

                string newTitle = windowText.ToString() + " - 工具" + index;
                SetWindowText(hwnd, newTitle);

                windowList.Add(new WindowInfo { Handle = hwnd, Title = newTitle });
                index++;
            }

            return windowList;
        }

        public void ClearTitles()
        {
            foreach (var entry in originalTitles)
            {
                SetWindowText(entry.Key, entry.Value);
            }
            originalTitles.Clear();
        }
    }

}
