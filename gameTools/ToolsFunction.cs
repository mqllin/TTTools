using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using TTTools.client;
using TTTools.windowsTools;

namespace TTTools.gameTools
{
    internal static class ToolsFunction
    {
        private static string dataDirectory = "./data/map/";

        // 是否已经持有打宝图任务
        public static void IsHasWaBaoTask(bool autoRemove = false )
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            api.IsHasWaBaoTask(autoRemove);
        }

        // 是否使用驱魔香
        public static void IsUseQuMoXiang()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            var use =  api.IsUseQuMoXiang(true);
        }

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
                LogService.Log("关闭背包");
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
        } // 打开背包

        public static void OpenTask()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            api.OpenTask();
        }

        public static async Task<bool> MoveToMapAsync(string targetMapName)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);
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
                ToolsFunction.MapGotoClick(currentMapName, nextRoute.ExitPoint.X, nextRoute.ExitPoint.Y);

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


        // 移动到地图
        public static bool MoveToMapByFly(string name)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);
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
                win.MoveMouse(point.Value.X + 1, point.Value.Y + 15, false);
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

        public static bool OpenMapAndMoveToPoint(int x, int y)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);

            win.MoveMouse(0, 0);
            Point? point = api.FindSomeThingInMapByFileName("ui", "mapPoint");
            if (point == null)
            {
                api.OpenMap();
                Thread.Sleep(500);
            }

            int xx = x;
            int yy = y;
            win.MoveMouse(x, y, false);
            Thread.Sleep(500);
            var img = pic.CaptureWindow(xx + 20, yy - 10, 144, 15);
            pic.SaveImage(img);
            var findxy = pic.FindXyInBitmap(img);

            if (findxy != null)
            {
                LogService.Log($"findxy={findxy.ToString()}");
            }
            else
            {
                LogService.Log("list count =0");
            }

            return true;
        }

        public static void up()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
            win.PushClick(screenCenter.X, screenCenter.Y - 15);
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
            win.PushClick(screenCenter.X - 1, screenCenter.Y);
        }

        public static void right()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
            win.PushClick(screenCenter.X + 1, screenCenter.Y);
        }

        public static void center()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            Point screenCenter = new Point(806 / 2, 692 / 2 - 22);
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
            int anchorX = point.Value.X + offset.X;
            int anchorY = point.Value.Y + offset.Y;

            int startX = anchorX; // 起点X
            int startY = anchorY; // 起点Y
            int endX = startX + MapPx.X; // 地图右下角的X坐标
            int endY = startY + MapPx.Y; // 地图右下角的Y坐标

            // 定义存储行和列的增量数据
            var rowPoints = new List<(int DeltaX, int GameX)>();
            var colPoints = new List<(int DeltaY, int GameY)>();

            // 扫描上边第一行
            for (int x = startX; x <= endX; x++)
            {
                win.MoveMouse(x, startY, false);
                var img = pic.CaptureWindow(x - 30, startY - 10, 160, 15);
                pic.SaveImage(img);
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
                pic.SaveImage(img);
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

        public static bool OpenMapAndMoveToPointAuto2(string mapName)
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
            int anchorX = point.Value.X + offset.X;
            int anchorY = point.Value.Y + offset.Y;

            // 定义存储行和列的增量数据
            var rowPoints = new List<(int DeltaX, int GameX)>();
            var colPoints = new List<(int DeltaY, int GameY)>();

            // 仅采集前 20 个像素的行值
            for (int x = anchorX; x < anchorX + 20 && x <= anchorX + MapPx.X; x++)
            {
                win.MoveMouse(x, anchorY, false);
                var img = pic.CaptureWindow(x - 30, anchorY - 10, 160, 15);
                var findxy = pic.FindXyInBitmap(img);

                if (findxy != null)
                {
                    int deltaX = x - anchorX;
                    rowPoints.Add((deltaX, findxy.Value.X));
                }
            }

            // 仅采集前 20 个像素的列值
            for (int y = anchorY; y < anchorY + 20 && y <= anchorY + MapPx.Y; y++)
            {
                win.MoveMouse(anchorX, y, false);
                var img = pic.CaptureWindow(anchorX + 20, y - 10, 144, 15);
                var findxy = pic.FindXyInBitmap(img);

                if (findxy != null)
                {
                    int deltaY = y - anchorY;
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

            // 自动补齐行和列
            int totalRows = MapPx.X / rowStep;
            int totalCols = MapPx.Y / colStep;

            for (int i = rowPoints.Count; i < totalRows; i++)
            {
                int deltaX = rowPoints[0].DeltaX + i * rowStep;
                int gameX = rowPoints[0].GameX + i * (rowPoints[1].GameX - rowPoints[0].GameX);
                rowPoints.Add((deltaX, gameX));
            }

            for (int i = colPoints.Count; i < totalCols; i++)
            {
                int deltaY = colPoints[0].DeltaY + i * colStep;
                int gameY = colPoints[0].GameY + i * (colPoints[1].GameY - colPoints[0].GameY);
                colPoints.Add((deltaY, gameY));
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
        public static (int DeltaX, int DeltaY, int GameX, int GameY)? FindClosestCoordinate(string mapName, int gameX,
            int gameY)
        {
            // 定义嵌入资源路径（命名空间.Resources.文件名）
            string resourcePath = $"TTTools.data.map.{mapName}.pak";

            // 加载嵌入资源
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    LogService.Log($"嵌入资源文件不存在: {resourcePath}");
                    return null;
                }

                var coordinates = new List<(int DeltaX, int DeltaY, int GameX, int GameY)>();

                // 从嵌入资源中读取数据
                using (var binaryReader = new BinaryReader(stream))
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
                    LogService.Log(
                        $"找到最接近的坐标: DeltaX={closest.Value.DeltaX}, DeltaY={closest.Value.DeltaY}, GameX={closest.Value.GameX}, GameY={closest.Value.GameY}");
                }
                else
                {
                    LogService.Log("没有找到任何接近的坐标。");
                }

                return closest;
            }
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

        public static bool MapGotoClick(string mapName, int x, int y)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var win = new WindowClickTools(hWnd);
            var api = new Method(hWnd);
            var pic = new PictureMethod(hWnd);
            var config = MapPoint.GetMapConfig(mapName);
            var offset = config.Offset;
            if (!api.IsMapOpen())
            {
                api.OpenMap();
            }

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
                int startX = point.Value.X + offset.X;
                int startY = point.Value.Y + offset.Y;

                win.MoveClick(startX + closestCoordinate.Value.DeltaX + config.MapClickOffset.X,
                    startY + closestCoordinate.Value.DeltaY + config.MapClickOffset.Y);
                // LogService.Log($"最接近的行增量: {closestCoordinate.Value.DeltaX}, 列增量: {closestCoordinate.Value.DeltaY}");
                LogService.Log($"游戏坐标: ({closestCoordinate.Value.GameX}, {closestCoordinate.Value.GameY})");
                api.CloseMap();
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
            var win = new WindowClickTools(hWnd);

            var pic = new PictureMethod(hWnd);
            var name = pic.GetCurrentMapName();
            LogService.Log($"当前在：{name}");
            return name;
        }

        
        public static async Task WabaoTaskAsync()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            var win = new WindowClickTools(hWnd);
            var pic = new PictureMethod(hWnd);
            win.MoveMouse(800, 0);
            IsUseQuMoXiang();
            IsHasWaBaoTask(true);
            await MoveToMapAsync("应天府");
            Thread.Sleep(2000);
            MapGotoClick("应天府", 280, 152);
            // 等待移动完成
            pic.WaitForMovementToStop();
            win.MoveClick(491 - 50, 287);
            Point? point1 = api.FindSomeThingInMapByFileName("ui", "打宝图对话1");
            if (point1 == null)
            {
                LogService.Log("没有找到打宝图对话1");
                return;
            }

            win.MoveClick(point1.Value.X, point1.Value.Y + 2, false, false);
            win.MoveMouse(0, 0);
            Point? point2 = api.FindSomeThingInMapByFileName("ui", "打宝图对话2");
            if (point2 == null)
            {
                LogService.Log("没有找到打宝图对话2");
                return;
            }

            win.MoveClick(point2.Value.X, point2.Value.Y, false, false);
            var taskImg = api.GetPopupImage();
            // var taskImg = ResourceLoader.LoadBitmap("data.素材.测试宝图对话.png");
            Bitmap handleImageYellow = pic.ReplaceColor(taskImg, "#f8fcf8", "#ffff00");
            handleImageYellow = pic.ReplaceColor(handleImageYellow, "#29548b", "#3978ac");
            pic.SaveImage(handleImageYellow, "handleImageYellow");
            var mapName = pic.GetCurrentMapNameByImage(handleImageYellow);
            Bitmap handleImageWhite = pic.ReplaceColor(handleImageYellow, "#ffff00", "#ffffff");
            Bitmap handleImageBlack = pic.ReplaceOtherColor(handleImageWhite, "#ffffff", "#000000");
            pic.SaveImage(handleImageBlack, "handleImageBlack");
            // 预处理截图，将非白色像素设置为透明
            // Bitmap handleImagePng = pic.PreprocessScreenshot(handleImageWhite);
            var kuohao1 = ResourceLoader.LoadBitmap("data.xy.坐标括号1.png");
            var kuohao2 = ResourceLoader.LoadBitmap("data.xy.坐标括号2.png");
            var p1 = pic.FindImageInImage(handleImageBlack, kuohao1);
            var p2 = pic.FindImageInImage(handleImageBlack, kuohao2);
            if (p1 == null)
            {
                LogService.Log($"p1没找到");
                return;
            }

            if (p2 == null)
            {
                LogService.Log($"p2没找到");
                return;
            }

            var width = p2.Value.X - p1.Value.X;
            var centerImage = pic.CaptureFromBitmap(handleImageBlack, p1.Value.X + 3, p1.Value.Y, width - 3, 20);
            pic.SaveImage(centerImage);
            var pointXy = pic.FindXyInBitmapBlack(centerImage);
            if (pointXy != null)
            {
                win.MoveClick(point2.Value.X + 10, point2.Value.Y + 10, true, false);

                LogService.Log($"寻找强盗：{mapName}:({pointXy.Value.X},{pointXy.Value.Y})");
                await MoveToMapAsync(mapName);
                ToolsFunction.MapGotoClick(mapName, pointXy.Value.X, pointXy.Value.Y);

                // CloseBackPack();
                // MoveToMapByFly(mapName);
                // ToolsFunction.MapGotoClick(mapName, pointXy.Value.X, pointXy.Value.Y);
                // pic.WaitForMovementToStop();
                // OpenBackPack();
                // win.MoveClick(p.Value.X + 20, p.Value.Y, true);
                // LogService.Log($"寻找宝图:完成一次");
            }
            else
            {
                LogService.Log($"没有找到{mapName}的强盗坐标");
            }
        }

        //用来测试宝图的位置
        public static string WabaoTest()
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            var win = new WindowClickTools(hWnd);

            var pic = new PictureMethod(hWnd);
            win.MoveMouse(800, 0);
            OpenBackPack();
            Thread.Sleep(500);
            var p = pic.FindSomeInBackpack("cangBaoTu");
            if (p != null)
            {
                win.MoveMouse(p.Value.X - 20, p.Value.Y);
                win.MoveMouse(p.Value.X - 22, p.Value.Y);
                win.MoveMouse(p.Value.X - 20, p.Value.Y);
                Thread.Sleep(1000);
                Bitmap targetImage = pic.CaptureWindow(p.Value.X - 320, p.Value.Y + 125, 280, 20);

                Bitmap handleImage = pic.ReplaceOtherColor(targetImage, "#f8fc00");
                pic.SaveImage(handleImage, "handleImage");

                Bitmap handleImageYellow = pic.ReplaceColor(handleImage, "#f8fc00", "#ffff00");
                pic.SaveImage(handleImageYellow, "handleImageYellow");
                var mapName = pic.GetCurrentMapNameByImage(handleImageYellow);
                if (mapName != null)
                {
                    Bitmap handleImageWhite = pic.ReplaceColor(handleImageYellow, "#ffff00", "#ffffff");
                    Bitmap handleImageBlack = pic.ReplaceOtherColor(handleImageWhite, "#ffffff", "#000000");
                    pic.SaveImage(handleImageBlack, "handleImageBlack");
                    // 预处理截图，将非白色像素设置为透明
                    // Bitmap handleImagePng = pic.PreprocessScreenshot(handleImageWhite);
                    var pointXy = pic.FindXyInBitmapBlack(handleImageBlack);
                    if (pointXy != null)
                    {
                        LogService.Log($"寻找宝图：{mapName}:({pointXy.Value.X},{pointXy.Value.Y})");
                        CloseBackPack();
                        MoveToMapByFly(mapName);
                        ToolsFunction.MapGotoClick(mapName, pointXy.Value.X, pointXy.Value.Y);
                        pic.WaitForMovementToStop();
                        OpenBackPack();
                        win.MoveClick(p.Value.X + 20, p.Value.Y, true);
                        LogService.Log($"寻找宝图:完成一次");
                    }
                    else
                    {
                        LogService.Log($"没有找到{mapName}的宝图坐标");
                    }
                }
                else
                {
                    LogService.Log("没找到宝图中的地图名称");
                }
            }
            else
            {
                LogService.Log("没有找到藏宝图");
            }

            win.MoveMouse(800, 0);
            return "";
        }
    }
}