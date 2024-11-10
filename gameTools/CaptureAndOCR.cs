using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Tesseract;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
namespace TTTools
{
    class CaptureAndOCR
    {
        public IntPtr hWnd;
        public int x;
        public int y;
        public int width;
        public int height;


        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public CaptureAndOCR(IntPtr hWnd, int x, int y, int width, int height)
        {
            this.hWnd = hWnd;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public static Bitmap CaptureWindow(IntPtr hWnd, int x, int y, int width, int height)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            // 截图
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left + x, rect.Top + y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            
            return ProcessImage(bmp);
        }

        public static Bitmap ProcessImage(Bitmap originalImage)
        {
            // 创建一个新的Bitmap对象作为输出
            Bitmap processedImage = new Bitmap(originalImage.Width, originalImage.Height);

            // 定义需要查找和替换的颜色
            Color targetBgColor = Color.FromArgb(255, 41, 84, 139);  // #29548B
            Color newBgColor = Color.White;  // #FFFFFF
            Color targetTextColor = Color.White;  // #FFFFFF
            Color newTextColor = Color.Black;  // #000000

            // 首先更改文字颜色
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    // 获取当前像素的颜色
                    Color pixelColor = originalImage.GetPixel(x, y);

                    // 检查像素颜色是否是目标文字颜色
                    if (pixelColor == targetTextColor)
                    {
                        processedImage.SetPixel(x, y, newTextColor);
                    }
                    else
                    {
                        // 如果不需要更改，复制原像素颜色
                        processedImage.SetPixel(x, y, pixelColor);
                    }
                }
            }

           

            return processedImage;
        }
        public static string OCR(Bitmap image)
        {
            string resultText = "";
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

            // 将Bitmap对象保存为临时文件
            image.Save(tempFilePath);
            //C:\Users\A\Documents\Project\windows\TTTools\data\
            //@"./data", 
            using (var engine = new TesseractEngine("C:/Users/A/Documents/Project/windows/TTTools/data", "chi_sim", EngineMode.Default))
            {
                using (var pix = Pix.LoadFromFile(tempFilePath))
                {
                    using (var page = engine.Process(pix))
                    {
                        resultText = page.GetText();
                    }
                }
            }
            // 删除临时文件（可选）
            //File.Delete(tempFilePath);

            return resultText;
        }

        public static string saveImage(Bitmap image)
        {
            string resultText = "";
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
        
        public static string BaiduOcr(Bitmap image)
        {
            // 设置APPID/AK/SK
           
            var API_KEY = "Xmjihm9QSBAvb87NSQpWYsGq";
            var SECRET_KEY = "raGhg4flmGyt79fqnHMQbt2RQ03ZGGMM";

            var client = new Baidu.Aip.Ocr.Ocr(API_KEY, SECRET_KEY);
            client.Timeout = 60000;  // 修改超时时间
            var tempFilePath = saveImage(image);
            var imageBty = File.ReadAllBytes(tempFilePath);
            // 调用通用文字识别（高精度版），可能会抛出网络等异常，请使用try/catch捕获
            var result = client.AccurateBasic(imageBty);
            Console.WriteLine(result);
            // 如果有可选参数
            var options = new Dictionary<string, object>{
        {"detect_direction", "true"},
        {"probability", "true"}
    };
            // 带参数调用通用文字识别（高精度版）
            result = client.AccurateBasic(imageBty, options);
            Console.WriteLine(result);
            //return result.ToString();

            // 解析 JSON 结果
            JObject parsedResult = JObject.Parse(result.ToString());

            // 初始化一个 StringBuilder 用于拼接字符串
            StringBuilder sb = new StringBuilder();

            // 遍历 'words_result' 数组
            foreach (var item in parsedResult["words_result"])
            {
                // 从每个元素中获取 'words' 字段，并加入 StringBuilder
                sb.Append(item["words"].ToString());
                sb.Append(" ");  // 在单词之间添加空格（可选）
            }

            // 输出或返回拼接完成的字符串
            string completeSentence = sb.ToString().Trim(); // 移除末尾可能多出来的空格
            Console.WriteLine(completeSentence);

            return completeSentence;
        }
        public static MapPoint ExtractLocationInfo(string input)
        {
            MapPoint locationInfo = null;

            // 定义正则表达式，用于查找地点和坐标
            Regex regex = new Regex(@"在(.*?)\((\d+,\d+)\)");

            // 执行匹配
            Match match = regex.Match(input);

            if (match.Success)
            {
                // 提取地点和坐标
                var name = match.Groups[1].Value.Replace("在", ""); ;
                string coordinates = match.Groups[2].Value;

                // 分割坐标为 x 和 y
                string[] xy = coordinates.Split(',');
               
                locationInfo = new MapPoint(name, int.Parse(xy[0]), int.Parse(xy[1]));

            }

            return locationInfo;
        }

    
       

        public MapPoint CaptureAndRecognize()
        {
            // 截图
            Bitmap capturedImage = CaptureWindow(hWnd, x, y, width, height);
            //saveImage(capturedImage);

            // OCR
            string recognizedText = BaiduOcr(capturedImage);
            return  ExtractLocationInfo(recognizedText);
        }
        
    }
}
