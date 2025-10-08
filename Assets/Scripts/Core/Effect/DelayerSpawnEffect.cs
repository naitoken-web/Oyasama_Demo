// DelayedSpawnEffect.cs（UIDベース版）
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Lucklike/Effects/Delayed Spawn (UID)")]
public class DelayedSpawnEffect : EffectSO, ICountdownEffect
{
    [Header("Spawn")]
    public SymbolSO SpawnSymbol;
    [Min(1)] public int SpawnCount = 1;
    [Min(1)] public int DelaySpins = 3;

    [Header("After Resolve")]
    public bool DestroySelfOnResolve = true;

    [TextArea] public string Description;

    // ===== EffectSO =====
    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        // Evaluate 自体では何もしない（初期化は GameController 側の初期化関数で行う）
        score.RequestCountdownStep(this); // このスピン終端で Step() を呼んでもらう
    }

    // ===== ICountdownEffect =====
    Dictionary<int, int> Dict(RunState run)
    {
        if (run == null) return null;
        if (!run.countdownByUid.TryGetValue(this, out var d))
        {
            d = new Dictionary<int, int>();
            run.countdownByUid[this] = d;
        }
        return d;
    }

    public void EnsureInitializedByUid(int uid, RunState run, Vector2Int pos)
    {
        if (uid <= 0) return;
        var d = Dict(run);
        if (d == null) return;
        if (!d.ContainsKey(uid)) d[uid] = DelaySpins;
    }

    public int GetCountdownByUid(int uid, RunState run)
    {
        if(uid == 6)
        {
            var y = Dict(run);
            var k =y.TryGetValue(uid, out var b) ? b : 0;
            Debug.Log($"[DelayedSpawnEffect] TryGetValue({uid}) = {k}");
        }
        if (uid <= 0) return 0;
        var d = Dict(run);
        if (d == null) return 0;
        var x = d.TryGetValue(uid, out var v) ? v : 0;
        return x;
    }

    public void Step(RunState run)
    {
        var d = Dict(run);
        if (d == null || d.Count == 0) return;

        var keys = new List<int>(d.Keys);
        foreach (var uid in keys)
        {
            var v = d[uid] - 1;
            d[uid] = v;

            if (v == 0)
            {
                // 生成
                for (int i = 0; i < SpawnCount; i++)
                    run.Bag.Add(SpawnSymbol);

                // 盤面/Bagの変更要求は保留キューに（GameControllerで適用）
                if (DestroySelfOnResolve)
                    run.PendingBoardOps.Add(new RunState.PendingBoardOp { Op = "DestroySelf", Uid = uid });

                d.Remove(uid);
            }
        }
    }
}
