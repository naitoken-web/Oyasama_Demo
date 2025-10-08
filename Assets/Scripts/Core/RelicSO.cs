// RelicSO.cs
using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

public abstract class RelicSO : ScriptableObject
{
    public string Id;
    public Sprite Icon;
    public Rarity Rarity;
    public float Weight = 1f;
    [TextArea] public string Description;   // ★ 説明テキスト

    // 最終スコアに掛け算するタイプなどの例
    public virtual int ModifyFinalScore(int total) => total;

    // 取得時にRunStateを更新したい場合（タグ倍率付与など）
    public virtual void OnAcquire(RunState run) { }
}
