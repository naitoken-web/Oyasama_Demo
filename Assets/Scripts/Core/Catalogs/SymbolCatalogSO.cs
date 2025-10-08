// SymbolCatalogSO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/SymbolCatalog")]
public class SymbolCatalogSO : ScriptableObject
{
    public List<SymbolSO> Commons = new();
    public List<SymbolSO> Uncommons = new();
    public List<SymbolSO> Rares = new();
    public List<SymbolSO> Epics = new();

    // 単純化：レア度の重みでリストを結合
    public List<SymbolSO> BuildWeightedPool(float wCommon = 1f, float wUnc = 0.5f, float wRare = 0.25f, float wEpic = 0.1f)
    {
        var list = new List<SymbolSO>();
        void add(List<SymbolSO> src, float w)
        {
            foreach (var s in src) { var rep = Mathf.Max(1, Mathf.RoundToInt(s.Weight * w * 10)); for (int i = 0; i < rep; i++) list.Add(s); }
        }
        add(Commons, wCommon); add(Uncommons, wUnc); add(Rares, wRare); add(Epics, wEpic);
        return list;
    }
}
