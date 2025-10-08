using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// ScoreResolver.cs
[CreateAssetMenu(fileName = "ScoreResolver", menuName = "Scriptable Objects/ScoreResolver")]
public class ScoreResolver : ScriptableObject
{

    public readonly List<AnimEvent> AnimEvents = new();
    public readonly List<DestroyRequest> DestroyQueue = new();
    public readonly List<(Vector2Int pos, int amount)> CellGains = new();

    public void EmitAnim(AnimEvent ev) => AnimEvents.Add(ev);

    public void MarkDestroy(Vector2Int pos, DestroyMeta meta)
        => DestroyQueue.Add(new DestroyRequest { Pos = pos, Meta = meta });

    public void AddCellGain(Vector2Int pos, int amount)
        => CellGains.Add((pos, amount));

    // ScoreResolver.cs（既存に追加）
    // 盤面ごとの最終スコアを返しつつ、各マスの獲得額リストも出す
    public int ResolveDetailed(SymbolSO[,] grid, out List<CellGain> gains, out string log)
    {
        gains = new List<CellGain>(grid.Length);
        // ここはあなたの既存ロジックに合わせて「セルごとの寄与額」を算出してください。
        // 例：単純に s.BaseScore をセル獲得額とする（nullは0）
        int total = 0;
        System.Text.StringBuilder sb = new();
        int W = grid.GetLength(0), H = grid.GetLength(1);
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                var s = grid[x, y];
                int a = (s == null) ? 0 : s.BaseScore;       // ←あなたの計算式へ置き換え可
                                                             // もし隣接ボーナスやタグ倍率があるなら、ここで a に反映してください
                if (a != 0) gains.Add(new CellGain(x, y, a));
                total += a;
            }
        log = sb.ToString();
        return total;
    }

}