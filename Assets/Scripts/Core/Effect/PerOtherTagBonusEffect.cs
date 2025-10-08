// PerOtherTagBonusEffect.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Lucklike/Effects/Per Other Tag Bonus")]
public class PerOtherTagBonusEffect : EffectSO
{
    [Header("Condition")]
    [Tooltip("数える対象のタグ名（例: \"Coin\", \"Cat\" など）")]
    public string TargetTag = "Coin";  // TagX

    [Header("Reward")]
    [Min(1)] public int CoinsPerMatch = 2;      // Y コイン
    public bool BounceOnTrigger = true;         // 発動時に跳ねる

    [TextArea] public string Description;       // UI用（任意）

    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        var self = board.Grid[ctx.Pos.x, ctx.Pos.y];
        if (self == null) return;

        // 全盤面から「自分以外」で TargetTag を持つものをカウント
        int w = board.Grid.GetLength(0);
        int h = board.Grid.GetLength(1);

        int count = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x == ctx.Pos.x && y == ctx.Pos.y) continue; // 自分は除外
                var s = board.Grid[x, y];
                if (s == null || s.Tags == null) continue;
                if (s.Tags.Contains(TargetTag)) count++;
            }
        }

        if (count <= 0) return;

        if (BounceOnTrigger)
            score.EmitAnim(AnimEvent.Bounce(ctx.Pos, 0.15f)); // 発動元を跳ねる

        int bonus = count * CoinsPerMatch;

        // スコアログ（任意）
        score.Add(bonus, $"{self.Id}: {TargetTag} x{count} → +{bonus}");

        // 発動元セル位置でポップ表示（加算はコミット側で CellGains 合算）
        score.AddCellGain(ctx.Pos, bonus);
    }
}
