// AddFlatPointsEffect.cs
using UnityEngine;
[CreateAssetMenu(menuName = "Lucklike/Effects/AddFlat")]
public class AddFlatPointsEffect : EffectSO
{
    public int Points = 1;

    [TextArea] public string desc;

    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        score.Add(Points, $"{ctx.Self.Id} base");
    }
}