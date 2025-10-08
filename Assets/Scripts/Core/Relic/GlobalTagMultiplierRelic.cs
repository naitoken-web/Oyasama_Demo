// GlobalTagMultiplierRelic.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/Relic/GlobalTagMultiplier")]
public class GlobalTagMultiplierRelic : RelicSO
{
    public string TargetTag = "Coin";
    public int Multiplier = 2;
    public override int ModifyFinalScore(int total)
    {
        // 本来はScoreBuilderのタグ内訳を見て掛けるのが厳密だが、
        // MVPでは最終倍率で簡易に調整してもOK。将来的にScoreBuilderへフック。
        return total; // まずはダミー。実装を進めるならScoreResolverにRunStateを渡してタグ加点へ反映。
    }
}
