using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Drawing.Imaging;
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

        // 记录坐标图片数据
        private readonly Dictionary<char, Bitmap> digitBitmaps = new Dictionary<char, Bitmap>();
        private readonly Dictionary<String, Bitmap> mapNameBitmaps = new Dictionary<String, Bitmap>();


        public PictureMethod(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            LoadDigitBitmaps();
            LoadMapNameBitmaps();

        }

        
        public Bitmap CaptureWindow(int x, int y, int width, int height)
        {
            IntPtr hSrcDC = IntPtr.Zero;
            IntPtr hDestDC = IntPtr.Zero;
            Bitmap bmp = null;

            try
            {
                hSrcDC = GetDC(hWnd);
                bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
            //SaveImage(fullCapture2);
            LogService.Debug(fullCapture2.ToString());

            using (Bitmap fullCapture = CaptureWindow(0, 0, width, height))
            {
                //SaveImage(toFind);
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
        // 寻找目标是否在传入的截图中
        public Point? FindBitmapInScreenshot(Bitmap screenshot, Bitmap target)
        {
            // 确保截图和目标图片不为空
            if (screenshot == null || target == null)
            {
                throw new ArgumentNullException("截图或目标图片不能为空。");
            }

            // 如果目标图片比截图大，直接返回 null
            if (target.Width > screenshot.Width || target.Height > screenshot.Height)
            {
                return null;
            }

            // 锁定截图和目标图片的像素数据
            var screenshotData = screenshot.LockBits(
                new Rectangle(0, 0, screenshot.Width, screenshot.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var targetData = target.LockBits(
                new Rectangle(0, 0, target.Width, target.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* screenshotPtr = (byte*)screenshotData.Scan0;
                    byte* targetPtr = (byte*)targetData.Scan0;

                    int screenshotStride = screenshotData.Stride;
                    int targetStride = targetData.Stride;

                    // 遍历截图的每个像素，尝试匹配目标图片
                    for (int y = 0; y <= screenshot.Height - target.Height; y++)
                    {
                        for (int x = 0; x <= screenshot.Width - target.Width; x++)
                        {
                            bool isMatch = true;

                            // 检查当前区域是否匹配目标图片
                            for (int ty = 0; ty < target.Height && isMatch; ty++)
                            {
                                for (int tx = 0; tx < target.Width; tx++)
                                {
                                    int screenshotIndex = (y + ty) * screenshotStride + (x + tx) * 4;
                                    int targetIndex = ty * targetStride + tx * 4;

                                    // 比较 RGBA 值
                                    if (*(int*)(screenshotPtr + screenshotIndex) != *(int*)(targetPtr + targetIndex))
                                    {
                                        isMatch = false;
                                        break;
                                    }
                                }
                            }

                            // 如果找到匹配的区域，返回左上角坐标
                            if (isMatch)
                            {
                                return new Point(x, y);
                            }
                        }
                    }
                }
            }
            finally
            {
                // 解锁像素数据
                screenshot.UnlockBits(screenshotData);
                target.UnlockBits(targetData);
            }

            // 如果未找到匹配的目标，返回 null
            return null;
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
                        X = foundPoints[0].X - 106,
                        Y = foundPoints[0].Y + 260
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




        // 加载数字图片数据到内存
        // 加载数字图片数据到内存
        private void LoadDigitBitmaps()
        {
            string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "xy");
            string[] files = { "0.png", "1.png", "2.png", "3.png", "4.png", "5.png", "6.png", "7.png", "8.png", "9.png", "point.png" };
            char[] characters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };

            if (!Directory.Exists(imageDirectory))
            {
                throw new DirectoryNotFoundException("图片资源目录未找到: " + imageDirectory);
            }

            for (int i = 0; i < files.Length; i++)
            {
                string filePath = Path.Combine(imageDirectory, files[i]);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("数字图片未找到: " + filePath);
                }

                using (Bitmap bmp = new Bitmap(filePath))
                {
                    // 将Bitmap克隆到缓存中，确保原图片资源被释放
                    Bitmap cachedBitmap = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(cachedBitmap))
                    {
                        g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    }

                    digitBitmaps[characters[i]] = cachedBitmap;
                }
            }
        }
        private void LoadMapNameBitmaps()
        {
            // 定义地图名称图片所在目录
            string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mapName");

            // 检查目录是否存在
            if (!Directory.Exists(imageDirectory))
            {
                throw new DirectoryNotFoundException($"地图名称图片资源目录未找到: {imageDirectory}");
            }

            // 获取目录下所有 .png 文件
            string[] files = Directory.GetFiles(imageDirectory, "*.png");

            foreach (string filePath in files)
            {
                // 获取文件名（去掉扩展名）作为键
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // 加载图片
                using (Bitmap bmp = new Bitmap(filePath))
                {
                    // 克隆图片到内存，确保原资源释放
                    Bitmap cachedBitmap = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(cachedBitmap))
                    {
                        g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    }

                    // 添加到字典中
                    mapNameBitmaps[fileName] = cachedBitmap;
                }
            }

            LogService.Log($"地图名称图片加载完成，共加载了 {mapNameBitmaps.Count} 张图片。");
        }



        // 预处理截图，将非白色部分设置为透明
        private Bitmap PreprocessScreenshot(Bitmap screenshot)
        {
            Bitmap processed = new Bitmap(screenshot.Width, screenshot.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // 定义白色的容错范围
            const int tolerance = 10;

            for (int y = 0; y < screenshot.Height; y++)
            {
                for (int x = 0; x < screenshot.Width; x++)
                {
                    Color pixel = screenshot.GetPixel(x, y);

                    // 判断像素是否接近白色
                    if (IsCloseToWhite(pixel, tolerance))
                    {
                        processed.SetPixel(x, y, pixel); // 保留近似白色像素
                    }
                    else
                    {
                        //LogService.Debug($"{x}, {y} pixel={pixel} set 0");
                        processed.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0)); // 设置透明
                    }
                }
            }

            return processed;
        }

        // 判断像素是否接近白色
        private bool IsCloseToWhite(Color color, int tolerance)
        {
            return Math.Abs(color.R - 255) <= tolerance &&
                   Math.Abs(color.G - 255) <= tolerance &&
                   Math.Abs(color.B - 255) <= tolerance;
        }
        public Point? FindXyInBitmap(Bitmap screenshot)
        {
            List<(char, Point)> foundCoordinates = new List<(char, Point)>();

            // 预处理截图，将非白色像素设置为透明
            Bitmap processedScreenshot = PreprocessScreenshot(screenshot);

            // 遍历所有缓存的数字模板
            foreach (var kvp in digitBitmaps)
            {
                char digit = kvp.Key;
                Bitmap digitBitmap = kvp.Value;
                bool isFind = false;

                // 滑动窗口比对
                for (int y = 0; y <= processedScreenshot.Height - digitBitmap.Height; y++)
                {
                    for (int x = 0; x <= processedScreenshot.Width - digitBitmap.Width; x++)
                    {
                        // 提取截图中的子区域
                        using (Bitmap subRegion = processedScreenshot.Clone(new Rectangle(x, y, digitBitmap.Width, digitBitmap.Height), processedScreenshot.PixelFormat))
                        {
                            // 判断是否匹配并验证左侧像素为空
                            if (CompareBitmapWithValidation(subRegion, digitBitmap, processedScreenshot, x, y, digit))
                            {
                                if (digit == '3')
                                {
                                    var img8 = digitBitmaps.ElementAt(8).Value;
                                    if (!CompareBitmapWithValidation(subRegion, img8, processedScreenshot, x, y, '8'))
                                    {
                                        foundCoordinates.Add((digit, new Point(x, y))); // 记录匹配到的字符及坐标
                                        isFind = true;
                                    }
                                }
                                else
                                {
                                    foundCoordinates.Add((digit, new Point(x, y))); // 记录匹配到的字符及坐标
                                }
                            }
                        }

                    }
                }
            }

            // 按 X 坐标排序
            foundCoordinates = foundCoordinates.OrderBy(c => c.Item2.X).ToList();

            // 如果没有找到任何匹配项，返回 null
            if (foundCoordinates.Count == 0)
            {
                return null;
            }

            // 根据坐标拼接字符串
            string extractedString = string.Concat(foundCoordinates.Select(c => c.Item1));

            // 尝试将字符串转换为 Point 类型
            if (TryParsePoint(extractedString, out Point result))
            {
                return result;
            }

            return null;
        }
        // 比较两个Bitmap，忽略透明部分，同时验证周围 1 像素是否透明
        private bool CompareBitmapWithValidation(Bitmap subRegion, Bitmap digitBitmap, Bitmap fullScreenshot, int startX, int startY, char digit)
        {
            if (subRegion.Width != digitBitmap.Width || subRegion.Height != digitBitmap.Height)
                return false;

            // 检查模板与子区域的匹配
            for (int y = 0; y < digitBitmap.Height; y++)
            {
                for (int x = 0; x < digitBitmap.Width; x++)
                {
                    Color pixel1 = subRegion.GetPixel(x, y);
                    Color pixel2 = digitBitmap.GetPixel(x, y);

                    // 忽略模板的透明像素
                    if (pixel2.A > 0 && pixel1 != pixel2)
                    {
                        return false; // 不匹配
                    }
                }
            }          

            // 验证周围 1 像素是否透明
            if (!IsSurroundingTransparent(fullScreenshot, startX, startY, digitBitmap.Width, digitBitmap.Height))
            {
                return false; // 周围像素不符合透明要求
            }
            return true;
        }

        // 尝试将字符串转换为 Point 类型
        private bool TryParsePoint(string input, out Point result)
        {
            result = Point.Empty;

            // 检查字符串是否包含逗号分隔的两个数字
            string[] parts = input.Split('.');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int x) &&
                int.TryParse(parts[1], out int y))
            {
                result = new Point(x, y);
                return true;
            }

            return false;
        }


        // 验证周围 1 像素是否透明
        private bool IsSurroundingTransparent(Bitmap fullScreenshot, int startX, int startY, int width, int height)
        {
            for (int y = startY - 1; y <= startY + height; y++)
            {
                for (int x = startX - 1; x <= startX + width; x++)
                {
                    // 跳过模板区域本身
                    if (y >= startY && y < startY + height && x >= startX && x < startX + width)
                    {
                        continue;
                    }

                    // 检查是否在截图范围内
                    if (x >= 0 && x < fullScreenshot.Width && y >= 0 && y < fullScreenshot.Height)
                    {
                        Color surroundingPixel = fullScreenshot.GetPixel(x, y);

                        // 如果周围的像素不透明，返回 false
                        if (surroundingPixel.A > 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true; // 所有周围像素均透明
        }
        /**
         * 获取当前地图名称
         */
        public string GetCurrentMapName()
        {
            // 定义截图区域
            int x = 686;
            int y = 19;
            int width = 110;
            int height = 16;
            Bitmap a = CaptureWindow(x, y, width, height);
            SaveImage(a);
            // 截取指定区域的截图
            using (Bitmap screenshot = CaptureWindow(x, y, width, height))
            {
               
                // 遍历 mapNameBitmaps 中的所有地图名称图片
                foreach (var entry in mapNameBitmaps)
                {
                    string mapName = entry.Key;
                    Bitmap mapBitmap = entry.Value;
                    SaveImage(mapBitmap);
                    SaveImage(screenshot);
                    var find = FindBitmapInScreenshot(mapBitmap, screenshot);
                    if (find!=null)
                    {
                        return mapName;
                    }
                       
                }
            }

            // 如果没有匹配项，返回 null 或空字符串
            return null;
        }


    }
}
