// Bag.cs（最小）
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Lucklike/Run/Bag")]
public class Bag : ScriptableObject
{
    [SerializeField] int nextUid = 1;

    public List<BagItem> Items = new(); // 旧: List<SymbolSO>

    public int Add(SymbolSO s)
    {
        if (s == null) return 0;
        var id = nextUid++;
        Items.Add(new BagItem { Uid = id, Symbol = s });
        return id;
    }

    public bool RemoveOne(SymbolSO s)
    {
        var idx = Items.FindIndex(it => it.Symbol == s);
        if (idx >= 0) { Items.RemoveAt(idx); return true; }
        return false;
    }

    public bool RemoveOneByUid(int uid)
    {
        var idx = Items.FindIndex(it => it.Uid == uid);
        if (idx >= 0) { Items.RemoveAt(idx); return true; }
        return false;
    }

    // 旧コードとの橋渡し（“中身だけ”必要な場面向け）
    public IEnumerable<SymbolSO> EnumerateSymbols()
    {
        foreach (var it in Items) yield return it.Symbol;
    }
}
