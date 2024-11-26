using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTTools.client;

namespace TTTools.gameTools
{
    internal static class ToolsFunction
    {
        private static string dataDirectory = "./data/map/";


        // 使用物品
        public static void UseBagItem(string name)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            if (api.IsBackpackOpen())
            {
                api.UseBagItem(name);
            }
        }

        // 关闭背包
        public static void CloseBackPack()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            if (api.IsBackpackOpen())
            {
                api.ClickBackpack();
            }
        }
        // 打开背包
        public static void OpenBackPack()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            if (!api.IsBackpackOpen())
            {
                api.ClickBackpack();
            }
        }

        // 移动到地图
        public static bool MoveToMap(string name)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);

            // 打开背包
            OpenBackPack();
            var cityMap = new Dictionary<string, string>
            {
                {"星秀村", "xingXiuCun"},
                {"应天府", "yingTianFu"},
                {"汴京城", "bianJingCheng"},
                {"清河县", "qingHeXian"},
                {"阳谷县", "yangGuXian"}
            };
            // 获取 name 对应的 code
            if (!cityMap.TryGetValue(name, out var targetCode))
            {
                return false;
            }
            UseBagItem("回程符");
            win.MoveMouse(800, 0);
            Point? point = api.FindSomeThingInMapByFileName("ui", targetCode);
            if (point == null)
            {
                return false;
            }
            win.MoveClick(point.Value.X + 15, point.Value.Y + 35);
            Thread.Sleep(500);
            //CloseBackPack();
            api.ClosePopupAuto();
            return true;
        }


        // 打开地图
        public static bool OpenMap()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);
            win.MoveMouse(0, 0);
            Point? point = api.FindSomeThingInMapByFileName("ui", "mapPoint");
            if (point != null)
            {
                
                win.MoveMouse(point.Value.X+1,point.Value.Y+15,false);
                //win.PushClick(point.Value.X, point.Value.Y + 10);
                return true;
            }
            else
            {
                api.OpenMap();
                Thread.Sleep(500);
                win.MoveClick(point.Value.X, point.Value.Y + 10);
                return true;
            }
        }

        public static bool OpenMapAndMoveToPoint(int x,int y)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);

            win.MoveMouse(0, 0);
            Point? point = api.FindSomeThingInMapByFileName("ui", "mapPoint");
            if (point != null)
            {
                int xx =x;
                int yy =y;
                win.MoveMouse(x,y,false);
                Thread.Sleep(500);
                var img =  pic.CaptureWindow(xx+20, yy-10, 144, 15);
                pic.SaveImage(img);         
                var findxy = pic.FindXyInBitmap(img);

                if (findxy!=null)
                {
                    LogService.Log($"findxy={findxy.ToString()}");

                }
                else
                {
                    LogService.Log("list count =0");
                }
                return true;
            }
            else
            {
                api.OpenMap();
                Thread.Sleep(500);
                //win.MoveMouse(point.Value.X + x, point.Value.Y + y, false);
                return true;
            }
        }
        public static void up()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
            win.PushClick(screenCenter.X, screenCenter.Y -15);
        }
        public static void down()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
            win.PushClick(screenCenter.X, screenCenter.Y + 1);
        }
        public static void lfet()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
            win.PushClick(screenCenter.X-1, screenCenter.Y );
        }
        public static void right()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
            win.PushClick(screenCenter.X+1, screenCenter.Y);
        }
        public static void center()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2-22);
            win.PushClick(screenCenter.X, screenCenter.Y);
        }
        //public static bool OpenMapAndMoveToPointAuto()
        //{
        //    var hWnd = ClientManager.CurrentSelectedClient.HWnd;
        //    var win = new WindowClickTools(hWnd);
        //    var api = new Method(hWnd);
        //    var pic = new PictureMethod(hWnd);

        //    win.MoveMouse(0, 0);
        //    Point? point = api.FindSomeThingInMapByFileName("ui", "mapPoint");

        //    if (point != null)
        //    {
        //        int startX = point.Value.X + 0;
        //        int startY = point.Value.Y + 12;
        //        int endX = startX + 485;
        //        int endY = startY + 354;

        //        for (int yy = startY; yy <= endY; yy++)
        //        {
        //            for (int xx = startX; xx <= endX; xx++)
        //            {
        //                // 移动鼠标到当前坐标
        //                win.MoveMouse(xx, yy, false);

        //                // 截图当前区域进行坐标分析
        //                var img = pic.CaptureWindow(xx + 20, yy - 10, 144, 15);
        //                var findxy = pic.FindXyInBitmap(img);

        //                // 输出格式化的坐标信息
        //                if (findxy != null)
        //                {
        //                    LogService.Log($"{xx},{yy}={findxy.Value.X},{findxy.Value.Y}");
        //                }
        //                else
        //                {
        //                    LogService.Log($"{xx},{yy}=null");
        //                }

        //                // 控制速度
        //                Thread.Sleep(1);
        //            }
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        api.OpenMap();
        //        Thread.Sleep(500);
        //        return false;
        //    }
        //}


        public static bool OpenMapAndMoveToPointAuto(string mapName)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);
            var config = MapPoint.GetMapConfig(mapName);
            var MapPx = config.MapPx;
            var offset = config.Offset;
            win.MoveMouse(0, 0);

            // 首先找到地图起点
            Point? point = api.FindSomeThingInMapByFileName("ui", "mapPoint");
            if (point == null)
            {
                LogService.Log("地图起点未找到");
                return false;
            }

            // 锚点起始位置
            int anchorX = point.Value.X+ offset.X;
            int anchorY = point.Value.Y+ offset.Y;

            int startX = anchorX;   // 起点X
            int startY = anchorY ; // 起点Y
            int endX = startX + MapPx.X;   // 地图右下角的X坐标
            int endY = startY + MapPx.Y;   // 地图右下角的Y坐标

            // 定义存储行和列的增量数据
            var rowPoints = new List<(int DeltaX, int GameX)>();
            var colPoints = new List<(int DeltaY, int GameY)>();

            // 扫描上边第一行
            for (int x = startX; x <= endX; x++)
            {
                win.MoveMouse(x, startY, false);
                var img = pic.CaptureWindow(x + 20, startY - 10, 144, 15);
                var findxy = pic.FindXyInBitmap(img);

                if (findxy != null)
                {
                    int deltaX = x - anchorX; // 记录相对于锚点的增量X
                    LogService.Log($"Row DeltaX {deltaX} -> GameX: {findxy.Value.X}");
                    rowPoints.Add((deltaX, findxy.Value.X));
                }
            }

            // 扫描左边第一列
            for (int y = startY; y <= endY; y++)
            {
                win.MoveMouse(startX, y, false);
                var img = pic.CaptureWindow(startX + 20, y - 10, 144, 15);
                var findxy = pic.FindXyInBitmap(img);

                if (findxy != null)
                {
                    int deltaY = y - anchorY; // 记录相对于锚点的增量Y
                    LogService.Log($"Col DeltaY {deltaY} -> GameY: {findxy.Value.Y}");
                    colPoints.Add((deltaY, findxy.Value.Y));
                }
            }

            // 推导行列间距
            int rowStep = rowPoints.Count > 1 ? rowPoints[1].DeltaX - rowPoints[0].DeltaX : 0;
            int colStep = colPoints.Count > 1 ? colPoints[1].DeltaY - colPoints[0].DeltaY : 0;

            // 检查间距是否有效
            if (rowStep == 0 || colStep == 0)
            {
                LogService.Log("无法计算行列间距，可能数据不足");
                return false;
            }

            // 推导全地图的点
            var coordinateList = new List<(int DeltaX, int DeltaY, int GameX, int GameY)>();
            for (int row = 0; row < rowPoints.Count; row++)
            {
                for (int col = 0; col < colPoints.Count; col++)
                {
                    int deltaX = rowPoints[row].DeltaX;
                    int deltaY = colPoints[col].DeltaY;

                    int gameX = rowPoints[row].GameX;
                    int gameY = colPoints[col].GameY;

                    coordinateList.Add((deltaX, deltaY, gameX, gameY));
                }
            }

            // 将所有增量坐标存入文件
            string filePath = Path.Combine(dataDirectory, $"{mapName}.pak");
            Directory.CreateDirectory(dataDirectory);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                foreach (var coord in coordinateList)
                {
                    binaryWriter.Write(coord.DeltaX); // 增量X
                    binaryWriter.Write(coord.DeltaY); // 增量Y
                    binaryWriter.Write(coord.GameX);
                    binaryWriter.Write(coord.GameY);
                }
            }

            LogService.Log($"地图扫描完成，共记录 {coordinateList.Count} 个坐标点。");
            return true;
        }



        //查询游戏坐标
        public static (int DeltaX, int DeltaY, int GameX, int GameY)? FindClosestCoordinate(string mapName, int gameX, int gameY)
        {
            string filePath = Path.Combine(dataDirectory, $"{mapName}.pak");

            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                LogService.Log($"坐标文件不存在: {filePath}");
                return null;
            }

            var coordinates = new List<(int DeltaX, int DeltaY, int GameX, int GameY)>();

            // 读取文件中的坐标
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    int deltaX = binaryReader.ReadInt32();
                    int deltaY = binaryReader.ReadInt32();
                    int recordedGameX = binaryReader.ReadInt32();
                    int recordedGameY = binaryReader.ReadInt32();

                    coordinates.Add((deltaX, deltaY, recordedGameX, recordedGameY));
                }
            }

            // 找到最接近的坐标
            (int DeltaX, int DeltaY, int GameX, int GameY)? closest = null;
            int minDifference = int.MaxValue;

            foreach (var coord in coordinates)
            {
                // 计算差值之和
                int difference = Math.Abs(coord.GameX - gameX) + Math.Abs(coord.GameY - gameY);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closest = coord;
                }
            }

            if (closest != null)
            {
                LogService.Log($"找到最接近的坐标: DeltaX={closest.Value.DeltaX}, DeltaY={closest.Value.DeltaY}, GameX={closest.Value.GameX}, GameY={closest.Value.GameY}");
            }
            else
            {
                LogService.Log("没有找到任何接近的坐标。");
            }

            return closest;
        }

        public static void ExportCoordinateDataToText(string mapName)
        {
            string filePath = Path.Combine(dataDirectory, $"{mapName}.pak");
            string outputFilePath = Path.Combine(dataDirectory, $"{mapName}.txt");

            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                LogService.Log($"坐标文件不存在: {filePath}");
                return;
            }

            var coordinates = new List<(int DeltaX, int DeltaY, int GameX, int GameY)>();

            // 读取文件中的坐标
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    int deltaX = binaryReader.ReadInt32();
                    int deltaY = binaryReader.ReadInt32();
                    int recordedGameX = binaryReader.ReadInt32();
                    int recordedGameY = binaryReader.ReadInt32();

                    coordinates.Add((deltaX, deltaY, recordedGameX, recordedGameY));
                }
            }

            // 按行分组
            var groupedByRow = coordinates.GroupBy(coord => coord.DeltaX)
                                           .OrderBy(group => group.Key);

            // 写入文件
            using (var writer = new StreamWriter(outputFilePath))
            {
                foreach (var rowGroup in groupedByRow)
                {
                    string rowString = string.Join(", ", rowGroup.Select(coord =>
                        $"[{coord.DeltaX},{coord.DeltaY}={coord.GameX},{coord.GameY}]"));
                    writer.WriteLine(rowString);
                }
            }

            LogService.Log($"坐标数据已成功导出到: {outputFilePath}");
        }
        public static bool MapGotoClick(string mapName,int x,int y)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);
            var config = MapPoint.GetMapConfig(mapName);
            var offset = config.Offset;

            var closestCoordinate = ToolsFunction.FindClosestCoordinate(mapName, x, y);
            if (closestCoordinate != null)
            {
                Point? point = api.FindSomeThingInMapByFileName("ui", "mapPoint");
                if (point == null)
                {
                    LogService.Log("地图起点未找到");
                    return false;
                }

                // 锚点起始位置
                int startX = point.Value.X+ offset.X;
                int startY = point.Value.Y+ offset.Y;

                win.MoveClick(startX+closestCoordinate.Value.DeltaX + config.MapClickOffset.X, startY+closestCoordinate.Value.DeltaY+config.MapClickOffset.Y);
                LogService.Log($"最接近的行增量: {closestCoordinate.Value.DeltaX}, 列增量: {closestCoordinate.Value.DeltaY}");
                LogService.Log($"游戏坐标: ({closestCoordinate.Value.GameX}, {closestCoordinate.Value.GameY})");
                return true;
            }
            else
            {
                LogService.Log("没有找到接近的坐标！");
                return false;
            }
        }
        
        public static string GetCurrentMapName()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;

            var pic = new PictureMethod(hWnd);
            var name = pic.GetCurrentMapName();
            LogService.Log(name);
            return name;
        }

    }
}
