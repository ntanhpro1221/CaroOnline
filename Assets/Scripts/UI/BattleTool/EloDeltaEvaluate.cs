using System;

public static class EloDeltaEvaluate {
    /// <summary>
    /// point gain when two player have same elo
    /// </summary>
    private const double midPoint = 15;
    /// <summary>
    /// The largest difference in points compared to <see cref="midPoint"/> when 2 players are not equal in points
    /// </summary>
    private const double maxDifPoint = 10;
    /// <summary>
    /// how slowly to reach <see cref="maxDifPoint"/>
    /// </summary>
    private const double reachDifPointSlow = 100;
    
    /// <summary>
    ///  atan(x / v) * 2 <br/>
    /// ----------------- * d + m <br/>
    ///        PI
    /// </summary>
    private static double PointCalcFunction(double delElo) 
        => Math.Atan(delElo / reachDifPointSlow) * 2 / Math.PI * maxDifPoint + midPoint;

    public static int GetDeltaElo(int myElo, int opponentElo, bool isWin)
        => (int)Math.Round(PointCalcFunction((isWin ? 1 : -1) * (opponentElo - myElo)) * (isWin ? 1 : -1));
}
