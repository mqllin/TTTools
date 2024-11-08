using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class DLLInjector
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const uint MEM_COMMIT = 0x00001000;
    private const uint PAGE_READWRITE = 0x04;

    public static bool InjectDLL(int targetProcessId, string dllPath)
    {
        IntPtr processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcessId);
        if (processHandle == IntPtr.Zero)
            return false;

        IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
        if (loadLibraryAddr == IntPtr.Zero)
            return false;

        IntPtr allocMemAddress = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT, PAGE_READWRITE);
        if (allocMemAddress == IntPtr.Zero)
            return false;

        byte[] dllPathBytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
        if (!WriteProcessMemory(processHandle, allocMemAddress, dllPathBytes, (uint)dllPathBytes.Length, 0))
            return false;

        if (CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero) == IntPtr.Zero)
            return false;

        return true;
    }

    public static void CallExportedFunction(string dllPath, string functionName)
    {
        IntPtr hModule = LoadLibrary(dllPath);
        if (hModule == IntPtr.Zero)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

        IntPtr procAddress = GetProcAddress(hModule, functionName);
        if (procAddress == IntPtr.Zero)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

        MyDLLFunctionDelegate del = (MyDLLFunctionDelegate)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(MyDLLFunctionDelegate));
        del();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void MyDLLFunctionDelegate();

  
}
