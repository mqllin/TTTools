using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TTTools.client;
using TTTools.gameTools;
using TTTools.windowsTools;

namespace TTTools
{
    class Method : BaseOperation
    {
        private readonly IntPtr hWnd;
        private readonly WindowClickTools win;
        private PictureMethod pic;


        IniFileHelper iniFileHelper = new IniFileHelper("settings.ini");
        private Dictionary<string, List<Bitmap>> featureImages = new Dictionary<string, List<Bitmap>>();

        public Method(IntPtr hWnd) : base(hWnd)
        {
            this.hWnd = hWnd;
            this.win = new WindowClickTools(hWnd);
            this.pic = new PictureMethod(hWnd);
            LoadFeatureImages();
        }

        public void InitWindows()
        {
            win.InitWindows();
        }

        private void LoadFeatureImages()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;


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

                for (int i = 1; i <= 10; i++) // 假设每种材料有两张图像
                {
                    try
                    {
                        Bitmap bitmap = ResourceLoader.LoadBitmap($"data.caiji.{material.Value}{i}.png");
                        featureImages[material.Key].Add(bitmap);
                    }
                    catch (Exception)
                    {
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
            win.MoveClick(x, y, false, false);
        }

        //点击背包
        public void OpenBackpack()
        {
            win.MoveClick(446, 605);
        }

        public void CloseBackpack()
        {
            win.MoveClick(446, 605);
        }

        public void ClickBackpack()
        {
            win.MoveClick(446, 605);
        }

        //打开背包
        public void OpenMap()
        {
            win.MoveMouse(0, 0);
            Point? point = FindSomeThingInMapByFileName("ui", "mapPoint");
            if (point == null)
            {
                win.SendTabKey();
            }
        }

        //打开任务栏
        public void OpenTask()
        {
            win.MoveMouse(0, 0);
            Point? point = FindSomeThingInMapByFileName("ui", "task");
            if (point == null)
            {
                win.MoveClick(605, 605);
            }
        }
        // 打开背包
        public void OpenBackPack()
        {
            if (!IsBackpackOpen())
            {
                ClickBackpack();
            }
        }
        // 关闭背包
        public void CloseBackPack()
        {
            if (IsBackpackOpen())
            {
                ClickBackpack();
                LogService.Log("关闭背包");
            }
        }
        //关闭任务栏
        public void CloseTask()
        {
            win.MoveClick(605, 605);
        }
        //是否领取了打宝图任务
        public bool IsHasWaBaoTask(bool autoRmove = false)
        {
            OpenTask();
            win.MoveMouse(0, 0);
            Point? point = FindSomeThingInMapByFileName("ui", "宝图任务");
            if (point == null)
            {
                LogService.Log("检查是否已经领取了打宝图任务：没有");
                CloseTask();
                return false;
            }
            if (autoRmove)
            {
                point = FindSomeThingInMapByFileName("ui", "放弃任务");
                if (point != null)
                {
                    win.MoveClick(point.Value.X, point.Value.Y);
                    LogService.Log("放弃了之前的挖宝任务");
                    CloseTask();
                    return false;
                }
                CloseTask();
                return true;
            }
            LogService.Log("检查是否已经领取了打宝图任务：有");
            CloseTask();
            return true;
        }

        //是否使用了驱魔香
        public bool IsUseQuMoXiang(bool use = false)
        {
            win.MoveMouse(0, 0);
            Point? point = FindSomeThingInMapByFileName("ui", "驱魔香状态");
            if (point == null)
            {
                LogService.Log("检查是否使用驱魔香：没有使用");
                if (use)
                {
                    OpenBackpack();
                    UseBagItem("驱魔香");
                    return true;
                }
                return false;
            }
            LogService.Log("检查是否使用驱魔香：已使用");
            return true;
        }
        /**
         * 购买驱魔香
         */
        public async Task BuySomeThing(string goodName, int buyNum = 1)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            win.MoveMouse(800, 0);
            await MoveToMapAsync("应天府");
            Thread.Sleep(2000);
            MapGotoClick("应天府", 123, 140);
            win.MoveClick(570, 297);
            Thread.Sleep(500);
            Point? p = FindSomeThingInMapByFileName("ui", "杂货店对话1");
            if (p == null)
            {
                LogService.Log("没有找到杂货店对话1");
                return;
            }
            win.MoveClick(p.Value.X, p.Value.Y, false, false);
            Thread.Sleep(500);
            Point? goods = FindSomeThingInMapByFileName("ui", goodName);
            if (goods == null)
            {
                LogService.Log("没有找到要买的物品");
                ClosePopupAuto();
                return;
            }
            else
            {
                win.MoveClick(goods.Value.X, goods.Value.Y, false, false);
                Point? buyBtn = FindSomeThingInMapByFileName("ui", "购买");
                if (buyBtn != null)
                {
                    for (int i = 0; i < buyNum; i++)
                    {
                        win.Click(buyBtn.Value.X, buyBtn.Value.Y, false, false);
                    }
                    ClosePopupAuto();
                    LogService.Log($"购买{goodName}x{buyNum}完成");
                    return;
                }
            }

        }
        public string GetCurrentMapName()
        {
            var name = pic.GetCurrentMapName();
            LogService.Log($"当前在：{name}");
            return name;
        }

