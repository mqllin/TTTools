using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
namespace TTTools
{
    class IniFileHelper
    {
        private string iniFilePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public IniFileHelper(string iniFileName)
        {
            // 获取程序运行的目录
            string dir = AppDomain.CurrentDomain.BaseDirectory;

            // 拼接完整的文件路径
            iniFilePath = Path.Combine(dir, iniFileName);

            // 创建文件如果它不存在
            if (!File.Exists(iniFilePath))
            {
                File.Create(iniFilePath).Close();
            }
        }

        public void IniWriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, iniFilePath);
        }

        public string IniReadValue(string section, string key, string defaultValue = "")
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, defaultValue, temp, 255, iniFilePath);
            return temp.ToString();
        }
    }
}
