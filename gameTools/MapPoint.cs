using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TTTools
{
    class MapPoint
    {
        public string LocationCn { get; set; } // 地图中文名
        public string LocationEn { get; set; } // 地图英文名
        public int X { get; set; }
        public int Y { get; set; }

        public MapPoint(string locationCn, string locationEn, int x, int y)
        {
            LocationCn = locationCn;
            LocationEn = locationEn;
            X = x;
            Y = y;
        }

        private static readonly Dictionary<string, MapConfig> mapConfigs = new Dictionary<string, MapConfig>
        {
            {"应天府", new MapConfig("YingTianFu", new Point(2, 36), new Point(400, 293), new Point(485, 354), new Point(0, 0))},
            {"黄泥岗", new MapConfig("HuangNiGang", new Point(1, 33), new Point(398, 256), new Point(239, 145), new Point(0, 25))},
            {"星秀村", new MapConfig("XingXiuCun", new Point(0, 12), new Point(400, 298), new Point(271, 193), new Point(0, 25))},
            {"星秀村东", new MapConfig("XingXiuCunDong", new Point(0, 15), new Point(398, 290), new Point(86, 61), new Point(0, 25))},
            {"盲肠山路", new MapConfig("MangYangShanLu", new Point(0, 12), new Point(400, 298), new Point(198, 148), new Point(0, 25))},
            {"应天府西", new MapConfig("YingTianFuXi", new Point(0, 12), new Point(400, 320), new Point(79, 61), new Point(0, 25))},
            {"应天府西郊", new MapConfig("YingTianFuXiJiao", new Point(36, 0), new Point(334, 317), new Point(158, 149), new Point(0, 25))},
            {"林中小居", new MapConfig("LinZhongXiaoQu", new Point(1, 12), new Point(397, 297), new Point(198, 150), new Point(0, 25))},
            {"应天府东", new MapConfig("YingTianFuDong", new Point(1, 12), new Point(398, 320), new Point(79, 62), new Point(0, 25))},
            {"阳谷县南", new MapConfig("YangGuXianNan", new Point(1, 57), new Point(398, 212), new Point(118, 63), new Point(0, 25))},
            {"阳谷县", new MapConfig("YangGuXian", new Point(1, 12), new Point(398, 297), new Point(198, 149), new Point(0, 25))},
            {"景阳冈", new MapConfig("JingYangGang", new Point(1, 67), new Point(398, 189), new Point(205, 92), new Point(0, 25))},
            {"清河县", new MapConfig("QingHeXian", new Point(1, 45), new Point(350, 210), new Point(180, 90), new Point(0, 25))},
            {"清河湿地", new MapConfig("QingHeShiDi", new Point(1, 50), new Point(360, 220), new Point(200, 100), new Point(0, 25))},
        };

        private static readonly Dictionary<string, List<Route>> mapRoutes = new Dictionary<string, List<Route>>
        {
            {"星秀村", new List<Route> { new Route("星秀村东", new Point(270, 159)) }},
            {"星秀村东", new List<Route> { new Route("盲肠山路", new Point(83, 16)) }},
            {"应天府", new List<Route>
                {
                    new Route("黄泥岗", new Point(10, 332)),
                    new Route("应天府西", new Point(5, 15)),
                    new Route("应天府东", new Point(270, 160))
                }
            },
            {"应天府西", new List<Route> { new Route("应天府西郊", new Point(5, 35)) }},
            {"应天府东", new List<Route> { new Route("应天府东郊", new Point(200, 150)) }},
            {"应天府西郊", new List<Route>
                {
                    new Route("林中小区", new Point(250, 100))
                }
            },
            {"阳谷县南", new List<Route> { new Route("阳谷县", new Point(300, 200)) }},
            {"阳谷县", new List<Route> { new Route("景阳冈", new Point(400, 300)) }},
            {"景阳冈", new List<Route>
                {
                    new Route("清河县", new Point(100, 70)),
                    new Route("清河湿地", new Point(120, 80))
                }
            },
        };

        public override string ToString()
        {
            return $"地点: {LocationCn} ({LocationEn}), X坐标: {X}, Y坐标: {Y}";
        }

        /// <summary>
        /// 查询地图配置（通过中文名或英文名）
        /// </summary>
        public static MapConfig GetMapConfig(string locationName)
        {
            foreach (var entry in mapConfigs)
            {
                if (entry.Key.Equals(locationName, StringComparison.OrdinalIgnoreCase) ||
                    entry.Value.EnglishName.Equals(locationName, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value;
                }
            }
            throw new KeyNotFoundException($"未找到名为 {locationName} 的地图配置。");
        }

        /// <summary>
        /// 根据当前地图名称和目标地图名称，返回移动路线
        /// </summary>
        public static List<Route> GetRoute(string currentMap, string targetMap)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<List<Route>>();

            if (!mapRoutes.ContainsKey(currentMap))
                throw new KeyNotFoundException($"地图 {currentMap} 无路由信息。");

            queue.Enqueue(new List<Route> { new Route(currentMap, Point.Empty) });

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var lastNode = path.Last();

                if (lastNode.NextMap == targetMap)
                    return path;

                if (!visited.Contains(lastNode.NextMap) && mapRoutes.ContainsKey(lastNode.NextMap))
                {
                    visited.Add(lastNode.NextMap);
                    foreach (var route in mapRoutes[lastNode.NextMap])
                    {
                        var newPath = new List<Route>(path) { route };
                        queue.Enqueue(newPath);
                    }
                }
            }

            throw new InvalidOperationException($"无法从 {currentMap} 到达 {targetMap}。");
        }
    }

    class MapConfig
    {
        public string EnglishName { get; }
        public Point Offset { get; } // 地图左上角偏移量
        public Point MapPx { get; }  // 地图像素尺寸
        public Point MapMaxPoint { get; }  // 地图坐标尺寸
        public Point MapClickOffset { get; }  // 点击偏移

        public MapConfig(string englishName, Point offset, Point mapPx, Point mapMaxPoint, Point mapClickOffset)
        {
            EnglishName = englishName;
            Offset = offset;
            MapPx = mapPx;
            MapMaxPoint = mapMaxPoint;
            MapClickOffset = mapClickOffset;
        }

        public override string ToString()
        {
            return $"地图英文名: {EnglishName}, 偏移量: ({Offset.X}, {Offset.Y}), 尺寸: ({MapPx.X}, {MapPx.Y})";
        }
    }

    class Route
    {
        public string NextMap { get; }
        public Point ExitPoint { get; }

        public Route(string nextMap, Point exitPoint)
        {
            NextMap = nextMap;
            ExitPoint = exitPoint;
        }

        public override string ToString()
        {
            return $"前往: {NextMap}, 出口坐标: ({ExitPoint.X}, {ExitPoint.Y})";
        }
    }
}
