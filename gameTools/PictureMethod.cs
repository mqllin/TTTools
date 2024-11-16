using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace TTTools
{
    class PictureMethod
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern IntPtr BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, int rop);

        const int SRCCOPY = 0x00CC0020;

        public IntPtr hWnd;

        public PictureMethod(IntPtr hWnd)
        {
            this.hWnd = hWnd;
           

        }

        public Bitmap CaptureWindow(int x, int y, int width, int height)
        {
            IntPtr hSrcDC = IntPtr.Zero;
            IntPtr hDestDC = IntPtr.Zero;
            Bitmap bmp = null;

            try
            {
                hSrcDC = GetDC(hWnd);
                bmp = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    hDestDC = graphics.GetHdc();

                    BitBlt(hDestDC, 0, 0, width, height, hSrcDC, x, y, SRCCOPY);

                    graphics.ReleaseHdc(hDestDC);
                }
            }
            finally
            {
                // Explicitly release the device contexts.
                if (hDestDC != IntPtr.Zero)
                {
                    ReleaseDC(hWnd, hDestDC);
                }
                if (hSrcDC != IntPtr.Zero)
                {
                    ReleaseDC(hWnd, hSrcDC);
                }
            }

            return bmp;
        }


        public bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Size != bmp2.Size) return false;

            for (int y = 0; y < bmp1.Height; ++y)
            {
                for (int x = 0; x < bmp1.Width; ++x)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public string SaveImage(Bitmap image)
        {
            // 获取程序运行目录
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string tempDirectory = Path.Combine(currentDirectory, "temp");

            // 检查 /temp 目录是否存在，如果不存在则创建
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            // 使用当前的Unix时间戳作为文件名
            string timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            string tempFilePath = Path.Combine(tempDirectory, $"temp_ocr_{timestamp}.png");

            // 将Bitmap对象保存为临时文件
            image.Save(tempFilePath);
            return tempFilePath;
        }


        public List<Point> FindBitmapInWindow(Bitmap toFind)
        {
            int width = 806;  // 定义窗口截图的宽度
            int height = 692; // 定义窗口截图的高度
            Bitmap fullCapture2 = CaptureWindow(0, 0, width, height);
            SaveImage(fullCapture2);
            LogService.Debug(fullCapture2.ToString());

            using (Bitmap fullCapture = CaptureWindow(0, 0, width, height))
            {
                SaveImage(toFind);
                List<Point> foundPoints = new List<Point>();

                var dataFullCapture = fullCapture.LockBits(new Rectangle(0, 0, fullCapture.Width, fullCapture.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var dataToFind = toFind.LockBits(new Rectangle(0, 0, toFind.Width, toFind.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* ptrFullCapture = (byte*)dataFullCapture.Scan0;
                    byte* ptrToFind = (byte*)dataToFind.Scan0;

                    int strideFullCapture = dataFullCapture.Stride;
                    int strideToFind = dataToFind.Stride;

                    for (int y = 0; y < fullCapture.Height - toFind.Height; y++)
                    {
                        for (int x = 0; x < fullCapture.Width - toFind.Width; x++)
                        {
                            bool found = true;
                            for (int y1 = 0; y1 < toFind.Height; y1++)
                            {
                                for (int x1 = 0; x1 < toFind.Width; x1++)
                                {
                                    int indexFullCapture = (y + y1) * strideFullCapture + (x + x1) * 4;
                                    int indexToFind = y1 * strideToFind + x1 * 4;

                                    if (*(int*)(ptrFullCapture + indexFullCapture) != *(int*)(ptrToFind + indexToFind))
                                    {
                                        found = false;
                                        break;
                                    }
                                }
                                if (!found) break;
                            }
                            if (found)
                            {
                                foundPoints.Add(new Point(x, y));
                            }
                        }
                    }
                }

                toFind.UnlockBits(dataToFind);
                fullCapture.UnlockBits(dataFullCapture);

                return foundPoints;
            }
        }
        public bool CheckIfWindowContentChanged()
        {
            int x = 126;
            int y = 87;
            int width = 260;
            int height = 216;

            using (Bitmap firstCapture = CaptureWindow(x, y, width, height))
            {
                Thread.Sleep(2000);
                using (Bitmap secondCapture = CaptureWindow(x, y, width, height))
                {
                    return !CompareBitmaps(firstCapture, secondCapture);
                }
            }
        }

        public bool CheckIsMoving()
        {
            int x = 679;
            int y = 43;
            int width = 119;
            int height = 85;

            using (Bitmap firstCapture = CaptureWindow(x, y, width, height))
            {
                Thread.Sleep(500);
                using (Bitmap secondCapture = CaptureWindow(x, y, width, height))
                {
                    return !CompareBitmaps(firstCapture, secondCapture);
                }
            }
        }

        //判断背包是否打开，如果打开了，返回第一个物品的位置
        public Point? IsBackpackOpen()
        {
            // 定义背包特征图片路径
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagePath = Path.Combine(currentDirectory, "data", "ui", "zhuangbeidaoju.png");

            // 检查特征图片文件是否存在
            if (!File.Exists(imagePath))
            {
                LogService.Log("背包特征图片不存在: " + imagePath);
                return null;
            }

            // 加载背包特征图片
            using (Bitmap backpackBitmap = new Bitmap(imagePath))
            {
                // 在当前窗口截图中寻找特征图片的位置
                List<Point> foundPoints = FindBitmapInWindow(backpackBitmap);

                // 如果找到匹配点，则返回第一个匹配点的坐标
                if (foundPoints.Count > 0)
                {
                    return new Point
                    {
                        X = foundPoints[0].X-106,
                        Y = foundPoints[0].Y+260
                    };
                }
            }

            // 没有找到匹配项，返回 null
            return null;
        }
        //寻找背包内的物品
        public Point? FindSomeInBackpack(string name)
        {
            // 定义背包特征图片路径
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagePath = Path.Combine(currentDirectory, "data", "ui", $"{name}.png");

            // 检查特征图片文件是否存在
            if (!File.Exists(imagePath))
            {
                LogService.Log($"{name}特征图片不存在: " + imagePath);
                return null;
            }

            // 加载背包特征图片
            using (Bitmap someThingImage = new Bitmap(imagePath))
            {
                // 在当前窗口截图中寻找特征图片的位置
                List<Point> foundPoints = FindBitmapInWindow(someThingImage);

                // 如果找到匹配点，则返回第一个匹配点的坐标
                if (foundPoints.Count > 0)
                {
                    return new Point
                    {
                        X = foundPoints[0].X + 20,
                        Y = foundPoints[0].Y + 20
                    };

                }
            }

            // 没有找到匹配项，返回 null
            return null;
        }
    }
}