        public async Task<bool> MoveToMapAsync(string targetMapName)
        {
            // 可以直接飞行的地图名单
            var flyableMaps = new HashSet<string> { "星秀村", "应天府", "汴京城", "清河县", "阳谷县" };
            // 获取当前地图名称
            var currentMapName = GetCurrentMapName();

            // 如果已经在目标地图，则直接返回成功
            if (currentMapName == targetMapName)
            {
                LogService.Log($"已经在目标地图: {targetMapName}");
                return true;
            }

            // 如果目标地图在飞行传送名单中，直接调用飞行方法
            if (flyableMaps.Contains(targetMapName))
            {
                if (MoveToMapByFly(targetMapName))
                {
                    LogService.Log($"通过飞行成功到达目标地图: {targetMapName}");
                    return true;
                }
                else
                {
                    LogService.Log($"通过飞行传送失败: {targetMapName}");
                    return false;
                }
            }
            // 获取完整路径
            List<string> route;
            try
            {
                route = MapPoint.GetRouteToMap(targetMapName);
            }
            catch (Exception ex)
            {
                LogService.Log($"获取目标地图路径失败: {ex.Message}");
                return false;
            }
            // 如果当前地图已经在路径中，跳过前面的地图
            int currentIndex = route.IndexOf(currentMapName);
            if (currentIndex == -1)
            {
                LogService.Log($"当前地图 {currentMapName} 不在到 {targetMapName} 的路径中，无法导航！");
                return false;
            }

            // 遍历路径，从当前地图的下一个地图开始
            bool isFirstStep = currentIndex == 0; // 仅当当前地图是路径起点时执行飞行传送
            for (int i = currentIndex + 1; i < route.Count; i++)
            {
                string step = route[i];
                LogService.Log($"导航至地图: {step}");

                if (isFirstStep)
                {
                    // 第一步使用飞行传送
                    if (currentMapName != step)
                    {
                        if (!MoveToMapByFly(step))
                        {
                            LogService.Log($"传送到起点地图 {step} 失败！");
                            return false;
                        }
                    }

                    // 更新当前地图
                    currentMapName = GetCurrentMapName();
                    if (currentMapName != step)
                    {
                        LogService.Log($"传送后预期在 {step}，但实际在 {currentMapName}！");
                        return false;
                    }

                    isFirstStep = false; // 标记飞行传送完成
                    continue; // 跳过后续逻辑，进入下一个循环
                }

                // 后续地图传送逻辑
                if (!MapPoint.mapOutPoint.ContainsKey(currentMapName))
                {
                    LogService.Log($"当前地图 {currentMapName} 无传送配置！");
                    return false;
                }

                // 获取当前地图的传送节点
                var outPoints = MapPoint.mapOutPoint[currentMapName];
                var nextRoute = outPoints.FirstOrDefault(r => r.NextMap == step);
                if (nextRoute == null)
                {
                    LogService.Log($"在地图 {currentMapName} 无法找到到达 {step} 的传送点！");
                    return false;
                }

                // 点击传送点
                LogService.Log($"点击传送点前往 {step}, 坐标: ({nextRoute.ExitPoint.X}, {nextRoute.ExitPoint.Y})");
                MapGotoClick(currentMapName, nextRoute.ExitPoint.X, nextRoute.ExitPoint.Y);

                // 等待移动完成
                if (!pic.WaitForMovementToStop())
                {
                    LogService.Log($"移动到 {step} 的过程失败！");
                    return false;
                }

                // 等待额外的 5 秒
                await Task.Delay(6000);

                // 更新当前地图
                currentMapName = GetCurrentMapName();
                if (currentMapName != step)
                {
                    LogService.Log($"期望在 {step}，但实际在 {currentMapName}！");
                    return false;
                }
            }

            LogService.Log($"成功到达目标地图: {targetMapName}");
            return true;
        }
        public bool MapGotoClick(string mapName, int x, int y)
        {
            var config = MapPoint.GetMapConfig(mapName);
            var offset = config.Offset;
            if (!IsMapOpen())
            {
                OpenMap();
            }

            var closestCoordinate = ToolsFunction.FindClosestCoordinate(mapName, x, y);
            if (closestCoordinate != null)
            {
                Point? point = FindSomeThingInMapByFileName("ui", "mapPoint");
                if (point == null)
                {
                    LogService.Log("地图起点未找到");
                    return false;
                }

                // 锚点起始位置
                int startX = point.Value.X + offset.X;
                int startY = point.Value.Y + offset.Y;

                win.MoveClick(startX + closestCoordinate.Value.DeltaX + config.MapClickOffset.X,
                    startY + closestCoordinate.Value.DeltaY + config.MapClickOffset.Y);
                // LogService.Log($"最接近的行增量: {closestCoordinate.Value.DeltaX}, 列增量: {closestCoordinate.Value.DeltaY}");
                LogService.Log($"游戏坐标: ({closestCoordinate.Value.GameX}, {closestCoordinate.Value.GameY})");
                CloseMap();
                return true;
            }
            else
            {
                LogService.Log("没有找到接近的坐标！");
                return false;
            }
        }



