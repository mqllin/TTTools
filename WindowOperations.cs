using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks; // 引入Task库
using System.Threading;
using System.Windows.Forms;

namespace TTTools
{
    class WindowOperations
    {
        private readonly IntPtr hWnd;
        private readonly Form1 Instance;

        // 假设当前鼠标的位置为 (currentX, currentY)
        private int currentX = 747, currentY = 114; // 你需要获取这两个值747, 114
        public WindowOperations(IntPtr hWnd, Form1 Instance)
        {
            this.hWnd = hWnd;
            this.Instance = Instance;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

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

        // 这里添加了 MOUSEINPUT 和 HARDWAREINPUT 结构，以便能够使用 InputUnion
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
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

        static readonly IntPtr HWND_TOP = new IntPtr(0);

        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint WM_MOUSEMOVE = 0x0200;

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
            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        dwFlags = 0 // Key down
                    }
                }
            };

            // Key up
            inputs[1] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
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
        }
        public void SendString(string str)
        {
            foreach (char c in str)
            {
                Thread.Sleep(100);


                ushort vk = 0;

                // 如果是字母
                if (char.IsLetter(c))
                {
                    vk = (ushort)(char.ToUpper(c)); // 转换为大写字母的ASCII码
                }
                // 如果是数字
                else if (char.IsDigit(c))
                {
                    vk = (ushort)(c); // 直接使用数字的ASCII码
                }

                if (vk != 0)
                {
                    SendKey(vk); // 调用之前定义的 SendKey 方法
                }
            }
        }
        public void SendTabKey()
        {
            SetForegroundWindow(hWnd); // 将窗口设置为前台窗口
            Thread.Sleep(500);

            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x09, // VK_TAB
                        dwFlags = 0 // Key down
                    }
                }
            };

            // Key up
            inputs[1] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x09, // VK_TAB
                        dwFlags = 2 // KEYEVENTF_KEYUP
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        public void SendEnterKey()
        {
            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x0D, // VK_RETURN
                        dwFlags = 0 // Key down
                    }
                }
            };

            // Key up
            inputs[1] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                U = new INPUT.InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x0D, // VK_RETURN
                        dwFlags = 2 // KEYEVENTF_KEYUP
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }
        public void PushCopy() {
            SetForegroundWindow(hWnd); // 将窗口设置为前台窗口
            Thread.Sleep(1000);

            SendKeys.SendWait("^v");
        }
        public void PushMsg(string text)
        {
            SetForegroundWindow(hWnd); // 将窗口设置为前台窗口

            List<INPUT> inputs = new List<INPUT>();

            foreach (var c in text)
            {
                // Key down
                inputs.Add(new INPUT
                {
                    type = 1, // INPUT_KEYBOARD
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

                // Key up
                inputs.Add(new INPUT
                {
                    type = 1, // INPUT_KEYBOARD
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
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        const uint WM_SETTEXT = 0x000C;

        public void SendChineseText( string text)
        {
            SendMessage(hWnd, WM_SETTEXT, IntPtr.Zero, text);
        }


        public void PushClick(int x, int y, bool isRightClick = false,bool hasOffset = true)
        {
            if (hasOffset)
            {
                y = y - 26;
                x = x - 2;
            }

            int steps = 30; // 在1秒内移动50步
            int sleepTime = 100 / steps; // 每步需要暂停的时间（毫秒）

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
            //Instance.AppendGlobalLog("PushClick " + x + "," + y+" right="+ isRightClick);
            uint downMessage = isRightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN;
            uint upMessage = isRightClick ? WM_RBUTTONUP : WM_LBUTTONUP;
            //SendMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(500);
            SendMessage(hWnd, downMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(100);
            SendMessage(hWnd, upMessage, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(500);

        }
        public void MoveMouse(int x, int y, bool hasOffset = true)
        {
            if (hasOffset)
            {
                y = y - 26;
                x = x - 2;
            }

            int steps = 30; // 在1秒内移动50步
            int sleepTime = 100 / steps; // 每步需要暂停的时间（毫秒）

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
            if (hasOffset) {
                // 校正坐标
                y = y - 26;
                x = x - 2;
            }
            
            // 输出日志
            //Instance.AppendGlobalLog("MouseHover " + x + "," + y);
            // 发送WM_MOUSEMOVE消息
            SendMessage(hWnd, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(x, y));
        }
    }
}
