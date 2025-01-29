using System;
using System.Threading;

public class MainThreadRunner : Singleton<MainThreadRunner> {
    private static SynchronizationContext mainContext;

    protected override void Awake() {
        base.Awake();
        mainContext = SynchronizationContext.Current;
    }

    public static void Run(Action action) 
        => mainContext.Post(_ => action?.Invoke(), null);
}