        // 移动到地图
        public bool MoveToMapByFly(string name)
        {
            var currentMapName = GetCurrentMapName();
            if (currentMapName == name)
            {
                return true;
            }

            // 打开背包
            OpenBackPack();
            var cityMap = new Dictionary<string, string>
            {
                { "星秀村", "xingXiuCun" },
                { "应天府", "yingTianFu" },
                { "汴京城", "bianJingCheng" },
                { "清河县", "qingHeXian" },
                { "阳谷县", "yangGuXian" }
            };
            // 获取 name 对应的 code
            if (!cityMap.TryGetValue(name, out var targetCode))
            {
                return false;
            }

            UseBagItem("回程符");
            win.MoveMouse(800, 0);
            Point? point = FindSomeThingInMapByFileName("ui", targetCode);
            if (point == null)
            {
                return false;
            }

            win.MoveClick(point.Value.X + 15, point.Value.Y + 35);
            Thread.Sleep(500);
            //CloseBackPack();
            ClosePopupAuto();
            return true;
        }

        public void CloseMap()
        {
            win.MoveMouse(0, 0);
            Point? point = FindSomeThingInMapByFileName("ui", "mapPoint");
            if (point != null)
            {
                win.SendTabKey();
            }
        }

        //背包是否打开
        public bool IsMapOpen()
        {
            var point = pic.IsMapOpen();
            if (point != null)
            {
                LogService.Log($"地图已经打开了{point.ToString()}");
                return true;
            }

            return false;
        }

