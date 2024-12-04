using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TTTools.windowsTools
{
    public class ResourceLoader
    {
        /// <summary>
        /// 从嵌入资源中加载 Bitmap 对象
        /// </summary>
        /// <param name="resourcePath">嵌入资源的路径（命名空间.路径.文件名）</param>
        /// <returns>Bitmap 对象</returns>
        public static Bitmap LoadBitmap(string resourcePath)
        {
            // 获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 从嵌入资源中获取流
            using (Stream stream = assembly.GetManifestResourceStream($"TTTools.{resourcePath}"))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"未找到嵌入资源: {resourcePath}");
                }

                // 加载并返回 Bitmap 对象
                return new Bitmap(stream);
            }
        }
    }
}
