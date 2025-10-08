// ConsumableCatalogSO.cs（タイプ→SO を引くためのカタログ）
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/Consumables/Catalog")]
public class ConsumableCatalogSO : ScriptableObject
{
    public List<ConsumableSO> Items = new();

    Dictionary<ConsumableType, ConsumableSO> _map;
    void OnEnable()
    {
        _map = new Dictionary<ConsumableType, ConsumableSO>();
        foreach (var it in Items) if (it) _map[it.Type] = it;
    }
    public ConsumableSO Get(ConsumableType t) => _map != null && _map.TryGetValue(t, out var so) ? so : null;
}