        //背包是否打开
        public bool IsBackpackOpen()
        {
            var point = pic.IsBackpackOpen();
            if (point != null)
            {
                LogService.Log($"背包已打开{point.ToString()}");
                return true;
            }

            return false;
        }

        /**
         * name 回程符 驱魔香
         **/
        public bool UseBagItem(string name)
        {
            if (!IsBackpackOpen())
            {
                OpenBackpack();
            }
            string nameCode = "";
            switch (name)
            {
                case "回程符": nameCode = "huiChengFu"; break;
                case "驱魔香": nameCode = "quMoXiang"; break;
            }

            Point? p = pic.FindSomeInBackpack(nameCode);
            if (p != null)
            {
                win.MoveClick(p.Value.X, p.Value.Y, true);
                LogService.Log($"使用道具{name}");
                CloseBackpack();

                return true;
            }
            else
            {
                return false;
            }
        }

        //使用背包物品-索引
        public void UseBagItem(int index = 1)
        {
            if (index < 1 || index > 20) // 检查index的有效性
            {
                Console.WriteLine("无效的物品索引");
                return;
            }

            ClickBackpack();
            Thread.Sleep(2000);
            // 计算行数和列数（从0开始）
            // 正确的行数和列数计算
            int row = (int)Math.Ceiling((double)index / 5.0);
            int col = index % 5 == 0 ? 5 : index % 5;
            // 转换为像素坐标
            int offsetX = 44 * col - 32; // 44是格子宽度，-22是为了点击格子中心
            int offsetY = 44 * row - 22; // 44是格子高度，-22是为了点击格子中心
            int rx = int.Parse(iniFileHelper.IniReadValue("box", "x", "436"));
            int ry = int.Parse(iniFileHelper.IniReadValue("box", "y", "325"));

            int x = rx + offsetX;
            int y = ry + offsetY;


            win.MoveClick(x, y, true);
            Thread.Sleep(2000);

            ClickBackpack();
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
            win.MoveClick(clickPoint.X, clickPoint.Y, false, false);
            return clickPoint;
        }
        public Bitmap? GetPopupImage()
        {
            Point? point1 = FindSomeThingInMapByFileName("ui", "duihuakuang");
            if (point1 != null)
            {
                return pic.CaptureWindow(point1.Value.X, point1.Value.Y, 439, 100);
            }

            return null;
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
            win.MoveClick(clickPoint.X, clickPoint.Y, false, false);
            return clickPoint;
        }

        public void ClickPopupItem(int x, int y)
        {
            int rx = int.Parse(iniFileHelper.IniReadValue("popup", "x", "186"));
            int ry = int.Parse(iniFileHelper.IniReadValue("popup", "y", "255"));
            int ix = rx + x;
            int iy = ry + y;
            win.MoveClick(ix, iy);
        }


       

      

        public Point? GetMapExportPoint(string currentName, string nextName)
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

     

        public void RushB(int index, int x, int y)
        {
            if (index == 1 || index % 5 == 0)
            {
                Thread.Sleep(2000);
                UseBagItem(4);
            }


            Thread.Sleep(2000);
            win.MoveClick(x, y);
            Thread.Sleep(2000);
            ClickPopupItem(64, 36);
        }

        public void EndCombatByMsg()
        {
            Thread.Sleep(1000);
            win.MoveClick(64, 620);
            Thread.Sleep(1000);
            win.PushCopy();
            Thread.Sleep(1000);
            win.SendEnterKey();
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

                foreach (var targetImage in targetImages) // 遍历每张特征图片
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

                        LogService.Log($"找到目标：{minDistance}  - {closestPoint.X}, {closestPoint.Y}");

                        win.MoveClick(closestPoint.X + offsetX, closestPoint.Y + offsetY, isRightClick);
                        win.MoveMouse(747, 114);

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

        // 在窗口中寻找素材目标
        public Point? FindSomeThingInMapByFileName(string type, string fileName)
        {
            Thread.Sleep(500);
            Bitmap targetImage = null;
            targetImage = ResourceLoader.LoadBitmap($"data.{type}.{fileName}.png");

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
                        LogService.Log($"distance：" + distance + "x=" + point.X + ",y=" + point.Y);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestPoint = point;
                        }
                    }

