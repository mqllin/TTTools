using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTTools.client;

namespace TTTools.gameTools
{
    internal static class ToolsFunction
    {
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
            win.PushClick(point.Value.X+15, point.Value.Y + 35);
            Thread.Sleep(500);
            //CloseBackPack();
            api.ClosePopupAuto();
            return true;
        }

    }
}
