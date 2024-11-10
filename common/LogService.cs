using System;
using System.Windows.Forms;

namespace TTTools
{
    public class LogService
    {
        // 控制是否开启调试输出的全局变量
        public static bool IsDebugEnabled { get; set; } = false;

        /// <summary>
        /// 始终输出日志信息到Form1的文本框
        /// </summary>
        /// <param name="message">要输出的日志信息</param>
        public static void Log(string message)
        {
            if (Form1.MainForm != null && Form1.MainForm.IsHandleCreated)
            {
                Form1.MainForm.Invoke(new Action(() =>
                {
                    Form1.MainForm.Dbug(message);
                }));
            }
        }

        /// <summary>
        /// 仅在IsDebugEnabled为true时输出调试信息
        /// </summary>
        /// <param name="message">要输出的调试信息</param>
        public static void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                Log(message);
            }
        }
    }
}
