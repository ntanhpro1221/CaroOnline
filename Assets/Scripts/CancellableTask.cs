using System.Threading.Tasks;
using System;
using System.Threading;

public class CancellableTask {
    private CancellationTokenSource _TokenSource;
    public Task CurTask { get; private set; }
    
    public CancellableTask(Func<CancellationToken, Task> task) {
        Start(task);
    }
    
    private async void Start(Func<CancellationToken, Task> task) {
        _TokenSource = new();
        CurTask = task.Invoke(_TokenSource.Token);
        await CurTask;
        _TokenSource.Dispose();
        _TokenSource = null;
    }

    public void Cancel() {
        _TokenSource?.Cancel();
    }
}
