// UIInventoryView.cs（差し替え/拡張）
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryView : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] RectTransform bagGrid;        // ← UIItemCountEntry を並べる
    [SerializeField] RectTransform relicGrid;      // ← 同上
    [SerializeField] UIItemCountEntry itemCountPrefab;  // ★ 新プレハブ

    [Header("Consumables")]
    [SerializeField] RectTransform consumableGrid;
    [SerializeField] UIConsumableEntry consumablePrefab;
    [SerializeField] ConsumableCatalogSO consumableCatalog;   // ★ 追加：カタログ
    // 表示中の各エントリを保持（後でカウント更新しやすい）
    Dictionary<ConsumableType, UIConsumableEntry> consumableEntries = new();

    [SerializeField] Button closeButton;

    bool removeMode = false;
    Action<SymbolSO> onRemoveSymbol;
    Action<RelicSO> onRemoveRelic;

    // 内部キャッシュ（表示中カウント）
    Dictionary<SymbolSO, int> bagCounts = new();
    Dictionary<RelicSO, int> relicCounts = new();
    // 表示中エントリへの逆引き
    Dictionary<SymbolSO, UIItemCountEntry> bagEntries = new();
    Dictionary<RelicSO, UIItemCountEntry> relicEntries = new();

    void Awake()
    {
        root.SetActive(false);
        closeButton.onClick.AddListener(() => Close());
    }

    public void Open(
        RunState run,
        bool removeMode,
        Action<SymbolSO> onRemoveSymbol,
        Action<RelicSO> onRemoveRelic,
        Action<ConsumableType> onClickConsumable = null
    )
    {
        this.removeMode = removeMode;
        this.onRemoveSymbol = onRemoveSymbol;
        this.onRemoveRelic = onRemoveRelic;

        root.SetActive(true);

        RebuildBagGrouped(run.Bag);
        RebuildRelicsGrouped(run.Relics);
        RebuildConsumables(run.Consumables, onClickConsumable);
    }

    public void Close() => root.SetActive(false);

    void ClearChildren(Transform t) { for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject); }

    // ===== Bag（シンボル） =====
    // BagItemはstructなのでnull比較不可。Symbolプロパティで判定
    void RebuildBagGrouped(Bag bag)
    {
        bagCounts.Clear(); bagEntries.Clear(); ClearChildren(bagGrid);
        if (bag?.Items == null) return;

        foreach (var s in bag.Items)
        {
            if (s.Symbol == null) continue; // BagItem自体はstructなのでnull不可、SymbolSOでnull判定
            bagCounts.TryGetValue(s.Symbol, out int c); bagCounts[s.Symbol] = c + 1;
        }

        foreach (var kv in bagCounts)
        {
            var entry = Instantiate(itemCountPrefab, bagGrid);
            entry.Bind(
                kv.Key.Icon,
                kv.Key.Id,
                kv.Value,
                payload: kv.Key,
                onClick: (obj) => {
                    if (!removeMode) return;
                    var sym = obj as SymbolSO;
                    if (sym == null) return;
                    onRemoveSymbol?.Invoke(sym);  // GameController側が Bag から1枚 Remove する
                },
                interactable: removeMode // 除去モードのときだけ押せる
            );
            bagEntries[kv.Key] = entry;
        }
    }

    // Bag UIの“1枚だけ減らす”反映（GameControllerから呼べるユーティリティ）
    public void OnBagOneRemoved(SymbolSO sym)
    {
        if (sym == null) return;
        if (!bagCounts.TryGetValue(sym, out var c)) return;
        c -= 1;
        if (c <= 0)
        {
            bagCounts.Remove(sym);
            if (bagEntries.TryGetValue(sym, out var e))
            {
                Destroy(e.gameObject);
                bagEntries.Remove(sym);
            }
        }
        else
        {
            bagCounts[sym] = c;
            if (bagEntries.TryGetValue(sym, out var e)) e.SetCount(c);
        }
    }

    // ===== Relics =====
    void RebuildRelicsGrouped(List<RelicSO> relics)
    {
        relicCounts.Clear(); relicEntries.Clear(); ClearChildren(relicGrid);
        if (relics == null) return;

        foreach (var r in relics)
        {
            if (r == null) continue;
            relicCounts.TryGetValue(r, out int c); relicCounts[r] = c + 1;
        }

        foreach (var kv in relicCounts)
        {
            var entry = Instantiate(itemCountPrefab, relicGrid);
            entry.Bind(
                kv.Key.Icon,
                kv.Key.Id,
                kv.Value,
                payload: kv.Key,
                onClick: (obj) => {
                    if (!removeMode) return;
                    var rel = obj as RelicSO;
                    if (rel == null) return;
                    onRemoveRelic?.Invoke(rel);   // GameController側が Relics から1つ Remove
                },
                interactable: removeMode
            );
            relicEntries[kv.Key] = entry;
        }
    }

    public void OnRelicOneRemoved(RelicSO relic)
    {
        if (relic == null) return;
        if (!relicCounts.TryGetValue(relic, out var c)) return;
        c -= 1;
        if (c <= 0)
        {
            relicCounts.Remove(relic);
            if (relicEntries.TryGetValue(relic, out var e))
            {
                Destroy(e.gameObject);
                relicEntries.Remove(relic);
            }
        }
        else
        {
            relicCounts[relic] = c;
            if (relicEntries.TryGetValue(relic, out var e)) e.SetCount(c);
        }
    }

    // ===== Consumables は従来通り =====
    void RebuildConsumables(ConsumableCounts c, Action<ConsumableType> onClick)
    {
        ClearChildren(consumableGrid);
        consumableEntries.Clear();
        if (consumableCatalog == null) return;

        void add(ConsumableType t, int count)
        {
            var so = consumableCatalog.Get(t);
            if (so == null) return; // カタログ未設定
            var e = Instantiate(consumablePrefab, consumableGrid);
            e.Bind(so, count, (clickedSO) => onClick?.Invoke(clickedSO.Type));
            consumableEntries[t] = e;
        }

        add(ConsumableType.RemoveToken, c.RemoveToken);
        add(ConsumableType.RerollItem, c.RerollItem);
        add(ConsumableType.RerollRelic, c.RerollRelic);
    }

    // 使ったあとにUIだけ更新したい場合のヘルパ
    public void SetConsumableCount(ConsumableType t, int count)
    {
        if (consumableEntries.TryGetValue(t, out var e)) e.SetCount(count);
    }
}
