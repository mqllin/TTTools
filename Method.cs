using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;


namespace TTTools
{
    class Method
    {
        private readonly IntPtr hWnd;
        private readonly WindowOperations wx;
        private readonly Form1 Instance;
        private PictureMethod Pic;
        private string popupX;
        private string popupY;
        private int rushTotal = 0;
        private bool isMoving = false; // 是否正在移动
        private bool isRunAction = false; //是否正在运行程序
        private System.Threading.Timer actionTimer; // 时钟
        IniFileHelper iniFileHelper = new IniFileHelper("settings.ini");
        private Dictionary<string, List<Bitmap>> featureImages = new Dictionary<string, List<Bitmap>>();

        public Method(IntPtr hWnd, Form1 Instance)
        {
            this.hWnd = hWnd;
            this.wx = new WindowOperations(hWnd, Instance);
            this.Instance = Instance;
            this.Pic = new PictureMethod(hWnd);
            LoadFeatureImages();

        }

        public void InitWindows()
        {
            wx.InitWindows();


        }
        public void pushMsg(string txt)
        {
            wx.SendChineseText(txt);
        }
        private void LoadFeatureImages()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dataDirectory = Path.Combine(currentDirectory, "data");

            // 定义需要加载的材料和对应的文件前缀
            Dictionary<string, string> materials = new Dictionary<string, string>
            {
                { "矿石", "kuangshi" },
                { "皮草", "caopi" },
                { "水晶", "shuijing" }
            };

