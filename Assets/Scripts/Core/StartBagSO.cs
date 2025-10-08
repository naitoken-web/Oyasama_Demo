// StartBagSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Lucklike/StartBag")]
public class StartBagSO : ScriptableObject
{
    // 例：Coin×4, Cherry×1 など、最初に入れておきたい実体を列挙
    public List<SymbolSO> Items = new();
    // 空白マスの“重み”（大きいほど空白が出やすい）
    [Min(0)] public float EmptyWeight = 0f;

    /// <summary>最初に一致した1個を削除して true。無ければ false。</summary>
    public bool RemoveOne(SymbolSO s)
    {
        if (s == null) return false;
        return Items.Remove(s);
    }
}
