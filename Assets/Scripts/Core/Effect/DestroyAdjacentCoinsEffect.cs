// DestroyAdjacentCoinsEffect.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/Effects/Destroy Adjacent Coins")]
public class DestroyAdjacentCoinsEffect : EffectSO
{
    [Header("Target")]
    public string TargetTag = "Coin";
    public bool Use8Neighbors = true;

    [Header("Reward")]
    [Min(1)] public int CoinPerDestroyed = 10;

    [TextArea] public string desc;

    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        var self = board.Grid[ctx.Pos.x, ctx.Pos.y];
        if (self == null) return;

        var neigh = board.Neighbors8(ctx.Pos); // 8近傍
        var targets = new List<Vector2Int>();
        foreach (var p in neigh)
        {
            var t = board.Grid[p.x, p.y];
            if (t == null) continue;
            if (t.Tags != null && t.Tags.Contains(TargetTag))
                targets.Add(p);
        }
        if (targets.Count == 0) return;

        score.EmitAnim(AnimEvent.Bounce(ctx.Pos, 0.15f));
        foreach (var p in targets)
        {
            score.EmitAnim(AnimEvent.Shatter(p, 0.18f)); // ← MarkDestroy とセットで積むのが安全
            score.MarkDestroy(p, new DestroyMeta { Cause = "DestroyAdjacentCoins", Source = self, Victim = board.Grid[p.x, p.y] });
        }

        int bonus = targets.Count * 10;
        score.AddBonusCoins(bonus);      // ← 合計だけはここで積み
        score.AddCellGain(ctx.Pos, bonus);  // 見た目用のポップは従来通り
    }
}
