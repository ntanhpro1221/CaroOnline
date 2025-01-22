using System;
using System.Threading.Tasks;

public class WaitHelper {
    public static async Task WaitFor(Func<bool> predict) {
        while (!predict()) await Task.Delay(33); 
    }
}