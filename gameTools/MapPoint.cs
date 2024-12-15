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

        // 地图配置（原样保留）
        private static readonly Dictionary<string, MapConfig> mapConfigs = new Dictionary<string, MapConfig>
        {
            {"应天府", new MapConfig("YingTianFu", new Point(2, 12), new Point(400, 293), new Point(485, 354), new Point(0, 25))},
            {"黄泥岗", new MapConfig("HuangNiGang", new Point(1, 33), new Point(398, 256), new Point(239, 145), new Point(0, 25))},
            {"星秀村", new MapConfig("XingXiuCun", new Point(0, 12), new Point(400, 298), new Point(271, 193), new Point(0, 25))},
            {"星秀村2", new MapConfig("XingXiuCun", new Point(0, 12), new Point(400, 298), new Point(271, 193), new Point(0, 25))},
            {"星秀村东", new MapConfig("XingXiuCunDong", new Point(0, 15), new Point(398, 290), new Point(86, 61), new Point(0, 25))},
            {"芒砀山麓", new MapConfig("MangDangShanLu", new Point(0, 12), new Point(400, 298), new Point(198, 148), new Point(0, 25))},
            {"应天府西", new MapConfig("YingTianFuXi", new Point(0, 12), new Point(400, 320), new Point(79, 61), new Point(0, 25))},
            {"应天西郊", new MapConfig("YingTianXiJiao", new Point(36, 0), new Point(400, 317), new Point(158, 149), new Point(0, 25))},
            {"林中小居", new MapConfig("LinZhongXiaoQu", new Point(1, 12), new Point(400, 297), new Point(198, 150), new Point(0, 25))},
            {"应天府东", new MapConfig("YingTianFuDong", new Point(1, 12), new Point(400, 320), new Point(79, 62), new Point(0, 25))},
            {"应天东郊", new MapConfig("YingTianDongJiao", new Point(1, 12), new Point(400, 300), new Point(198, 143), new Point(0, 25))},
            {"阳谷县南", new MapConfig("YangGuXianNan", new Point(1, 57), new Point(400, 212), new Point(118, 63), new Point(0, 25))},
            {"阳谷县", new MapConfig("YangGuXian", new Point(1, 12), new Point(400, 297), new Point(198, 149), new Point(0, 25))},
            {"景阳冈", new MapConfig("JingYangGang", new Point(1, 67), new Point(400, 189), new Point(205, 92), new Point(0, 25))},
            {"清河县", new MapConfig("QingHeXian", new Point(1, 65), new Point(400, 200), new Point(236, 116), new Point(0, 25))},
            {"清河湿地", new MapConfig("QingHeShiDi", new Point(1, 50), new Point(400, 220), new Point(200, 100), new Point(0, 25))},
        };

        // 地图传送节点（新增）
        public static readonly Dictionary<string, List<Route>> mapOutPoint = new Dictionary<string, List<Route>>
        {
            {"星秀村", new List<Route> { new Route("星秀村东", new Point(270, 161)) }},
            {"星秀村东", new List<Route> { new Route("芒砀山麓", new Point(83, 18)) }},
            {"应天府", new List<Route>
                {
                    new Route("黄泥岗", new Point(10, 332)),
                    new Route("应天府西", new Point(5, 15)),
                    new Route("应天府东", new Point(486, 297))
                }
            },
            {"应天府西", new List<Route> { new Route("应天府西郊", new Point(5, 35)) }},
            {"应天府东", new List<Route> { new Route("应天府东郊", new Point(76, 30)) }},
            {"阳谷县", new List<Route>
            {
                new Route("景阳冈", new Point(14, 6)),
                new Route("阳谷县南", new Point(75, 149)),
            }},
            {"景阳冈", new List<Route>
                {
                    new Route("景阳冈东", new Point(199, 16)),
                    new Route("清河县", new Point(2, 27)),
                    new Route("清河湿地", new Point(85, 1))
                }
            },
        };

        // 路由配置：记录地图之间的完整路径
        private static readonly Dictionary<string, List<string>> mapRoutes = new Dictionary<string, List<string>>
        {
            { "星秀村东", new List<string> { "星秀村", "星秀村东" } },
            { "芒砀山麓", new List<string> { "星秀村", "星秀村东", "芒砀山麓" } },
            { "应天府西", new List<string> { "应天府", "应天府西" } },
            { "应天西郊", new List<string> { "应天府", "应天府西","应天西郊" } },
            { "林中小居", new List<string> { "应天府", "应天府西","应天西郊","林中小居" } },
            { "应天府东", new List<string> { "应天府", "应天府东" } },
            { "应天东郊", new List<string> { "应天府", "应天府东","应天东郊" } },
            { "阳谷县南", new List<string> { "阳谷县", "阳谷县南" } },
            { "景阳冈", new List<string> { "阳谷县", "景阳冈" } }
        };

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
        /// 获取目标地图的完整路径
        /// </summary>
        public static List<string> GetRouteToMap(string targetMap)
        {
            if (!mapRoutes.ContainsKey(targetMap))
                throw new KeyNotFoundException($"目标地图 {targetMap} 无路由信息。");

            return mapRoutes[targetMap];
        }

        /// <summary>
        /// 获取地图的传送节点
        /// </summary>
        public static List<Route> GetOutPoints(string mapName)
        {
            if (!mapOutPoint.ContainsKey(mapName))
                throw new KeyNotFoundException($"地图 {mapName} 无传送节点信息。");

            return mapOutPoint[mapName];
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
    }
}
