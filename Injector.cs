using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace TTTools
{
    internal class Injector
    {
        private IntPtr gameWindowHandle;

        // 构造函数，接收目标游戏窗口的句柄
        public Injector(IntPtr gameWindowHandle)
        {
            this.gameWindowHandle = gameWindowHandle;
        }

        // 从 kernel32.dll 导入 OpenProcess 函数，用于获取目标进程的句柄
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        // 从 kernel32.dll 导入 GetProcAddress 函数，用于获取指定模块中函数的地址
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // 从 kernel32.dll 导入 GetModuleHandle 函数，用于获取指定模块的句柄
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // 从 kernel32.dll 导入 VirtualAllocEx 函数，用于在目标进程中分配内存
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        // 从 kernel32.dll 导入 WriteProcessMemory 函数，用于写入目标进程的内存
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out int lpNumberOfBytesWritten);

        // 从 kernel32.dll 导入 CreateRemoteThread 函数，用于在目标进程中创建新线程
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        // 此方法用于将 DLL 注入到目标游戏窗口
        public void Inject(string dllPath)
        {
            uint processId;
            GetWindowThreadProcessId(gameWindowHandle, out processId);

            IntPtr hProcess = OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, (int)processId);

            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), 0x1000 | 0x2000, 0x04);

            int bytesWritten;
            WriteProcessMemory(hProcess, allocMemAddress, Encoding.Default.GetBytes(dllPath), (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);

            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, out IntPtr threadId);

            // 等待远程线程执行完成
            WaitForSingleObject(hThread, 0xFFFFFFFF);

            // 清理
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }

    

        // 从 user32.dll 导入 GetWindowThreadProcessId 函数，用于获取窗口的线程和进程 ID
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

      

    }
}
