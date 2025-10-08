using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCommitExecutor
{
    readonly BoardView boardView;
    readonly EffectAnimator effectAnimator;
    readonly CoinPopupManager popupManager;
    readonly UIInventoryView inventoryView;

    public ScoreCommitExecutor(BoardView boardView, EffectAnimator effectAnimator, CoinPopupManager popupManager, UIInventoryView inventoryView)
    {
        this.boardView = boardView;
        this.effectAnimator = effectAnimator;
        this.popupManager = popupManager;
        this.inventoryView = inventoryView;
    }

    public IEnumerator CommitScoreCoroutine(SymbolSO[,] grid, RunState run, ScoreBuilder score, System.Random rng)
    {
        var playedShatter = new HashSet<Vector2Int>();
        foreach (var ev in score.AnimEvents)
        {
            var view = boardView != null ? boardView.GetCellView(ev.Pos.x, ev.Pos.y) : null;
            if (!view) continue;

            switch (ev.Type)
            {
                case AnimEventType.Bounce:
                    if (effectAnimator != null)
                        yield return effectAnimator.Bounce(view, ev.Duration);
                    break;

                case AnimEventType.Shatter:
                    if (!playedShatter.Add(ev.Pos)) break;
                    effectAnimator?.Shatter(view, ev.Duration);
                    break;
            }
        }

        if (playedShatter.Count > 0)
            yield return new WaitForSeconds(0.1f);

        var processedCells = new HashSet<Vector2Int>();
        var removedVictims = new List<SymbolSO>();

        foreach (var req in score.DestroyQueue)
        {
            var pos = req.Pos;
            if (!processedCells.Add(pos)) continue;

            var victim = req.Meta?.Victim ?? grid[pos.x, pos.y];
            if (victim == null) continue;

            if (grid[pos.x, pos.y] == victim) grid[pos.x, pos.y] = null;

            if (run?.Bag != null && run.Bag.RemoveOne(victim))
                removedVictims.Add(victim);

            run?.DestroyHistory.Add(new DestroyEntry
            {
                Floor = run.Floor,
                SpinIndex = run.TotalSpins,
                Symbol = victim,
                PosX = pos.x,
                PosY = pos.y,
                Cause = req.Meta?.Cause ?? "Unknown"
            });
        }

        if (score.SpawnQueue != null && score.SpawnQueue.Count > 0)
        {
            var empties = new List<Vector2Int>();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] == null)
                        empties.Add(new Vector2Int(x, y));
                }
            }

            for (int i = 0; i < score.SpawnQueue.Count; i++)
            {
                var sreq = score.SpawnQueue[i];
                if (sreq == null || sreq.Def == null) continue;

                Vector2Int dst = sreq.Pos;

                bool canUsePreferred = InRange(dst, grid) && grid[dst.x, dst.y] == null;
                if (!canUsePreferred)
                {
                    if (empties.Count == 0) break;
                    int pick = rng != null ? rng.Next(empties.Count) : UnityEngine.Random.Range(0, empties.Count);
                    dst = empties[pick];
                }

                grid[dst.x, dst.y] = sreq.Def;
                run?.Bag?.Add(sreq.Def);

                for (int k = 0; k < empties.Count; k++)
                {
                    if (empties[k].x == dst.x && empties[k].y == dst.y)
                    {
                        empties.RemoveAt(k);
                        break;
                    }
                }
            }

            score.SpawnQueue.Clear();
        }

        boardView?.SetFromGrid(grid);

        if (inventoryView != null && removedVictims.Count > 0)
        {
            foreach (var v in removedVictims)
                inventoryView.OnBagOneRemoved(v);
        }

        if (score.CellGains != null && score.CellGains.Count > 0)
        {
            int extra = 0;
            for (int i = 0; i < score.CellGains.Count; i++) extra += score.CellGains[i].amount;
            if (run != null)
            {
                run.Coins += extra;
            }

            var gains = DraftUIUtil.ToGains(score.CellGains);
            if (popupManager != null)
                yield return popupManager.ShowAscending(boardView, gains);
        }
    }

    public void ApplyPendingBoardOps(SymbolSO[,] grid, RunState run)
    {
        if (grid == null || run?.PendingBoardOps == null || run.PendingBoardOps.Count == 0) return;
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        foreach (var op in run.PendingBoardOps)
        {
            if (op.Op == "DestroySelf" && op.Uid > 0)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (run.CurrentUidGrid != null && run.CurrentUidGrid[x, y] == op.Uid)
                        {
                            var symbol = grid[x, y];
                            grid[x, y] = null;
                            if (run.CurrentUidGrid != null) run.CurrentUidGrid[x, y] = 0;
                            if (symbol != null) run?.Bag?.RemoveOneByUid(op.Uid);
                        }
                    }
                }
            }
        }

        run.PendingBoardOps.Clear();
    }

    bool InRange(Vector2Int p, SymbolSO[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    }
}
