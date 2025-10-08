using UnityEngine;
using System.Collections.Generic;
public class RelicCatalogSO : ScriptableObject
{
    public List<RelicSO> Commons = new();
    public List<RelicSO> Uncommons = new();
    public List<RelicSO> Rares = new();
    public List<RelicSO> Epics = new();

    // 単純化：レア度の重みでリストを結合
    public List<RelicSO> BuildWeightedPool(float wCommon = 1f, float wUnc = 0.5f, float wRare = 0.25f, float wEpic = 0.1f)
    {
        var list = new List<RelicSO>();
        void add(List<RelicSO> src, float w)
        {
            foreach (var s in src) { var rep = Mathf.Max(1, Mathf.RoundToInt(s.Weight * w * 10)); for (int i = 0; i < rep; i++) list.Add(s); }
        }
        add(Commons, wCommon); add(Uncommons, wUnc); add(Rares, wRare); add(Epics, wEpic);
        return list;
    }
}