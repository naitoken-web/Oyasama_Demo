// UIRelicBar.cs（SymbolView を使う版）
using UnityEngine;
using System.Collections.Generic;

public class UIRelicBar : MonoBehaviour
{
    [SerializeField] RectTransform root;
    [SerializeField] SymbolView iconPrefab;   // ← Image から差し替え

    readonly List<SymbolView> _icons = new();

    public void Refresh(IReadOnlyList<RelicSO> relics)
    {
        // 既存クリア
        for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
        _icons.Clear();

        // 所持レリックを並べる（サブテキストは空でOK）
        foreach (var r in relics)
        {
            var view = Instantiate(iconPrefab, root);
            view.Bind(r != null ? r.Icon : null, "");  // SymbolViewのBindをそのまま再利用
            _icons.Add(view);
        }
    }
}
