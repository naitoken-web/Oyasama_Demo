// ScorePlumbing.cs（新規 or 既存ScoreBuilderの横に）
using System.Collections.Generic;
using UnityEngine;

public enum AnimEventType { Bounce, Shatter }

public struct AnimEvent
{
    public AnimEventType Type;
    public Vector2Int Pos;
    public float Duration;

    public static AnimEvent Bounce(Vector2Int pos, float dur) => new AnimEvent { Type = AnimEventType.Bounce, Pos = pos, Duration = dur };
    public static AnimEvent Shatter(Vector2Int pos, float dur) => new AnimEvent { Type = AnimEventType.Shatter, Pos = pos, Duration = dur };
}

public class DestroyMeta
{
    public string Cause;   // 例: "DestroyAdjacentCoins"
    public SymbolSO Source;  // 発動元
    public SymbolSO Victim;  // 破壊される側
}

public class DestroyRequest
{
    public Vector2Int Pos;
    public DestroyMeta Meta;
}

public class SpawnRequest
{
    public Vector2Int Pos;
    public SymbolSO Def;
    public string Reason;
}

/// <summary>
/// 既存の ScoreBuilder に“積むだけ”の拡張を追加
/// </summary>
public partial class ScoreBuilder
{
    public readonly List<AnimEvent> AnimEvents = new();
    public readonly List<DestroyRequest> DestroyQueue = new();
    public readonly List<(Vector2Int pos, int amount)> CellGains = new();
    private readonly HashSet<Vector2Int> destroyMarked = new();
    private readonly HashSet<Vector2Int> shatterMarked = new();

    public int BonusCoins;                       // 効果による追加コインの合算

    public readonly List<RarityBoostDraftEffect> DraftBoosts = new();

    public readonly List<SpawnRequest> SpawnQueue = new();

    /// <summary>Effect側から「破壊予定か？」を判定するための公開アクセサ</summary>
    public bool IsMarkedForDestroy(Vector2Int pos) => destroyMarked.Contains(pos);

    /// <summary>生成リクエストを積む（Commit側で実際に盤面へ反映）</summary>
    public void EnqueueSpawn(SymbolSO def, Vector2Int pos, string reason = "")
    {
        if (def == null) return;
        SpawnQueue.Add(new SpawnRequest { Def = def, Pos = pos, Reason = reason });
    }

    public void AddDraftRarityBoost(RarityBoostDraftEffect b)
    {
        if (b != null) DraftBoosts.Add(b); // スタック可（同じ効果が複数あれば累積）
    }

    public void AddBonusCoins(int v) => BonusCoins += v;

    public void EmitAnim(AnimEvent ev)
    {
        // Shatter だけ座標でデデュープ。Bounceはそのまま（発動元が複数ある可能性）
        if (ev.Type == AnimEventType.Shatter)
        {
            if (!shatterMarked.Add(ev.Pos)) return;
        }
        AnimEvents.Add(ev);
    }

    public void MarkDestroy(Vector2Int pos, DestroyMeta meta)
    {
        if (!destroyMarked.Add(pos)) return;     // 既に同セルを登録済みなら捨てる
        DestroyQueue.Add(new DestroyRequest { Pos = pos, Meta = meta });
    }

    public void AddCellGain(Vector2Int pos, int amount)
        => CellGains.Add((pos, amount));

    public RunState Run { get; set; } // Evaluate側から Run を参照させる

    readonly HashSet<ICountdownEffect> countdownSteppers = new();
    public void RequestCountdownStep(ICountdownEffect eff)
    {
        if (eff != null) countdownSteppers.Add(eff); // 同スピン内は一意
    }
    public IEnumerable<ICountdownEffect> GetCountdownSteppers() => countdownSteppers;
}
