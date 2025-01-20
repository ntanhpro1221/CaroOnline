using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public static class ConnectionChecker {
    public static bool CachedInternetCheckResult { get; private set; } = false;

    public static async Task<bool> CheckConnection(float timeout = 10000) {
        Ping ping = new("8.8.8.8");
        Stopwatch stopwatch = new();
        stopwatch.Start();

        while (!ping.isDone) {
            if (stopwatch.ElapsedMilliseconds > timeout) return CachedInternetCheckResult = false;
            await Task.Delay(100);
        }
        
        return CachedInternetCheckResult = ping.time >= 0;
    } 
}