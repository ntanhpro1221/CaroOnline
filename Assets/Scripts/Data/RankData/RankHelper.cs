using System;
using System.Linq;
using UnityEngine;

public static class RankHelper {
    public static RankData GetRankOfElo(int elo, PropertySet<RankType, RankDataConfig> configRank, RankDataSO rankData) {
        var sortedConfigRank = configRank.OrderBy(ele => ele.Value.LowerBound).ToList();
        return rankData[Enum.Parse<RankType>(
            (elo < sortedConfigRank[0].Value.LowerBound
            ? sortedConfigRank[0]
            : sortedConfigRank.Where(keyVal => elo >= keyVal.Value.LowerBound).Last()).Key)];
    }
}