            foreach (var material in materials)
            {
                featureImages[material.Key] = new List<Bitmap>();

                for (int i = 1; i <= 10; i++)  // 假设每种材料有两张图像
                {
                    string imagePath = Path.Combine(dataDirectory, $"{material.Value}{i}.png");

                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            Bitmap bitmap = new Bitmap(imagePath);
                            featureImages[material.Key].Add(bitmap);
                        }
                        catch (Exception ex)
                        {
                            Instance.AppendGlobalLog($"无法加载图像 {imagePath}: {ex.Message}");

                        }
                    }
                    else
                    {
                        //Instance.AppendGlobalLog($"图像文件不存在: {imagePath}");

                    }
                }
            }
        }
        public void ClickSelf()
        {
            var width = 806;
            var height = 629;
            var x = width / 2;
            var y = height / 2 - 50;
            wx.PushClick(x, y, false, false);
        }
        public void ClickBagBox()
        {
            wx.PushClick(446, 605);
        }

        public void UseBagItem(int index = 1)
        {
            if (index < 1 || index > 20)  // 检查index的有效性
            {
                Console.WriteLine("无效的物品索引");
                return;
            }
            ClickBagBox();
            Thread.Sleep(2000);
            // 计算行数和列数（从0开始）
            // 正确的行数和列数计算
            int row = (int)Math.Ceiling((double)index / 5.0);
            int col = index % 5 == 0 ? 5 : index % 5;
            // 转换为像素坐标
            int offsetX = 44 * col - 32;  // 44是格子宽度，-22是为了点击格子中心
            int offsetY = 44 * row - 22;  // 44是格子高度，-22是为了点击格子中心
            int rx = int.Parse(iniFileHelper.IniReadValue("box", "x", "436"));
            int ry = int.Parse(iniFileHelper.IniReadValue("box", "y", "325"));

            int x = rx + offsetX;
            int y = ry + offsetY;
            //Instance.AppendGlobalLog("col" + col + "," + row);

            //Instance.AppendGlobalLog("offset" + offsetX + ","+ offsetY);
            //Instance.AppendGlobalLog("click" + x+","+y);

            wx.PushClick(x, y, true);
            Thread.Sleep(2000);

            ClickBagBox();
        }

        public Point ClickPopupItemAuto(Point point)
        {
            int anchorX = int.Parse(iniFileHelper.IniReadValue("popup", "x", "186"));
            int anchorY = int.Parse(iniFileHelper.IniReadValue("popup", "y", "255"));

            Point? anchorPoint = FindSomeThingInMapByFileName("ui", "duihuakuang");
            if (anchorPoint != null)
            {
                anchorX = anchorPoint.Value.X;
                anchorY = anchorPoint.Value.Y;
            }

            Point clickPoint = new Point(point.X + anchorX, point.Y + anchorY);
            wx.PushClick(clickPoint.X, clickPoint.Y, false, false);
            return clickPoint;
        }
        public Point ClosePopupAuto()
        {
            int anchorX = 0;
            int anchorY = 0;
            Point? anchorPoint = FindSomeThingInMapByFileName("ui", "close");
            if (anchorPoint != null)
            {
                anchorX = anchorPoint.Value.X;
                anchorY = anchorPoint.Value.Y;
            }

            Point clickPoint = new Point(anchorX, anchorY);
            wx.PushClick(clickPoint.X, clickPoint.Y, false, false);
            return clickPoint;
        }
        public void ClickPopupItem(int x, int y)
        {
            int rx = int.Parse(iniFileHelper.IniReadValue("popup", "x", "186"));
            int ry = int.Parse(iniFileHelper.IniReadValue("popup", "y", "255"));
            int ix = rx + x;
            int iy = ry + y;
            wx.PushClick(ix, iy);
        }


        public void ReadPopupContent()
        {
            int width = 463;
            int height = 116;
        }
        public void Teleport(int type = 0)
        {

            //WindowOperations.PushClick(hWnd, x, y);
            int x = 60;
            int y = 350;
            // 星秀村
            if (type == 0)
            {
                UseBagItem(1);
            }
            //应天府
            if (type == 1)
            {
                UseBagItem(2);
            }
            // 汴京城
            if (type == 3)
            {
                UseBagItem(3);
            }

        }
        public Point? GetMapExportPoin(string currentName, string nextName)
        {
            if (currentName == "应天府")
            {
                if (nextName == "应天府东")
                {
                    return new Point(484, 297);
                }
                if (nextName == "应天府西")
                {
                    return new Point(5, 10);
                }
            }
            if (currentName == "应天府东")
            {
                if (nextName == "应天府")
                {
                    return new Point(3, 27);
                }
                if (nextName == "应天府东郊")
                {
                    return new Point(74, 29);
                }
            }
            if (currentName == "应天府西")
            {
                if (nextName == "应天府")
                {
                    return new Point(74, 35);
                }
                if (nextName == "应天府西郊")
                {
                    return new Point(2, 35);
                }
            }
            return null;
        }
        public void GetNpcTaskByTs()
        {
            Teleport(1);
            Thread.Sleep(2000);
            wx.MouseHover(330, 180);
            Thread.Sleep(2000);
            wx.PushClick(330, 180);
            Thread.Sleep(3000);
            ClickPopupItem(55, 48);
        }

        public void GetTS()
        {
            int x = 194;
            int y = 282;
            int width = 430;
            int height = 105;
            CaptureAndOCR ocr = new CaptureAndOCR(hWnd, x, y, width, height);
            MapPoint point = ocr.CaptureAndRecognize();
            Instance.AppendGlobalLog("天师说：" + point.ToString());
        }

        public void RushB(int index, int x, int y)
        {

            if (index == 1 || index % 5 == 0)
            {
                Thread.Sleep(2000);
                UseBagItem(4);
            }


            Thread.Sleep(2000);
            wx.PushClick(x, y);
            Thread.Sleep(2000);
            ClickPopupItem(64, 36);
            //Instance.AppendGlobalLog("第：" + RushTotal.ToString()+"次开始战斗");

        }
        public void EndCombatByMsg()
        {

            Thread.Sleep(1000);
            wx.PushClick(64, 620);
            Thread.Sleep(1000);
            wx.PushCopy();
            Thread.Sleep(1000);
            wx.SendEnterKey();
        }
        public void EndCombat2()
        {

            // 创建一个TcpClientWrapper对象并连接到服务器
            TcpClientWrapper tcpClient = new TcpClientWrapper("121.41.112.13", 9874);

            // 发送数据
            tcpClient.SendData("1D 00 80 CB 92 CD 13 89 82 A4 C6 B5 B5 C0 A4 B5 B1 C7 B0 A4 C4 DA C8 DD A8 CD CB B3 F6 D5 BD B6 B7");

            // 接收数据
            string receivedData = tcpClient.ReceiveData();
            Console.WriteLine("Received: " + receivedData);
            Instance.AppendGlobalLog("receivedData" + receivedData);
            // 关闭连接
            tcpClient.Close();

        }

        private static byte[] HexStringToBytes(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public void EndCombat()
        {
            try
            {
                // 将十六进制字符串转换为字节数组
                string hexString = "1D 00 80 CB 92 CD 13 89 82 A4 C6 B5 B5 C0 A4 B5 B1 C7 B0 A4 C4 DA C8 DD A8 CD CB B3 F6 D5 BD B6 B7";
                byte[] data = new byte[]
                {
                    0x1D, 0x00, 0x80, 0xCB, 0x92, 0xCD, 0x13, 0x89,
                    0x82, 0xA4, 0xC6, 0xB5, 0xB5, 0xC0, 0xA4, 0xB5,
                    0xB1, 0xC7, 0xB0, 0xA4, 0xC4, 0xDA, 0xC8, 0xDD,
                    0xA8, 0xCD, 0xCB, 0xB3, 0xF6, 0xD5, 0xBD, 0xB6, 0xB7
                };

                TcpClient client = new TcpClient();

                // 绑定到本地端口2584
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 2584);
                client.Client.Bind(localEndPoint);

                // 连接到远程服务器
                client.Connect("121.41.112.13", 9874);


                // 获取用于数据传输的流对象
                NetworkStream stream = client.GetStream();

                // 发送数据
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Data sent.");
                Instance.AppendGlobalLog("结束战斗");
                // 关闭连接
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
                Instance.AppendGlobalLog("结束战斗：" + e.Message);

            }
        }
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid length of hex string.");
            }

            byte[] byteArray = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string byteValue = hexString.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(byteValue, 16);
            }

            return byteArray;
        }

        public Point? FindAndClickSomeThingInMap(string name, bool isRightClick = false)
        {
            int offsetX = 0, offsetY = 0;
            if (name == "矿石")
            {
                offsetX = 0;
                offsetY = 30;
            }
            if (name == "水晶")
            {
                offsetX = 0;
                offsetY = 30;
            }
            if (name == "皮草")
            {
                offsetX = 0;
                offsetY = 30;
            }
            if (featureImages.TryGetValue(name, out var targetImages))
            {
                PictureMethod pic = new PictureMethod(hWnd);

                foreach (var targetImage in targetImages)  // 遍历每张特征图片
                {
                    List<Point> locations = pic.FindBitmapInWindow(targetImage);
                    if (locations.Count > 0)
                    {
                        Point screenCenter = new Point(806 / 2, 692 / 2);
                        Point closestPoint = locations[0];
                        double minDistance = Distance(screenCenter, closestPoint);

                        foreach (Point point in locations)
                        {
                            double distance = Distance(screenCenter, point);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestPoint = point;
                            }
                        }

                        //Instance.AppendGlobalLog($"screenCenter：{screenCenter.X}, {screenCenter.Y}");
                        Instance.AppendGlobalLog($"找到目标：{minDistance}  - {closestPoint.X}, {closestPoint.Y}");

                        wx.PushClick(closestPoint.X + offsetX, closestPoint.Y + offsetY, isRightClick);
                        wx.MoveMouse(747, 114);

                        return closestPoint;
                    }


                }

                // 所有图片都没有找到，进行其他处理
                return null;
            }
            else
            {
                // 没有找到对应的特征图片，进行其他处理
                return null;
            }
        }

        public Point? FindSomeThingInMapByFileName(string type, string fileName)
        {
            Bitmap targetImage = null;
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string imagePath = Path.Combine(currentDirectory, $"data/{type}", $"{fileName}.png");
            targetImage = new Bitmap(imagePath);

            if (targetImage != null)
            {
                PictureMethod pic = new PictureMethod(hWnd);

                List<Point> locations = pic.FindBitmapInWindow(targetImage);
                if (locations.Count > 0)
                {
                    Point screenCenter = new Point(806 / 2, 692 / 2);
                    Point closestPoint = locations[0];
                    double minDistance = Distance(screenCenter, closestPoint);

                    foreach (Point point in locations)
                    {
                        double distance = Distance(screenCenter, point);
                        Instance.AppendGlobalLog($"distance：" + distance + "x=" + point.X + ",y=" + point.Y);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestPoint = point;
                        }
                    }
                    // 最近的点是closestPoint
                    minDistance = Distance(screenCenter, closestPoint);
                    Instance.AppendGlobalLog($"screenCenter：{screenCenter.X}, {screenCenter.Y}");
                    Instance.AppendGlobalLog($"找到目标：{minDistance}  - {closestPoint.X}, {closestPoint.Y}");
                    return closestPoint;
                }
                else
                {
                    // 没有找到图片，进行其他处理
                    Instance.AppendGlobalLog("没有找到");
                    return null;
                }
            }
            else
            {
                // targetImage为null，输出错误或进行其他处理
                return null;

            }
        }
        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        ////private int nullCounter = 0;  // 用于跟踪连续返回 null 的次数
        //private int lastPointIndex = 0;
        //public Point? HandleCollection(string name)
        //{

        //    var point = FindSomeThingInMap( name);
        //    if (point != null)
        //    {
        //        int x = point.Value.X;
        //        int y = point.Value.Y;
        //        wx.PushClick(x, y, true, false);
        //        wx.MoveMouse(x + 300, y - 300, false);
        //        Thread.Sleep(100);
        //        HandleCollection(name);
        //    }
        //    else
        //    {
        //        nullCounter++;  // 递增计数器
        //        if (nullCounter >= 3)
        //        {
        //            MoveCollectionLocation(name);  // 调用 move 方法
        //            nullCounter = 0;  // 重置计数器
        //        }
        //    }
        //    return point;


        ////}
        //public void HandleCollectionTimer(string name)
        //{
        //    //UseBagItem(4);
        //    lastPointIndex = 0;
        //    var point = FindSomeThingInMap( name);
        //    if (point != null)
        //    {
        //        nullCounter = 0;  // 重置计数器
        //        int x = point.Value.X;
        //        int y = point.Value.Y;
        //        wx.PushClick(x, y, true, false);
        //        wx.MoveMouse(x + 300, y - 300, false);
        //        Thread.Sleep(100);
        //    }
        //    else
        //    {
        //        nullCounter++;  // 递增计数器
        //        if (nullCounter >= 3)
        //        {
        //            MoveCollectionLocation(name);  // 调用 move 方法
        //            nullCounter = 0;  // 重置计数器
        //        }
        //    }
        //    Pic.CheckIsMoving()

        //}

        public Point ClickTabMapPoint(Point point)
        {
            wx.SendTabKey();
            Thread.Sleep(1000);
            var mapAnchorPoint = FindSomeThingInMapByFileName("ui", "close");
            var offsetX = 231;
            var offsetY = 180;

            if (mapAnchorPoint != null)
            {
                offsetX = mapAnchorPoint.Value.X;
                offsetY = mapAnchorPoint.Value.Y;
            }
            Point clickPoint = new Point(point.X + offsetX, point.Y + offsetY + 26);

            wx.PushClick(clickPoint.X, clickPoint.Y);
            Thread.Sleep(1000);
            wx.SendTabKey();
            Thread.Sleep(1000);
            return clickPoint;

        }
        public void ClickMapPoint(Point point)
        {
            wx.SendTabKey();
            Thread.Sleep(1000);
            wx.PushClick(point.X, point.Y);
            Thread.Sleep(1000);
            wx.SendTabKey();
            Thread.Sleep(1000);

        }
        // 更换采集地点
        public void MoveCollectionLocation(string name, int index)
        {
            // 初始化用于存放坐标的变量
            List<Point> points = new List<Point>();
            int mapX = int.Parse(iniFileHelper.IniReadValue("map", "x", "222"));
            int mapY = int.Parse(iniFileHelper.IniReadValue("map", "y", "151"));

            // 判断name是否为“矿石”
            if (name == "矿石")
            {

                int offsetX = -7;
                int offsetY = 34;
                // 为points加入坐标
                points = CalculateGridIntersections(name);
                foreach (Point point in points)
                {
                    string text = $"地图坐标点：({point.X}, {point.Y})";
                    Instance.AppendGlobalLog(text);
                }
                if (points.Count > 0)
                {
                    var point = points[index];

                    int x = mapX + offsetX + point.X;
                    int y = mapY + offsetY + point.Y;
                    ClickMapPoint(new Point(x, y));
                    Instance.AppendGlobalLog($"点击坐标点=：({mapX}+{offsetX}+{point.X}={x})");

                }


            }
        }

        // 计算并返回所有30*30网格的交叉点坐标
        public List<Point> CalculateGridIntersections(string name)
        {
            int mapWidth = 0;
            int mapHeight = 0;
            if (name == "矿石")
            {
                mapWidth = 236;
                mapHeight = 144;

            }
            // 初始化用于存放坐标的List
            List<Point> points = new List<Point>();

            // 网格大小
            int gridSize = 30;

            // 计算交叉点数量并添加到列表中
            for (int y = 0; y <= mapHeight - gridSize; y += gridSize)
            {
                for (int x = 0; x <= mapWidth - gridSize; x += gridSize)
                {
                    points.Add(new Point(x + gridSize, y + gridSize));
                }
            }

            return points;
        }
        public bool AwaitMoving()
        {
            PictureMethod pic = new PictureMethod(hWnd);
            while (pic.CheckIsMoving())
            {
                // 你可能需要在这里加上一点延迟，以防止CPU使用率过高
                Thread.Sleep(500);
            }
            return true;
        }
        public void SwitchToEnglishInput()
        {
            // 按下 Alt 键（虚拟键码 0xA4）
            wx.SendKey(0xA4);
            // 按下 Shift 键
            wx.SendKey(0x10);

         
        }
        public void loginAuto(int index,string username,string password)
        {
            WindowUtilities.ChangeInputEn();
            //SwitchToEnglishInput();
            wx.PushClick(574,132);
            wx.PushClick(543,445);
            wx.PushClick(612, 454);
            // 删除账号
            Thread.Sleep(100);

            wx.PushClick(459, 265);
            for (var i = 0; i < 30; i++) {
                wx.SendKey(0x08);
            }
            wx.SendString(username);

            wx.PushClick(459, 290);
            for (var i = 0; i < 30; i++)
            {
                wx.SendKey(0x08);
            }
            wx.SendString(password);

            wx.PushClick(470, 352);
            int userX = 286 + (index * 62);
            int userY = 370;
            wx.PushClick(userX, userY);
            wx.PushClick(userX, userY);
            Instance.AppendGlobalLog($"角色{index},登录完成");
        }
    }
}
