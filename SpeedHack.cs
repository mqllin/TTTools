using HarmonyLib;
using System;
using System.Runtime.InteropServices;

namespace TTTools
{
    public class SpeedHack
    {
        // 导入speedhack-x86_64.dll中的InitializeSpeedhack函数
        [DllImport(@"data\dll\speedhack-x86_64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "InitializeSpeedhack")]
        public static extern void InitializeSpeedhack(float speed);

        [DllImport(@"data\dll\speedhack-x86_64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "speedhackversion_GetTickCount")]
        public static extern uint speedhackversion_GetTickCount();

        [DllImport(@"data\dll\speedhack-x86_64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "speedhackversion_QueryPerformanceCounter")]
        public static extern bool speedhackversion_QueryPerformanceCounter(out long lpPerformanceCount);

        // 导入kernel32.dll中的GetTickCount和QueryPerformanceCounter函数
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        public static void ChangeSpeed(float newSpeed)
        {
            var harmony = new Harmony("com.example.speedhack");  // 创建一个新的Harmony实例
            // 创建钩子，将原始的GetTickCount和QueryPerformanceCounter函数替换为自定义的实现
            harmony.Patch(
                original: AccessTools.Method(typeof(SpeedHack), nameof(GetTickCount)),
                prefix: new HarmonyMethod(typeof(MyPatches), nameof(MyPatches.Prefix_GetTickCount))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SpeedHack), nameof(QueryPerformanceCounter)),
                prefix: new HarmonyMethod(typeof(MyPatches), nameof(MyPatches.Prefix_QueryPerformanceCounter))
            );
            InitializeSpeedhack(newSpeed);

        }

        public static class MyPatches
        {
            // 自定义的GetTickCount实现
            public static bool Prefix_GetTickCount(ref uint __result)
            {
                __result = speedhackversion_GetTickCount();
                // 替换为调用speedhackversion_GetTickCount的代码
                return false;  // 告诉Harmony跳过原始方法
            }

            // 自定义的QueryPerformanceCounter实现
            public static bool Prefix_QueryPerformanceCounter(out long lpPerformanceCount)
            {
                bool result = speedhackversion_QueryPerformanceCounter(out lpPerformanceCount);
                return false;  // 告诉Harmony跳过原始方法
            }
        }
    }
}
