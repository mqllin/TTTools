using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace TTTools
{
    public class LoginMethod
    {
        string username;
        string password;
        int userInedx;
        private readonly Form1 Instance;

        IniFileHelper iniFileHelper = new IniFileHelper("settings.ini");

        public LoginMethod(string username, string password, int userInedx, Form1 instance)
        {
            this.username = username;
            this.password = password;
            this.userInedx = userInedx;
            Instance = instance;
        }


        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static IntPtr FindWindowByProcessId(int processId)
        {
            IntPtr windowHandle = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                uint winProcessId;
                GetWindowThreadProcessId(hWnd, out winProcessId);
                if (winProcessId == processId)
                {
                    windowHandle = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return windowHandle;
        }

        public void LoginGame()
        {
            // 从textBox6获取游戏客户端的目录路径
            string gameDir = iniFileHelper.IniReadValue("gamepath", "path", "");
            //判断gameDir不为空
            if (gameDir == "")
            {
                LogService.Log("游戏目录为空");
                return;
            }

            // 拼接完整的应用目录
            string fullGamePath = System.IO.Path.Combine(gameDir, "ggegame.exe");

            // 启动游戏客户端
            Process gameProcess = new Process();
            gameProcess.StartInfo.FileName = fullGamePath;
            gameProcess.StartInfo.WorkingDirectory = gameDir;  // 设置工作目录
            gameProcess.Start();

            // 获取新创建进程的PID
            int processId = gameProcess.Id;

            // 等待游戏客户端启动（可选）
            gameProcess.WaitForInputIdle();

            // 通过PID获取窗口句柄
            IntPtr gameWindowHandle = FindWindowByProcessId(processId);
            LogService.Log("游戏窗口:" + gameWindowHandle);


            // 使用 WindowApi 设置窗口位置到屏幕左上角 (0, 0)
            if (!WindowApi.SetWindowPosition(gameWindowHandle, 0, 0))
            {
                LogService.Log("无法设置窗口位置。");
                return;
            }

            var api = new Method(gameWindowHandle);
            api.InitWindows();

            // 绝对路径或确保相对路径正确
            string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "dll", "Dll1.dll");
            if (gameWindowHandle == IntPtr.Zero)
            {
                LogService.Log("找不到游戏窗口句柄。");
                return;
            }

            // 现在你有了游戏窗口的句柄，可以继续你的其他操作
            api.loginAuto(userInedx, username, password);
            //api.PerformLoginAsync(userInedx,username,password);
          
        }
    }
}
