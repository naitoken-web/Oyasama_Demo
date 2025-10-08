// DraftSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/DraftSystem")]
public class DraftSystem : ScriptableObject
{
    [System.NonSerialized] public RunState CurrentRun; // ← 追加（GameControllerが毎回セット）
    public SymbolCatalogSO symbolCatalog;
    public RelicCatalogSO relicCatalog;

    public virtual List<SymbolSO> GenerateSymbolOptions(System.Random rng, int count = 3)
    {
        var pool = symbolCatalog.BuildWeightedPool();
        var options = new List<SymbolSO>(count);
        for (int i = 0; i < count; i++) options.Add(pool[rng.Next(pool.Count)]);
        return options;
    }

    public virtual List<RelicSO> GenerateRelicOptions(System.Random rng, int count = 3)
    {
        var pool = relicCatalog.BuildWeightedPool();
        var options = new List<RelicSO>(count);
        for (int i = 0; i < count; i++) options.Add(pool[rng.Next(pool.Count)]);
        return options;
    }
}
