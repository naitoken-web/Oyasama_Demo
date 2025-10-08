// BoardView.cs（差分版：自動生成対応）
using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] int width = 5;
    [SerializeField] int height = 4;
    [SerializeField] Transform container;              // GridLayoutGroupの親
    [SerializeField] SymbolView cellPrefab;            // ★ 追加：セルのプレハブ
    [SerializeField] bool autoBuildIfEmpty = true;     // ★ 子が無ければ自動生成

    SymbolView[,] cells;
    SymbolSO[,] lastGrid;
    public SymbolSO[,] CurrentGrid { get; private set; }

    List<ICountdownEffect> countdownSources;
    RunState run;

    void Awake()
    {
        EnsureGridBuilt();
        CacheCells(); // 生成後にキャッシュ
    }

    void EnsureGridBuilt()
    {
        if (container == null)
        {
            Debug.LogError("[BoardView] container が設定されていません。");
            return;
        }
        int childCount = container.GetComponentsInChildren<SymbolView>(includeInactive: true).Length;
        if (childCount >= width * height) return; // 既に揃っている

        if (!autoBuildIfEmpty)
        {
            Debug.LogWarning($"[BoardView] 子のSymbolViewが不足({childCount}/{width * height}) ですが autoBuildIfEmpty=false のため生成しません。");
            return;
        }
        if (cellPrefab == null)
        {
            Debug.LogError("[BoardView] cellPrefab が未設定なので自動生成できません。プレハブを割り当ててください。");
            return;
        }

        // ★ 足りない分だけ生成（全消しして作り直してもOK）
        // ここでは一旦全消し→正確に width*height を生成します。
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(container.GetChild(i).gameObject);
        }
        for (int i = 0; i < width * height; i++)
        {
            var v = Instantiate(cellPrefab, container);
            v.name = $"Cell_{i}";
            v.Clear(); // 初期は空表示に
        }

        // GridLayoutGroup を使っていれば、整列は自動で行われます
        Debug.Log($"[BoardView] 自動生成: {width * height} 個の SymbolView を作成しました。");
    }

    void CacheCells()
    {
        var childViews = container.GetComponentsInChildren<SymbolView>(includeInactive: true);
        if (childViews.Length < width * height)
        {
            Debug.LogError($"[BoardView] 子のSymbolView不足: {childViews.Length} < {width * height}. container={container?.name}");
        }
        cells = new SymbolView[width, height];

        // 左→右、上→下 で割付（GridLayoutGroup標準）
        int i = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (i < childViews.Length) cells[x, y] = childViews[i++];
            }
        }
    }

    public SymbolView GetCellView(int x, int y)
    {
        if (cells == null) { EnsureGridBuilt(); CacheCells(); }
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return cells[x, y];
    }

    public void SetFromGrid(SymbolSO[,] grid)
    {
        CurrentGrid = grid;                 // ★保持
        if (cells == null) { EnsureGridBuilt(); CacheCells(); }
        if (grid == null) { ClearAll(); lastGrid = null; return; }

        int W = grid.GetLength(0), H = grid.GetLength(1);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var v = GetCellView(x, y);
                if (v == null) continue;
                var s = grid[Mathf.Clamp(x, 0, W - 1), Mathf.Clamp(y, 0, H - 1)];
                if (s == null) v.Clear(); else v.Bind(s.Icon, "");
            }
        lastGrid = (SymbolSO[,])grid.Clone();
    }

    public void RefreshAll()
    {
        if (cells == null) { EnsureGridBuilt(); CacheCells(); }
        if (lastGrid == null)
        {
            Debug.LogWarning("[BoardView] RefreshAll before any SetFromGrid. Skipped.");
            return;
        }
        int W = lastGrid.GetLength(0), H = lastGrid.GetLength(1);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var v = GetCellView(x, y);
                if (v == null) continue;
                var s = lastGrid[Mathf.Clamp(x, 0, W - 1), Mathf.Clamp(y, 0, H - 1)];
                if (s == null) v.Clear(); else v.Bind(s.Icon, "");
            }
    }

    public void ClearAll()
    {
        if (cells == null) { EnsureGridBuilt(); CacheCells(); }
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var v = GetCellView(x, y);
                if (v != null) v.Clear();
            }
    }

    public void ApplyCountdownsForCurrentGrid()
    {
        if (CurrentGrid == null || run?.CurrentUidGrid == null) return;
        if (countdownSources == null) return;
        int W = CurrentGrid.GetLength(0), H = CurrentGrid.GetLength(1);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var v = GetCellView(x, y);
                if (!v) continue;
                int uid = run.CurrentUidGrid[x, y];
                int max = 0;
                if (uid > 0)
                    for (int i = 0; i < countdownSources.Count; i++)
                        max = Mathf.Max(max, countdownSources[i].GetCountdownByUid(uid, run));
                v.SetCountdown(max); // 0なら内部で非表示
            }
    }


    public void SetCountdownSources(List<ICountdownEffect> src, RunState r)
    {
        countdownSources = src; run = r;
    }
}
