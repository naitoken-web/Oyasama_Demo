// DraftSystem.cs の置き換え（被り無し版）
using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

[CreateAssetMenu(menuName = "Lucklike/DraftSystem (Unique)")]
public class DraftSystemUnique : DraftSystem
{

    public override List<SymbolSO> GenerateSymbolOptions(System.Random rng, int count = 3)
    {
        var pool = symbolCatalog.BuildWeightedPool();
        // ★ レアリティ補正（Symbols 対象）
        pool = ApplyRarityBoost(pool, DraftTarget.Symbols, s => s.Rarity);

        var options = new List<SymbolSO>(count);
        while (options.Count < count && pool.Count > 0)
        {
            int idx = rng.Next(pool.Count);
            var pick = pool[idx];
            options.Add(pick);
            pool.RemoveAll(p => p == pick); // 被り無し
        }
        return options;
    }

    public override List<RelicSO> GenerateRelicOptions(System.Random rng, int count = 3)
    {
        var pool = relicCatalog.BuildWeightedPool();
        // ★ レアリティ補正（Relics 対象）
        pool = ApplyRarityBoost(pool, DraftTarget.Relics, r => r.Rarity);

        var options = new List<RelicSO>(count);
        while (options.Count < count && pool.Count > 0)
        {
            int idx = rng.Next(pool.Count);
            var pick = pool[idx];
            options.Add(pick);
            pool.RemoveAll(p => p == pick); // 被り無し
        }
        return options;
    }

    // DraftSystemUnique.cs（追記）
    // DraftSystemUnique.cs（追記/流用）
    List<T> ApplyRarityBoost<T>(
        List<T> basePool,
        DraftTarget target,
        System.Func<T, Rarity> getRarity)
    {
        var run = CurrentRun;
        if (run == null || run.ActiveRarityBoosts == null || run.ActiveRarityBoosts.Count == 0)
            return basePool;

        var counts = new Dictionary<T, int>();
        foreach (var t in basePool)
            counts[t] = counts.TryGetValue(t, out var c) ? c + 1 : 1;

        float MultFor(Rarity r)
        {
            float mult = 1f;
            foreach (var b in run.ActiveRarityBoosts)
            {
                if (!(b.AppliesTo == DraftTarget.Both || b.AppliesTo == target)) continue;
                if (r >= b.MinRarity) mult *= (1f + b.BonusPercent * 0.01f); // 乗算でスタック
            }
            return mult;
        }

        var boosted = new List<T>(basePool.Count);
        foreach (var kv in counts)
        {
            var item = kv.Key;
            int baseCount = kv.Value;
            int boostedCount = Mathf.Max(1, Mathf.RoundToInt(baseCount * MultFor(getRarity(item))));
            for (int i = 0; i < boostedCount; i++) boosted.Add(item);
        }
        return boosted;
    }
}
