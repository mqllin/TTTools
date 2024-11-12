using System;
using System.Drawing;
using System.Threading;

namespace TTTools
{
    internal class ActionTreasureMethod
    {
        private readonly IntPtr hWnd;
        private readonly WindowClickTools wx;
        private readonly Method api;
        private readonly PictureMethod pic;
        private readonly Form1 Instance;

        public ActionTreasureMethod(IntPtr hWnd, Form1 Instance)
        {
            this.hWnd = hWnd;
            this.wx = new WindowClickTools(hWnd);
            this.api = new Method(hWnd);
            this.Instance = Instance;
            this.pic = new PictureMethod(hWnd);

        }

        public void Run()
        {

            api.UseBagItem(2);
            MapPoint npc = new MapPoint("应天府",280,152);
            npc.X = npc.X + 1;
            var lastPoint =  api.ClickTabMapPoint(npc.GetPx());
            // 等待移动完成
            api.AwaitMoving();
            api.ClickSelf();
            Thread.Sleep(1000);
            api.ClickPopupItemAuto(new Point(59, 50));
            Thread.Sleep(1000);

            Point? popupPoint = api.FindSomeThingInMapByFileName("ui","popup");
            if (popupPoint!=null) {
                CaptureAndOCR orc = new CaptureAndOCR(hWnd, popupPoint.Value.X, popupPoint.Value.Y+26, 439, 31);
                MapPoint location = orc.CaptureAndRecognize();
                if (location != null)
                {
                    Thread.Sleep(1000);
                    LogService.Log("接到盗宝贼任务：" + location.ToString());
                    api.ClosePopupAuto();
                    Thread.Sleep(1000);
                    var toLocation = location.Location;
                   
                    var nextPoint = api.GetMapExportPoin("应天府",location.Location);
                    if (nextPoint != null) {
                        // 前往目标地图
                        api.ClickTabMapPoint((Point)nextPoint);
                        api.AwaitMoving();

                    }

                }
                else
                {
                    LogService.Log("阅读任务内容失败");

                }
                
            }
          



        }
    }
}
