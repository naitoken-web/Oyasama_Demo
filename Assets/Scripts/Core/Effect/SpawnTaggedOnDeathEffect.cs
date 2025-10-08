// SpawnFromAttachedPoolOnDeathEffect.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自身が今ターンに破壊予定なら、インスペクタで指定した候補プールから
/// ランダムに X 個のシンボルを選び、空マスへ生成する。
/// </summary>
[CreateAssetMenu(menuName = "Lucklike/Effects/Spawn From Attached Pool On Death")]
public class SpawnFromAttachedPoolOnDeathEffect : EffectSO
{
    [Header("抽選元プール（ここに列挙した SymbolSO から選ぶ）")]
    public List<SymbolSO> CandidatePool = new();

    [Header("生成数 X")]
    [Min(1)] public int SpawnCount = 1;

    [Header("配置設定")]
    [Tooltip("まずは自分がいたマスを最優先で埋める")]
    public bool PreferOwnerCell = true;

    [Header("抽選設定")]
    [Tooltip("同じシンボルを重複選出してよい")]
    public bool AllowDuplicates = true;

    [Tooltip("SymbolSO.Weight を確率重みとして使う（<=0 は 1 とみなす）")]
    public bool UseWeights = true;

    [TextArea] public string Description;

    public override void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score)
    {
        // 破壊予定でなければ発動しない
        if (!score.IsMarkedForDestroy(ctx.Pos)) return;

        // プールから有効候補を整える（null を除外）
        var candidates = BuildPool(CandidatePool);
        if (candidates.Count == 0) return;

        // 生成先セルを集める
        var targets = CollectTargets(board, ctx.Pos, SpawnCount, PreferOwnerCell);
        if (targets.Count == 0) return;

        // 抽選して生成をキュー
        var pool = AllowDuplicates ? candidates : new List<SymbolSO>(candidates);
        int num = Mathf.Min(SpawnCount, targets.Count);

        for (int i = 0; i < num; i++)
        {
            var chosen = ChooseOne(pool, UseWeights);
            if (chosen == null) break;

            if (!AllowDuplicates)
            {
                pool.Remove(chosen);
                if (pool.Count == 0 && i < num - 1) break;
            }

            // ↓あなたの生成APIに合わせて置き換え可
            score.EnqueueSpawn(chosen, targets[i], "SpawnFromAttachedPoolOnDeath");
        }
    }

    // ===== ヘルパ =====

    private List<SymbolSO> BuildPool(List<SymbolSO> src)
    {
        var list = new List<SymbolSO>();
        if (src == null) return list;
        for (int i = 0; i < src.Count; i++)
        {
            var s = src[i];
            if (s == null) continue;
            list.Add(s);
        }
        return list;
    }

    private List<Vector2Int> CollectTargets(BoardSnapshot board, Vector2Int owner, int need, bool preferOwner)
    {
        var targets = new List<Vector2Int>(need);

        if (preferOwner && IsEmpty(board, owner))
            targets.Add(owner);

        var empties = new List<Vector2Int>();
        int W = board.Grid.GetLength(0), H = board.Grid.GetLength(1);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var p = new Vector2Int(x, y);
                if (preferOwner && p == owner) continue;
                if (IsEmpty(board, p)) empties.Add(p);
            }

        // Fisher–Yates シャッフル
        for (int i = empties.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (empties[i], empties[j]) = (empties[j], empties[i]);
        }

        int take = Mathf.Min(need - targets.Count, empties.Count);
        for (int i = 0; i < take; i++) targets.Add(empties[i]);

        return targets;
    }

    private bool IsEmpty(BoardSnapshot board, Vector2Int p) => board.Grid[p.x, p.y] == null;

    private SymbolSO ChooseOne(List<SymbolSO> src, bool useWeights)
    {
        if (src == null || src.Count == 0) return null;

        if (!useWeights)
            return src[Random.Range(0, src.Count)];

        // 重み付き（Weight <= 0 は 1 として扱う）
        float total = 0f;
        for (int i = 0; i < src.Count; i++)
            total += Mathf.Max(1f, src[i].Weight);

        float r = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < src.Count; i++)
        {
            acc += Mathf.Max(1f, src[i].Weight);
            if (r <= acc) return src[i];
        }
        return src[src.Count - 1];
    }
}
