using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace TTTools.client
{
    public static class ClientManager
    {
        // 全局 Client 列表
        public static List<Client> Clients { get; private set; } = new List<Client>();

        // 当前选中的 Client
        public static Client CurrentSelectedClient { get; set; }

        // 任务映射，存储不同 Action 的任务
        private static Dictionary<string, Func<Client, Task>> actionMap = new Dictionary<string, Func<Client, Task>>();

        // 初始化任务映射
        public static void InitializeActionMap()
        {
            actionMap["hello"] = async (client) =>
            {
              
                LogService.Debug($"客户端 {client.Title} 执行任务: {client.Action}");
                await Task.Delay(2000); // 模拟任务执行
                //client.Action = null; // 完成任务后清空
            };
            actionMap["采集矿石"] = async (client) =>
            {
                LogService.Debug($"客户端 {client.Title} 执行任务: {client.Action}");
                var hWnd = client.HWnd;
                var api = new Method(hWnd);
                var point = api.FindAndClickSomeThingInMap("矿石", true);
                if (point == null)
                {
                    LogService.Debug("没有找到矿石");

                }
            };
            actionMap["采集水晶"] = async (client) =>
            {
                LogService.Debug($"客户端 {client.Title} 执行任务: {client.Action}");
                var hWnd = client.HWnd;
                var api = new Method(hWnd);
                var point = api.FindAndClickSomeThingInMap("水晶", true);
                if (point == null)
                {
                    LogService.Debug("没有找到矿石");

                }
            };
            actionMap["采集皮草"] = async (client) =>
            {
                LogService.Debug($"客户端 {client.Title} 执行任务: {client.Action}");
                var hWnd = client.HWnd;
                var api = new Method(hWnd);
                var point = api.FindAndClickSomeThingInMap("皮草", true);
                if (point == null)
                {
                    LogService.Debug("没有找到矿石");

                }
            };
            actionMap["采集矿石"] = async (client) =>
            {
                LogService.Debug($"客户端 {client.Title} 执行任务: {client.Action}");
                var hWnd = client.HWnd;
                var api = new Method(hWnd);
                var point = api.FindAndClickSomeThingInMap("矿石", true);
                if (point == null)
                {
                    LogService.Debug("没有找到矿石");

                }
            };
        }

        // 添加新的任务到映射
        public static void AddTaskToActionMap(string actionName, Func<Client, Task> taskAction)
        {
            actionMap[actionName] = taskAction;
        }

        // 获取任务映射
        public static Func<Client, Task> GetTask(string actionName)
        {
            return actionMap.TryGetValue(actionName, out var task) ? task : null;
        }

        // 清除所有客户端任务
        public static void ClearAllTasks()
        {
            foreach (var client in Clients)
            {
                client.ClearTask();
            }
        }
    }

    public class Client
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        public IntPtr HWnd { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Action { get; set; }
        public bool IsLead { get; set; }
        public bool IsActionRun { get; set; }
        public bool IsActionAuto { get; set; }//是否循环执行


        private CancellationTokenSource _cancellationTokenSource;
        private Task _monitorTask;

        public Client(IntPtr hWnd, string id, string title, bool isLead)
        {
            Id = id;
            Title = title;
            IsLead = isLead;
            HWnd = hWnd;
        }

        // 启动客户端的任务监控
        public void StartMonitoring()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _monitorTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    //LogService.Debug($"title=${Title},action={Action}");
                    // 如果没有正在执行的任务，并且有指定的 Action
                    if (!IsActionRun && !string.IsNullOrEmpty(Action))
                    {
                        // 获取对应的任务
                        var taskAction = ClientManager.GetTask(Action);

                        // 如果找到任务，则执行
                        if (taskAction != null)
                        {
                            LogService.Debug($"title=${Title},action={Action}- 开始执行");

                            UpdateTitle();
                            IsActionRun = true;  // 标记为正在执行
                            await taskAction(this);
                            IsActionRun = false; // 完成后重置状态
                        }
                        else
                        {
                            ClearTask(); // 如果任务不存在则清空 Action
                        }
                    }

                    // 延迟一段时间后再次检查 Action 状态
                    await Task.Delay(1000); // 你可以根据需求调整此值
                }
            }, cancellationToken);
        }


        // 停止任务监控
        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            _monitorTask?.Wait();
            _cancellationTokenSource?.Dispose();
            IsActionRun = false;
            Action = "";
            UpdateTitle();
        }

        // 清空任务
        public void ClearTask()
        {
            Action = null;
            UpdateTitle();
            LogService.Log($"客户端 {Title} 任务已清空");
        }

        // 更新窗口标题
        public void UpdateTitle()
        {
            //string newTitle = "";
            //if (Action != "")
            //{
            //    newTitle = $"{Title} - {Action}";

            //}
            //else
            //{
            //    newTitle = $"{Title}";
            //}

            //SetWindowText(HWnd, newTitle);
        }
    }
}
