using System.Collections.Generic;
using UnityEngine;

public class CountdownCoordinator
{
    readonly List<ICountdownEffect> countdownSources = new();
    readonly BoardView boardView;
    readonly SpinAnimator spinAnimator;

    public CountdownCoordinator(BoardView boardView, SpinAnimator spinAnimator)
    {
        this.boardView = boardView;
        this.spinAnimator = spinAnimator;
    }

    public List<ICountdownEffect> CountdownSources => countdownSources;

    public void RebuildFromBagOnly(RunState run)
    {
        countdownSources.Clear();
        var uniq = new HashSet<ScriptableObject>();

        if (run?.Bag?.Items == null) return;

        foreach (var it in run.Bag.Items)
        {
            var symbol = it.Symbol;
            if (symbol?.Effects == null) continue;
            for (int i = 0; i < symbol.Effects.Count; i++)
            {
                if (symbol.Effects[i] is ScriptableObject so && symbol.Effects[i] is ICountdownEffect ce && uniq.Add(so))
                {
                    countdownSources.Add(ce);
                }
            }
        }
    }

    public void PrewarmForTarget(RunState run, SymbolSO[,] target)
    {
        if (target == null || run?.CurrentUidGrid == null || countdownSources.Count == 0) return;

        int width = target.GetLength(0);
        int height = target.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var symbol = target[x, y];
                if (symbol == null || symbol.Effects == null) continue;

                int uid = run.CurrentUidGrid[x, y];
                if (uid <= 0) continue;

                var pos = new Vector2Int(x, y);
                for (int i = 0; i < symbol.Effects.Count; i++)
                {
                    if (symbol.Effects[i] is ICountdownEffect ce)
                    {
                        ce.EnsureInitializedByUid(uid, run, pos);
                    }
                }
            }
        }
    }

    public void InitializeAndApply(RunState run, SymbolSO[,] grid)
    {
        if (grid == null) return;
        RebuildFromRun(run, grid);
        SyncWithAnimator(run);
        ApplyCountdownsForCurrentGrid();
    }

    public void SyncWithAnimator(RunState run)
    {
        spinAnimator?.SetCountdownSources(countdownSources, run);
        boardView?.SetCountdownSources(countdownSources, run);
    }

    public void ApplyCountdownsForCurrentGrid()
    {
        boardView?.ApplyCountdownsForCurrentGrid();
    }

    void RebuildFromRun(RunState run, SymbolSO[,] grid)
    {
        countdownSources.Clear();
        var uniq = new HashSet<ScriptableObject>();

        if (run?.Bag?.Items != null)
        {
            foreach (var it in run.Bag.Items)
            {
                var symbol = it.Symbol;
                if (symbol?.Effects == null) continue;
                for (int i = 0; i < symbol.Effects.Count; i++)
                {
                    if (symbol.Effects[i] is ScriptableObject so && symbol.Effects[i] is ICountdownEffect ce && uniq.Add(so))
                    {
                        countdownSources.Add(ce);
                    }
                }
            }
        }

        if (grid == null) return;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var symbol = grid[x, y];
                if (symbol?.Effects == null) continue;
                for (int i = 0; i < symbol.Effects.Count; i++)
                {
                    if (symbol.Effects[i] is ScriptableObject so && symbol.Effects[i] is ICountdownEffect ce && uniq.Add(so))
                    {
                        countdownSources.Add(ce);
                    }
                }
            }
        }

        if (grid != null && run?.CurrentUidGrid != null)
        {
            int widthUid = grid.GetLength(0);
            int heightUid = grid.GetLength(1);
            for (int y = 0; y < heightUid; y++)
            {
                for (int x = 0; x < widthUid; x++)
                {
                    var symbol = grid[x, y];
                    if (symbol == null || symbol.Effects == null) continue;
                    int uid = run.CurrentUidGrid[x, y];
                    if (uid <= 0) continue;
                    var pos = new Vector2Int(x, y);
                    for (int i = 0; i < symbol.Effects.Count; i++)
                    {
                        if (symbol.Effects[i] is ICountdownEffect ce)
                        {
                            ce.EnsureInitializedByUid(uid, run, pos);
                        }
                    }
                }
            }
        }
    }
}
