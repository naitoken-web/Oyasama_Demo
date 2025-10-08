// DraftCardStyleSO.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Lucklike/UI/DraftCardStyle")]
public class DraftCardStyleSO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public Rarity Rarity;
        public Sprite Background;      // ★ 背景スプライト（なければ単色）
        public Color FallbackColor = Color.white;
        public Color TitleColor = Color.white;   // タイトル文字色（任意）
        public Color DescColor = Color.white;   // 説明文字色（任意）
    }
    public List<Entry> Styles = new();

    Dictionary<Rarity, Entry> map;
    void OnEnable()
    {
        map = new Dictionary<Rarity, Entry>();
        foreach (var e in Styles) if (e != null) map[e.Rarity] = e;
    }
    public Entry Get(Rarity r) => map != null && map.TryGetValue(r, out var e) ? e : null;
}
