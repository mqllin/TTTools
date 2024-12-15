using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TTTools
{
    class WindowClickTools
    {
        private readonly IntPtr hWnd;
        private int currentX = 747, currentY = 114;

        public WindowClickTools(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);


        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint WM_MOUSEMOVE = 0x0200;
        const uint WM_CHAR = 0x0102;
        const uint WM_SETTEXT = 0x000C;
        const uint WM_IME_CHAR = 0x0286;
        const int VK_BACK = 0x08; // 退格键的虚拟键码
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        static readonly IntPtr HWND_TOP = new IntPtr(0);

        public void InitWindows()
        {
            SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE);
        }

        public static IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        public void SendKey(ushort key)
        {
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)key, IntPtr.Zero);
            Thread.Sleep(50);
            SendMessage(hWnd, WM_KEYUP, (IntPtr)key, IntPtr.Zero);
        }

        public void SendEnterKey()
        {
            const int VK_RETURN = 0x0D;
            PostMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_RETURN, IntPtr.Zero);
            Thread.Sleep(50);
            PostMessage(hWnd, WM_KEYUP, (IntPtr)VK_RETURN, IntPtr.Zero);
        }
        public void SendBackspaceKey()
        {
            // 发送退格键按下消息
            PostMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_BACK, IntPtr.Zero);
            Thread.Sleep(50); // 稍作延迟

            // 发送退格键抬起消息
            PostMessage(hWnd, WM_KEYUP, (IntPtr)VK_BACK, IntPtr.Zero);
        }
        public void SendTabKey()
        {
            const int VK_TAB = 0x09;
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_TAB, IntPtr.Zero);
            Thread.Sleep(50);
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_TAB, IntPtr.Zero);
        }

        public void PushCopy()
        {
            const int VK_CONTROL = 0x11;  // Ctrl key
            const int VK_C = 0x43;        // 'C' key

            // Ctrl down
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_CONTROL, IntPtr.Zero);
            Thread.Sleep(50);
            // 'C' down
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_C, IntPtr.Zero);
            Thread.Sleep(50);
            // 'C' up
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_C, IntPtr.Zero);
            // Ctrl up
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_CONTROL, IntPtr.Zero);
        }
        //快捷键 开关任务栏
        public void PushAltQ()
        {
            const int VK_ALT = 0x12;  // ALT key
            const int VK_Q = 0x51;    // 'Q' key
            // Ctrl down
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_ALT, IntPtr.Zero);
            Thread.Sleep(50);
            // 'C' down
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_Q, IntPtr.Zero);
            Thread.Sleep(50);
            // 'C' up
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_Q, IntPtr.Zero);
            // Ctrl up
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_ALT, IntPtr.Zero);
        }

        public void PushAltKey(string key)
        {
            const int VK_ALT = 0x12;  // ALT key

            // 验证输入的字母是否合法
            if (string.IsNullOrEmpty(key) || key.Length != 1 || !char.IsLetter(key[0]))
            {
                throw new ArgumentException("参数必须是一个字母字符");
            }

            // 转换字母为大写并获取其虚拟键码
            char upperKey = char.ToUpper(key[0]);
            int vkKey = upperKey;

            // ALT down
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)VK_ALT, IntPtr.Zero);
            Thread.Sleep(50);

            // Key down
            SendMessage(hWnd, WM_KEYDOWN, (IntPtr)vkKey, IntPtr.Zero);
            Thread.Sleep(50);

            // Key up
            SendMessage(hWnd, WM_KEYUP, (IntPtr)vkKey, IntPtr.Zero);

            // ALT up
            SendMessage(hWnd, WM_KEYUP, (IntPtr)VK_ALT, IntPtr.Zero);
        }

        public void PushMsg(string text)
        {
            foreach (char c in text)
            {
                SendMessage(hWnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
                Thread.Sleep(50);  // 延迟确保目标窗口处理字符
            }
        }

      
        public void SendText(string text)
        {
            foreach (char c in text)
            {
                SendMessage(hWnd, WM_IME_CHAR, (IntPtr)c, IntPtr.Zero);  // 逐字符发送
                Thread.Sleep(50); // 稍作延迟，确保每个字符都被接收
            }
        }
        public void SendString(string str)
        {
            foreach (char c in str)
            {
                ushort vk = 0;
                if (char.IsLetterOrDigit(c))
                {
                    vk = (ushort)char.ToUpper(c);
                    SendKey(vk);
                }
                else if (c == ' ')
                {
                    SendKey(0x20); // 空格键
                }
            }
        }

        public void PushClick(int x, int y, bool isRightClick = false, bool hasOffset = true)
        {
            if (hasOffset)
            {
                x -= 2;
                y -= 26;
            }

         
            currentX = x;
            currentY = y;

            uint downMessage = isRightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint upMessage = isRightClick ? WM_RBUTTONUP : WM_LBUTTONUP;

            SendMessage(hWnd, downMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(100);
            SendMessage(hWnd, upMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(200);
        }

        public void MoveClick(int x, int y, bool isRightClick = false, bool hasOffset = true)
        {
            if (hasOffset)
            {
                x -= 2;
                y -= 26;
            }

            int steps = 30;
            int sleepTime = 50 / steps;

            float stepX = (float)(x - currentX) / steps;
            float stepY = (float)(y - currentY) / steps;

            for (int i = 1; i <= steps; i++)
            {
                int nextX = currentX + (int)(stepX * i);
                int nextY = currentY + (int)(stepY * i);
                SendMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(nextX, nextY));
                Thread.Sleep(sleepTime);
            }
            currentX = x;
            currentY = y;

            uint downMessage = isRightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint upMessage = isRightClick ? WM_RBUTTONUP : WM_LBUTTONUP;

            SendMessage(hWnd, downMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(100);
            SendMessage(hWnd, upMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(200);
        }
        public void Click(int x, int y, bool isRightClick = false, bool hasOffset = true)
        {
            if (hasOffset)
            {
                x -= 2;
                y -= 26;
            }
            currentX = x;
            currentY = y;
            uint downMessage = isRightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint upMessage = isRightClick ? WM_RBUTTONUP : WM_LBUTTONUP;
            SendMessage(hWnd, downMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(100);
            SendMessage(hWnd, upMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(100);
        }
        public void MoveMouse(int x, int y, bool hasOffset = true)
        {
            if (hasOffset)
            {
                x -= 2;
                y -= 26;
            }

            int steps = 30;
            int sleepTime = 100 / steps;

            float stepX = (float)(x - currentX) / steps;
            float stepY = (float)(y - currentY) / steps;

            for (int i = 1; i <= steps; i++)
            {
                int nextX = currentX + (int)(stepX * i);
                int nextY = currentY + (int)(stepY * i);
                SendMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(nextX, nextY));
                Thread.Sleep(sleepTime);
            }
            currentX = x;
            currentY = y;
        }

        public void MoveMouse(Point? point, bool hasOffset = true)
        {
            if (point == null)
            {
                return;
            }
            var x = point.Value.X;
            var y = point.Value.Y;
            if (hasOffset)
            {
                x -= 2;
                y -= 26;
            }

            int steps = 30;
            int sleepTime = 100 / steps;

            float stepX = (float)(x - currentX) / steps;
            float stepY = (float)(y - currentY) / steps;

            for (int i = 1; i <= steps; i++)
            {
                int nextX = currentX + (int)(stepX * i);
                int nextY = currentY + (int)(stepY * i);
                SendMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(nextX, nextY));
                Thread.Sleep(sleepTime);
            }
            currentX = x;
            currentY = y;
        }
        public void MouseHover(int x, int y, bool hasOffset = true)
        {
            if (hasOffset)
            {
                x -= 2;
                y -= 26;
            }
            SendMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(x, y));
        }
    }
}
