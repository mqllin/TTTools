using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TTTools
{
    public  class WindowUtilities
    {
        // 导入 Windows API 函数
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ActivateKeyboardLayout(IntPtr hkl, uint Flags);


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



  
        public static void ChangeInputEn()
        {
            // 加载英语键盘布局（例如美国英语："00000409"）
            IntPtr hklEnglish = LoadKeyboardLayout("00000409", 1);

            // 激活加载的键盘布局
            ActivateKeyboardLayout(hklEnglish, 0);

            Console.WriteLine("已切换到默认的英文输入法");
        }
    }

}
