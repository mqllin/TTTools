using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTTools
{
    class ActionCollectMethod
    {

        private int nullCounter = 0;  // 用于跟踪连续返回 null 的次数
        private int lastMapMoveIndex = 0;  //记录上一次点击地图的位置
        private readonly Method api;
        private readonly WindowOperations wx;
        private readonly Form1 Instance;
        private PictureMethod Pic;
        private readonly IntPtr hWnd;
        private bool IsRuning;

        public ActionCollectMethod(IntPtr hWnd, Form1 Instance)
        {
            this.hWnd = hWnd;
            this.wx = new WindowOperations(hWnd, Instance);
            this.api = new Method(hWnd, Instance);
            this.Instance = Instance;
            this.Pic = new PictureMethod(hWnd);
            nullCounter = 0;
            lastMapMoveIndex = 0;
            IsRuning = true;
        }
        public void clear() {
            IsRuning = false;
        }

     

        public async void StartActionCollet()
        {

            await Task.Run(() =>
            {
                //while (IsRuning)
                //{
                //    var name = "矿石";
                //    var point = api.FindSomeThingInMap(name);
                //    if (point != null)
                //    {
                //        nullCounter = 0;  // 重置计数器
                //        Thread.Sleep(1000);
                //        StartActionCollet();
                //    }
                //    else
                //    {
                //        nullCounter++;  // 递增计数器
                //        if (nullCounter >= 3)
                //        {
                //            nullCounter = 0;  // 重置计数器
                //            lastMapMoveIndex++;
                //            if (lastMapMoveIndex > api.CalculateGridIntersections(name).Count)
                //            {
                //                lastMapMoveIndex = 0;
                //            }
                //            HandActionColletMove(name, lastMapMoveIndex);

                //        }
                //    }
                //    Thread.Sleep(2000);  // 暂停500毫秒再次检查
                //}
            });

        }
        public async void HandActionColletMove(string name, int lastMapMoveIndex)
        {
            api.MoveCollectionLocation(name, lastMapMoveIndex);  // 调用 move 方法
            PictureMethod pic = new PictureMethod(hWnd);

            await Task.Run(() =>
            {
                while (true)
                {
                    if (!pic.CheckIsMoving())
                    {
                        break;  // 如果 CheckIsMoving 返回 false，跳出循环
                    }
                    Thread.Sleep(500);  // 暂停500毫秒再次检查
                }
            });

            StartActionCollet();  // 在移动结束后重新开始动作收集
        }
    }
}
