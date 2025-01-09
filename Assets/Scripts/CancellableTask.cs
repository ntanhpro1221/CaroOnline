using System.Threading.Tasks;
using System;
using System.Threading;

public class CancellableTask {
    private CancellationTokenSource _TokenSource;
    
    public CancellableTask(Func<CancellationToken, Task> task) {
        Start(task);
    }
    
    private async void Start(Func<CancellationToken, Task> task) {
        _TokenSource = new();
        await task.Invoke(_TokenSource.Token);
        _TokenSource.Dispose();
        _TokenSource = null;
    }

    public void Cancel() {
        _TokenSource?.Cancel();
    }
}