                    // 最近的点是closestPoint
                    minDistance = Distance(screenCenter, closestPoint);
                    LogService.Log($"screenCenter：{screenCenter.X}, {screenCenter.Y}");
                    LogService.Log($"找到目标：{minDistance}  - {closestPoint.X}, {closestPoint.Y}");
                    return closestPoint;
                }
                else
                {
                    // 没有找到图片，进行其他处理
                    LogService.Log("没有找到");
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
            win.SendTabKey();
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

            win.MoveClick(clickPoint.X, clickPoint.Y);
            Thread.Sleep(1000);
            win.SendTabKey();
            Thread.Sleep(1000);
            return clickPoint;
        }

        public void ClickMapPoint(Point point)
        {
            win.SendTabKey();
            Thread.Sleep(1000);
            win.MoveClick(point.X, point.Y);
            Thread.Sleep(1000);
            win.SendTabKey();
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
                    LogService.Debug(text);
                }

                if (points.Count > 0)
                {
                    var point = points[index];

                    int x = mapX + offsetX + point.X;
                    int y = mapY + offsetY + point.Y;
                    ClickMapPoint(new Point(x, y));
                    LogService.Debug($"点击坐标点=：({mapX}+{offsetX}+{point.X}={x})");
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
            win.SendKey(0xA4);
            // 按下 Shift 键
            win.SendKey(0x10);
        }

        public void loginAuto(int index, string username, string password)
        {
            //WindowUtilities.ChangeInputEn();
            //SwitchToEnglishInput();
            win.MoveClick(574, 132);
            win.MoveClick(543, 445);
            win.MoveClick(612, 454);
            // 删除账号
            // Thread.Sleep(100);
            //
            // wx.MoveClick(459, 265);
            // for (var i = 0; i < 30; i++)
            // {
            //     wx.SendBackspaceKey();
            // }
            //
            // Thread.Sleep(50);
            //
            // wx.SendText(username);
            //
            // wx.MoveClick(459, 290);
            // for (var i = 0; i < 30; i++)
            // {
            //     wx.SendBackspaceKey();
            // }
            //
            // Thread.Sleep(50);
            //
            // wx.SendText(password);

            win.MoveClick(470, 352);
            int userX = 286 + (index * 62);
            int userY = 370;
            win.MoveClick(userX, userY);
            win.MoveClick(515, 417);
            LogService.Log($"角色{index},登录完成");
        }

        // 测试-执行操作的模板方法
        public async Task PerformLoginAsync(int index, string username, string password)
        {
            await ExecuteAsync(async () =>
            {
                await ClickAsync(574, 132); // 进入登录界面
                await ClickAsync(543, 445); // 点击账号输入框
                await ClickAsync(612, 454); // 点击账号输入框
                Thread.Sleep(100);
                await ClickAsync(459, 265);

                await EnterTextAsync(username);
                await ClickAsync(459, 290); // 点击密码输入框
                await EnterTextAsync(password);
                await ClickAsync(515, 417); // 点击登录按钮


                await ClickAsync(286 + (index * 62), 370);
                await ClickAsync(515, 417);
                LogService.Log($"角色 {index} 登录完成");
            });
        }

        // 测试


        // 测试-执行操作的模板方法
        public async Task GameSendMsg(string text)
        {
            await ExecuteAsync(async () =>
            {
                var win = new WindowClickTools(hWnd);
                win.MoveClick(187, 619);
                win.SendText(text);
                win.SendEnterKey();
            });
        }
    }
}