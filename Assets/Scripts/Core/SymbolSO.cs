// SymbolSO.cs（Step1から拡張）
using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Lucklike/Symbol")]
public class SymbolSO : ScriptableObject
{
    public string Id;
    public Sprite Icon;
    public int BaseScore = 1;
    public List<string> Tags = new();
    public float Weight = 1f;
    public List<EffectSO> Effects = new(); // ★追加：効果の組み合わせ
    public Rarity Rarity = Rarity.Common; // 既存アセットはデフォルトでCommon
    [TextArea] public string Description;   // ★ 説明テキスト
}