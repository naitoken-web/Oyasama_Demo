using UnityEngine;

// AdjacentTagBonusEffect.csÅió◊ê⁄É^ÉOÅ~î{ó¶Åj
[CreateAssetMenu(menuName = "Lucklike/Effects/AdjTagBonus")]
public class AdjacentTagBonusEffect : EffectSO
{
    public string TargetTag = "Reimu";
    public int PerNeighbor = 1;
    public bool Use8Neighbors = true;

    [TextArea] public string desc;

    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        var neigh = Use8Neighbors ? board.Neighbors8(ctx.Pos) : board.Neighbors4(ctx.Pos);
        int count = 0;
        foreach (var p in neigh)
        {
            var s = board.Grid[p.x, p.y];
            if (s != null && s.Tags.Contains(TargetTag)) count++;
        }
        if (count > 0) score.Add(count * PerNeighbor, $"{ctx.Self.Id} adj {TargetTag}Å~{count}");
    }
}