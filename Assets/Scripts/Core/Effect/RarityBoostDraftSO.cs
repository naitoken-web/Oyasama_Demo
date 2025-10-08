// RarityBoostDraftEffect.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/Effects/Rarity Boost (Draft)")]
public class RarityBoostDraftEffect : EffectSO
{
    [Header("適用条件")]
    public Rarity MinRarity = Rarity.Rare;     // このレア以上
    [Range(0, 500)] public float BonusPercent = 25f; // +X%
    public DraftTarget AppliesTo = DraftTarget.Both;

    [TextArea] public string Description; // 任意：UI用

    // ★ スピン解決中に“そのシンボルが存在する限り”ブーストを積む
    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        score.AddDraftRarityBoost(this);
        // ※演出不要。常時効果なのでアニメは積まない
    }
}
