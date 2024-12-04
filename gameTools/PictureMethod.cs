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
using System.Reflection;
using TTTools.windowsTools;
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
        public List<Point> FindBitmapInScreenshot(Bitmap screenshot, Bitmap target)
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

            //SaveImage(toFind);
            List<Point> foundPoints = new List<Point>();

            var dataFullCapture = screenshot.LockBits(new Rectangle(0, 0, screenshot.Width, screenshot.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var dataToFind = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptrFullCapture = (byte*)dataFullCapture.Scan0;
                byte* ptrToFind = (byte*)dataToFind.Scan0;

                int strideFullCapture = dataFullCapture.Stride;
                int strideToFind = dataToFind.Stride;

                for (int y = 0; y < screenshot.Height - target.Height; y++)
                {
                    for (int x = 0; x < screenshot.Width - target.Width; x++)
                    {
                        bool found = true;
                        for (int y1 = 0; y1 < target.Height; y1++)
                        {
                            for (int x1 = 0; x1 < target.Width; x1++)
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

            target.UnlockBits(dataToFind);
            screenshot.UnlockBits(dataFullCapture);

            return foundPoints;
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
            // 加载背包特征图片
            using (Bitmap backpackBitmap = ResourceLoader.LoadBitmap("data.ui.zhuangbeidaoju.png"))
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
            // 加载背包特征图片
            using (Bitmap someThingImage = ResourceLoader.LoadBitmap($"data.ui.{name}.png"))
            {
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
            // 获取当前程序集
            var assembly = Assembly.GetExecutingAssembly();

            // 获取所有嵌入资源的名称
            var resourceNames = assembly.GetManifestResourceNames();

            // 定义资源前缀（命名空间.路径）
            string resourcePrefix = $"{assembly.GetName().Name}.data.mapName.";

            // 过滤出地图名称图片资源
            var mapNameResources = resourceNames.Where(name => name.StartsWith(resourcePrefix) && name.EndsWith(".png"));

            foreach (string resourceName in mapNameResources)
            {
                // 提取文件名（去掉路径和扩展名）作为键
                string fileName = Path.GetFileNameWithoutExtension(resourceName.Replace(resourcePrefix, ""));

                // 从嵌入资源加载图片
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"未找到嵌入的资源: {resourceName}");
                    }

                    using (Bitmap bmp = new Bitmap(stream))
                    {
                        // 克隆图片到内存，确保资源释放
                        Bitmap cachedBitmap = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(cachedBitmap))
                        {
                            g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        }

                        // 添加到字典中
                        mapNameBitmaps[fileName] = cachedBitmap;
                    }
                }
            }

            //LogService.Log($"地图名称图片加载完成，共加载了 {mapNameBitmaps.Count} 张图片。");
        }



        // 预处理截图，将非白色部分设置为透明
        public Bitmap PreprocessScreenshot(Bitmap screenshot)
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
                        Bitmap subRegion2 = processedScreenshot.Clone(
                            new Rectangle(x, y, digitBitmap.Width, digitBitmap.Height),
                            processedScreenshot.PixelFormat);
                        // 提取截图中的子区域
                        using (Bitmap subRegion = processedScreenshot.Clone(new Rectangle(x, y, digitBitmap.Width, digitBitmap.Height), processedScreenshot.PixelFormat))
                        {
                            SaveImage(subRegion2);
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
        public Point? FindXyInBitmap2(Bitmap screenshot)
        {
            List<(char, Point)> foundCoordinates = new List<(char, Point)>();

            // 遍历所有缓存的数字模板
            foreach (var kvp in digitBitmaps)
            {
                char digit = kvp.Key;
                Bitmap digitBitmap = kvp.Value;

                // 寻找数字在图片中的所有位置
                List<Point> digitPositions = FindAllOccurrences(screenshot, digitBitmap);

                // 记录找到的数字及其坐标
                foreach (var position in digitPositions)
                {
                    foundCoordinates.Add((digit, position));
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

        // 方法：寻找所有的匹配位置
        private List<Point> FindAllOccurrences(Bitmap screenshot, Bitmap template)
        {
            List<Point> positions = new List<Point>();

            var screenshotData = screenshot.LockBits(new Rectangle(0, 0, screenshot.Width, screenshot.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var templateData = template.LockBits(new Rectangle(0, 0, template.Width, template.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* screenshotPtr = (byte*)screenshotData.Scan0;
                    byte* templatePtr = (byte*)templateData.Scan0;

                    int screenshotStride = screenshotData.Stride;
                    int templateStride = templateData.Stride;

                    for (int y = 0; y <= screenshot.Height - template.Height; y++)
                    {
                        for (int x = 0; x <= screenshot.Width - template.Width; x++)
                        {
                            bool match = true;

                            for (int ty = 0; ty < template.Height; ty++)
                            {
                                for (int tx = 0; tx < template.Width; tx++)
                                {
                                    int screenshotIndex = (y + ty) * screenshotStride + (x + tx) * 4;
                                    int templateIndex = ty * templateStride + tx * 4;

                                    // 比较 ARGB 值
                                    if (*(int*)(screenshotPtr + screenshotIndex) != *(int*)(templatePtr + templateIndex))
                                    {
                                        match = false;
                                        break;
                                    }
                                }

                                if (!match)
                                    break;
                            }

                            if (match)
                            {
                                positions.Add(new Point(x, y)); // 记录匹配位置
                            }
                        }
                    }
                }
            }
            finally
            {
                screenshot.UnlockBits(screenshotData);
                template.UnlockBits(templateData);
            }

            return positions;
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
                    var find = FindBitmapInScreenshot( screenshot, mapBitmap);
                    if (find.Count>0)
                    {
                        return mapName;
                    }
                       
                }
            }

            // 如果没有匹配项，返回 null 或空字符串
            return null;
        }
        /**
         * 处理图片，把传入的图片中所有的颜色除参数1以外的颜色全部替换为参数2的颜色
         * 返回处理后的图片
         */
        public Bitmap ReplaceOtherColor(Bitmap sourceBitmap, string hexColorToKeep = "#FFFF00", string hexReplacementColor = "#3978AC")
        {
            // 将16进制颜色代码转换为Color对象
            Color colorToKeep = HexToColor(hexColorToKeep);
            Color replacementColor = HexToColor(hexReplacementColor);

            // 创建一个新的Bitmap作为输出
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);

            // 遍历原始图片的像素
            for (int y = 0; y < sourceBitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap.Width; x++)
                {
                    // 获取当前像素颜色
                    Color pixelColor = sourceBitmap.GetPixel(x, y);

                    // 如果像素颜色不是目标颜色，则替换为指定颜色
                    if (pixelColor.ToArgb() != colorToKeep.ToArgb())
                    {
                        resultBitmap.SetPixel(x, y, replacementColor);
                    }
                    else
                    {
                        // 保留目标颜色
                        resultBitmap.SetPixel(x, y, pixelColor);
                    }
                }
            }

            return resultBitmap;
        }
        public Bitmap ReplaceColor(Bitmap sourceBitmap, string hexColorToReplace, string hexReplacementColor)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));
            if (string.IsNullOrWhiteSpace(hexColorToReplace) || string.IsNullOrWhiteSpace(hexReplacementColor))
                throw new ArgumentNullException("颜色参数不能为空。");

            // 将16进制颜色代码转换为Color对象
            Color colorToReplace = HexToColor(hexColorToReplace);
            Color replacementColor = HexToColor(hexReplacementColor);

            // 创建一个新的Bitmap，避免直接修改源图片
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);

            // 遍历原图片的像素
            for (int y = 0; y < sourceBitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap.Width; x++)
                {
                    // 获取当前像素颜色
                    Color pixelColor = sourceBitmap.GetPixel(x, y);

                    // 如果颜色匹配，则替换；否则保持原样
                    if (pixelColor.ToArgb() == colorToReplace.ToArgb())
                    {
                        resultBitmap.SetPixel(x, y, replacementColor);
                    }
                    else
                    {
                        resultBitmap.SetPixel(x, y, pixelColor);
                    }
                }
            }

            return resultBitmap;
        }

        // 将16进制颜色代码转换为Color对象的方法
        private Color HexToColor(string hexColor)
        {
            // 确保颜色代码以#开头
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1); // 移除#号
            }

            if (hexColor.Length != 6 && hexColor.Length != 8)
                throw new ArgumentException("16进制颜色代码格式不正确，应为6位（RGB）或8位（ARGB）格式。");

            int alpha = 255; // 默认为完全不透明
            int startIndex = 0;

            if (hexColor.Length == 8) // 如果提供了8位颜色代码
            {
                alpha = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                startIndex = 2;
            }

            int red = Convert.ToInt32(hexColor.Substring(startIndex, 2), 16);
            int green = Convert.ToInt32(hexColor.Substring(startIndex + 2, 2), 16);
            int blue = Convert.ToInt32(hexColor.Substring(startIndex + 4, 2), 16);

            return Color.FromArgb(alpha, red, green, blue);
        }

        /**
         * 对比图片，在大图片中寻找小图片，如果找到了则返回小图片在大图片的像素位置，没找到返回null
         */
        public Point? FindImageInImage(Bitmap bigImage, Bitmap smallImage)
        {
            if (bigImage == null || smallImage == null)
                throw new ArgumentNullException("图片不能为空。");

            if (smallImage.Width > bigImage.Width || smallImage.Height > bigImage.Height)
                return null; // 小图片比大图片大，直接返回 null

            // 锁定大图片和小图片的像素数据
            var bigData = bigImage.LockBits(new Rectangle(0, 0, bigImage.Width, bigImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var smallData = smallImage.LockBits(new Rectangle(0, 0, smallImage.Width, smallImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                // 获取大图片和小图片的像素指针
                unsafe
                {
                    byte* bigPtr = (byte*)bigData.Scan0;
                    byte* smallPtr = (byte*)smallData.Scan0;

                    int bigStride = bigData.Stride;
                    int smallStride = smallData.Stride;

                    // 遍历大图片中的每个可能的起始点
                    for (int y = 0; y <= bigImage.Height - smallImage.Height; y++)
                    {
                        for (int x = 0; x <= bigImage.Width - smallImage.Width; x++)
                        {
                            bool match = true;

                            // 遍历小图片的每个像素
                            for (int sy = 0; sy < smallImage.Height; sy++)
                            {
                                for (int sx = 0; sx < smallImage.Width; sx++)
                                {
                                    int bigIndex = (y + sy) * bigStride + (x + sx) * 4;
                                    int smallIndex = sy * smallStride + sx * 4;

                                    // 比较 ARGB 值
                                    if (*(int*)(bigPtr + bigIndex) != *(int*)(smallPtr + smallIndex))
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if (!match)
                                    break;
                            }

                            if (match)
                            {
                                return new Point(x, y); // 找到匹配，返回起始坐标
                            }
                        }
                    }
                }
            }
            finally
            {
                // 解锁像素数据
                bigImage.UnlockBits(bigData);
                smallImage.UnlockBits(smallData);
            }

            return null; // 未找到匹配
        }
        /**
         * 填充透明图片背景为黑色
         */
        public Bitmap FillTransparentPixelsWithBlack(Bitmap sourceBitmap)
        {
            // 创建一个新的 Bitmap，用于保存结果
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Format32bppArgb);

            for (int y = 0; y < sourceBitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap.Width; x++)
                {
                    // 获取当前像素的颜色
                    Color pixelColor = sourceBitmap.GetPixel(x, y);

                    // 如果像素透明，则填充为黑色
                    if (pixelColor.A == 0)
                    {
                        resultBitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        // 否则保留原来的颜色
                        resultBitmap.SetPixel(x, y, pixelColor);
                    }
                }
            }

            return resultBitmap;
        }





    }
}
