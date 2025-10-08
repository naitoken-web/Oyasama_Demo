// TagCountBonusEffect.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/Effects/Tag Count Bonus")]
public class TagCountBonusEffect : EffectSO
{
    [Header("Condition")]
    [Tooltip("これらのいずれかのタグを持つシンボルを数えます")]
    public List<string> TargetTags = new() { "Coin" };  // 例：{"Cat","Dog"} など
    [Min(1)] public int ThresholdCount = 3;             // X：これ以上あれば発動

    [Header("Reward")]
    [Min(1)] public int RewardCoins = 10;               // Y：加算コイン

    [TextArea] public string Description;               // 任意：UI用

    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        // 発動元が生きている？
        var self = board.Grid[ctx.Pos.x, ctx.Pos.y];
        if (self == null) return;

        // 全盤面を走査して、TargetTags のいずれかを持つシンボルをカウント
        int W = board.Grid.GetLength(0), H = board.Grid.GetLength(1);
        int count = 0;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var s = board.Grid[x, y];
                if (s == null || s.Tags == null) continue;
                // いずれか一致でOK（タグ大小区別は現実装に合わせて、必要なら ToLower で統一）
                for (int i = 0; i < TargetTags.Count; i++)
                {
                    var tag = TargetTags[i];
                    if (string.IsNullOrEmpty(tag)) continue;
                    if (s.Tags.Contains(tag)) { count++; break; }
                }
            }

        if (count < ThresholdCount) return; // 条件未達 → 何もしない

        // 発動アニメ（発動元を跳ねる）
        score.EmitAnim(AnimEvent.Bounce(ctx.Pos, 0.15f));

        // ボーナス（RunStateへの加算は Commit 側で行う想定）
        score.Add(RewardCoins, $"{self.Id}: {count} tags ≥ {ThresholdCount} → +{RewardCoins}");
        score.AddCellGain(ctx.Pos, RewardCoins); // 発動元位置にポップ
    }
}
