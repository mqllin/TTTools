using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TTTools
{
    public class WindowApi
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }

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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);



        private static Dictionary<IntPtr, string> originalTitles = new Dictionary<IntPtr, string>();

        // 设置窗口位置
        public static bool SetWindowPosition(IntPtr hWnd, int x, int y, int width = 0, int height = 0, bool noResize = true)
        {
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOZORDER = 0x0004;
            uint flags = SWP_NOZORDER;

            if (noResize)
                flags |= SWP_NOSIZE;

            return SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, flags);
        }

        // 更新窗口标题，并返回包含窗口句柄和用户名的列表
        public List<WindowApi> UpdateWindowTitles(string className)
        {
            List<WindowApi> windowList = new List<WindowApi>();
            IntPtr hwnd = IntPtr.Zero;

            while (true)
            {
                hwnd = FindWindowEx(IntPtr.Zero, hwnd, className, null);
                if (hwnd == IntPtr.Zero)
                    break;

                StringBuilder windowText = new StringBuilder(256);
                GetWindowText(hwnd, windowText, 256);

                originalTitles[hwnd] = windowText.ToString();

                // 获取最后一个空格分隔的部分作为用户名
                string[] parts = windowText.ToString().Split(' ');
                string username = parts.Length > 0 ? parts[^1] : "Unknown";

                // 将窗口信息存入列表
                windowList.Add(new WindowApi { Handle = hwnd, Title = username });
            }

            return windowList;
        }

        // 恢复窗口的原始标题
        public void ClearTitles()
        {
            foreach (var entry in originalTitles)
            {
                SetWindowText(entry.Key, entry.Value);
            }
            originalTitles.Clear();
        }

        // 切换到英文输入法
        public static void ChangeInputEn()
        {
            // 加载英语键盘布局（例如美国英语："00000409"）
            IntPtr hklEnglish = LoadKeyboardLayout("00000409", 1);

            // 激活加载的键盘布局
            ActivateKeyboardLayout(hklEnglish, 0);
            LogService.Log("已切换到默认的英文输入法");
        }

        // 根据窗口类名获取所有的进程ID
        public static List<int> GetProcessIdsByClassName(string className)
        {
            List<int> processIds = new List<int>();
            IntPtr hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, className, null);
            while (hWnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                processIds.Add((int)processId);
                hWnd = FindWindowEx(IntPtr.Zero, hWnd, className, null);
            }
            return processIds;
        }

        // 根据窗口句柄获取进程ID
        public static int GetProcessIdByWindowHandle(decimal windowHandle)
        {
            IntPtr hWnd = new IntPtr((int)windowHandle);
            GetWindowThreadProcessId(hWnd, out uint processId);
            return (int)processId;
        }

        // 通过16进制窗口句柄获取进程ID
        public static int GetProcessIdByHexWindowHandle(string hexWindowHandle)
        {
            if (!int.TryParse(hexWindowHandle, System.Globalization.NumberStyles.HexNumber, null, out int windowHandleInt))
                throw new ArgumentException("Invalid hex format", nameof(hexWindowHandle));

            IntPtr hWnd = new IntPtr(windowHandleInt);
            GetWindowThreadProcessId(hWnd, out uint processId);
            return (int)processId;
        }


        /// <summary>
        /// 查询并输出所有子窗口的句柄、类名和标题。
        /// </summary>
        /// <param name="mainWindowHandle">主窗口句柄</param>
        public static void LogAllWindowHandles(IntPtr mainWindowHandle)
        {
            // 初始化一个队列以进行广度优先遍历
            Queue<IntPtr> windowsQueue = new Queue<IntPtr>();
            windowsQueue.Enqueue(mainWindowHandle);

            // 遍历队列中的所有窗口
            while (windowsQueue.Count > 0)
            {
                IntPtr currentHandle = windowsQueue.Dequeue();

                // 获取窗口标题
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(currentHandle, windowTitle, 256);

                // 获取窗口类名
                StringBuilder className = new StringBuilder(256);
                GetClassName(currentHandle, className, 256);

                // 记录窗口信息
                LogService.Log($"句柄: {currentHandle}, 类名: {className}, 标题: {windowTitle}");

                // 查找当前窗口的第一个子窗口
                IntPtr childHandle = FindWindowEx(currentHandle, IntPtr.Zero, null, null);

                // 遍历当前窗口的所有子窗口
                while (childHandle != IntPtr.Zero)
                {
                    windowsQueue.Enqueue(childHandle);
                    childHandle = FindWindowEx(currentHandle, childHandle, null, null); // 获取下一个兄弟窗口
                }
            }
        }
    }
}
