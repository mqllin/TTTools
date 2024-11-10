using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTools.windowsTools
{
    class WindowOperationsAsync
    {
        private readonly nint hWnd;
        private readonly Form1 Instance;

        // 假设当前鼠标的位置为 (currentX, currentY)
        private int currentX = 747, currentY = 114; // 默认初始位置

        public WindowOperationsAsync(nint hWnd, Form1 Instance)
        {
            this.hWnd = hWnd;
            this.Instance = Instance;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(nint hwnd, string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(nint hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));

            [StructLayout(LayoutKind.Explicit)]
            public struct InputUnion
            {
                [FieldOffset(0)] public MOUSEINPUT mi;
                [FieldOffset(0)] public KEYBDINPUT ki;
                [FieldOffset(0)] public HARDWAREINPUT hi;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public nint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public long time;
            public uint dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        static readonly nint HWND_TOP = new nint(0);

        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint WM_MOUSEMOVE = 0x0200;

        public async Task InitWindowsAsync()
        {
            SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE);
            await Task.Delay(10); // 微小延迟以确保窗口设置
        }

        public static nint MakeLParam(int x, int y)
        {
            return y << 16 | x & 0xFFFF;
        }

        public async Task SendKeyAsync(ushort key)
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0] = new INPUT
            {
                type = 1,
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        dwFlags = 0 // Key down
                    }
                }
            };

            inputs[1] = new INPUT
            {
                type = 1,
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        dwFlags = 2 // KEYEVENTF_KEYUP
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, INPUT.Size);
            await Task.Delay(50); // 延迟确保按键生效
        }

        public async Task SendStringAsync(string str)
        {
            foreach (char c in str)
            {
                await Task.Delay(100);
                ushort vk = char.IsLetterOrDigit(c) ? char.ToUpper(c) : (ushort)0;

                if (vk != 0)
                {
                    await SendKeyAsync(vk);
                }
            }
        }

        public async Task SendTabKeyAsync()
        {
            SetForegroundWindow(hWnd);
            await Task.Delay(500);

            await SendKeyAsync(0x09); // VK_TAB
        }

        public async Task SendEnterKeyAsync()
        {
            await SendKeyAsync(0x0D); // VK_RETURN
        }

        public async Task PushCopyAsync()
        {
            SetForegroundWindow(hWnd);
            await Task.Delay(500);
            SendKeys.SendWait("^v");
            await Task.Delay(100);
        }

        public async Task PushMsgAsync(string text)
        {
            SetForegroundWindow(hWnd);

            List<INPUT> inputs = new List<INPUT>();

            foreach (var c in text)
            {
                inputs.Add(new INPUT
                {
                    type = 1,
                    U = new INPUT.InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = c,
                            dwFlags = 4 // KEYEVENTF_UNICODE
                        }
                    }
                });

                inputs.Add(new INPUT
                {
                    type = 1,
                    U = new INPUT.InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = c,
                            dwFlags = 4 | 2 // KEYEVENTF_UNICODE | KEYEVENTF_KEYUP
                        }
                    }
                });
            }

            SendInput((uint)inputs.Count, inputs.ToArray(), INPUT.Size);
            await Task.Delay(50 * text.Length); // 延迟以确保每个字符的输入间隔
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        const uint WM_SETTEXT = 0x000C;
        public void SendChineseText(string text)
        {
            SendMessage(hWnd, 0x000C, nint.Zero, text); // WM_SETTEXT
        }

        public async Task PushClickAsync(int x, int y, bool isRightClick = false, bool hasOffset = true)
        {
            if (hasOffset)
            {
                y -= 26;
                x -= 2;
            }

            int steps = 60;
            int sleepTime = 1000 / steps;

            float stepX = (float)(x - currentX) / steps;
            float stepY = (float)(y - currentY) / steps;

            for (int i = 1; i <= steps; i++)
            {
                int nextX = currentX + (int)(stepX * i);
                int nextY = currentY + (int)(stepY * i);

                SendMessage(hWnd, WM_MOUSEMOVE, nint.Zero, MakeLParam(nextX, nextY));
                await Task.Delay(sleepTime);
            }
            currentX = x;
            currentY = y;

            uint downMessage = isRightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint upMessage = isRightClick ? WM_RBUTTONUP : WM_LBUTTONUP;

            await Task.Delay(100);
            SendMessage(hWnd, downMessage, nint.Zero, MakeLParam(x, y));
            await Task.Delay(100);
            SendMessage(hWnd, upMessage, nint.Zero, MakeLParam(x, y));
            await Task.Delay(100);
        }

        public async Task MoveMouseAsync(int x, int y, bool hasOffset = true)
        {
            if (hasOffset)
            {
                y -= 26;
                x -= 2;
            }

            int steps = 60;
            int sleepTime = 1000 / steps;

            float stepX = (float)(x - currentX) / steps;
            float stepY = (float)(y - currentY) / steps;

            for (int i = 1; i <= steps; i++)
            {
                int nextX = currentX + (int)(stepX * i);
                int nextY = currentY + (int)(stepY * i);

                SendMessage(hWnd, WM_MOUSEMOVE, nint.Zero, MakeLParam(nextX, nextY));
                await Task.Delay(sleepTime);
            }
            currentX = x;
            currentY = y;
        }

        public async Task MouseHoverAsync(int x, int y, bool hasOffset = true)
        {
            if (hasOffset)
            {
                y -= 26;
                x -= 2;
            }

            SendMessage(hWnd, WM_MOUSEMOVE, nint.Zero, MakeLParam(x, y));
            await Task.Delay(50);
        }
    }
}
