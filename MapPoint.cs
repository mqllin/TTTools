using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TTTools
{
    class MapPoint
    {
        public string Location { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public MapPoint(string location, int x, int y)
        {
            Location = location;
            X = x;
            Y = y;

        }
        private static Dictionary<string, (Point anchorOffset, Point mapMax, Point anchorMax)> mapConfigs = new Dictionary<string, (Point anchorOffset, Point mapMax, Point anchorMax)>
        {
            {"应天府", (new Point(-392, 38), new Point(485, 354), new Point(9, 332))},
            {"应天府东", (new Point(-392, 24), new Point(80, 62), new Point(9, 345))},
            // 其他地图，格式：{"地图名", (锚点偏移, 地图最大坐标, 锚点最大坐标)}
        };

        public override string ToString()
        {
            return $"地点: {Location}, X坐标: {X}, Y坐标: {Y}";
        }

        public Point GetPx()
        {
            if (!mapConfigs.ContainsKey(Location))
            {
                throw new Exception($"未配置的地图：{Location}");
            }

            var config = mapConfigs[Location];
            Point anchorOffset = config.anchorOffset;
            Point mapMax = config.mapMax;
            Point anchorMax = config.anchorMax;

            // 转换比例
            float ratioX = (float)(anchorMax.X - anchorOffset.X) / mapMax.X;
            float ratioY = (float)(anchorMax.Y - anchorOffset.Y) / mapMax.Y;

            // 计算相对于锚点的坐标
            int anchorX = (int)(X * ratioX + anchorOffset.X);
            int anchorY = (int)(Y * ratioY + anchorOffset.Y);

            Console.WriteLine($"相对于锚点的坐标是 ({anchorX}, {anchorY})");
            return new Point(anchorX, anchorY);
        }
    }
}
