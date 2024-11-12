using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TTTools.windowsTools;
using static System.Net.Mime.MediaTypeNames;

namespace TTTools
{
    public abstract class BaseOperation
    {
        protected readonly IntPtr hWnd;
        protected readonly CancellationTokenSource cancellationTokenSource;
        protected readonly CancellationToken cancellationToken;
        WindowOperationsAsync windowOperationsAsync;
        public BaseOperation(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            windowOperationsAsync = new WindowOperationsAsync(hWnd);
        }

        // 定义一个模板方法
        public async Task ExecuteAsync(Func<Task> operationTask)
        {
            try
            {
              
                await operationTask();  // 执行操作
            }
            catch (OperationCanceledException)
            {
                LogService.Debug("操作已取消。");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        // 允许外部取消操作
        public void CancelOperation()
        {
            cancellationTokenSource.Cancel();
        }

        public async Task ClickAsync(int x, int y)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await Task.Delay(500, cancellationToken);  // 模拟点击的延迟
            await windowOperationsAsync.PushClickAsync(x, y);
            LogService.Debug($"点击窗口：[{x},{y}]" );

        }
        public async Task EnterTextAsync(string text)
        {
            foreach (var ch in text)
            {
                if (cancellationToken.IsCancellationRequested) return;
                await windowOperationsAsync.SendKeyAsync(ch);
                await Task.Delay(100, cancellationToken);  // 模拟打字延迟
            }
            await windowOperationsAsync.SendEnterKeyAsync();
            LogService.Debug("发言："+ text);
        }
    }
}
