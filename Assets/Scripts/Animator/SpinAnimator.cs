// SpinAnimator.cs（列別→全体一括 抽選＆描画 版）
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAnimator : MonoBehaviour
{
    [SerializeField] BoardView boardView;

    [Header("Timing")]
    [SerializeField, Min(0f)] float columnStagger = 0.08f; // 列が“参加”し始める遅延（見た目の段階的スタート）
    [SerializeField, Min(0.1f)] float baseDuration = 0.9f;  // 全体のスピン時間
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, Tooltip("最速〜最遅の更新間隔")] Vector2 tickIntervalRange = new Vector2(0.02f, 0.09f);

    Bag currentBag;
    public void SetBag(Bag bag) => currentBag = bag;

    SymbolView GetView(int x, int y) => boardView != null ? boardView.GetCellView(x, y) : null;

    // 内部状態
    SymbolSO[,] currentFxGrid;                // 現在“表示中”の盤面（出現上限チェック用）
    Dictionary<SymbolSO, int> bagCounts;      // 所持シンボルの枚数辞書
    int totalEmpty;                           // 空白総枠 = 総セル数 - 所持総枚数

    List<ICountdownEffect> countdownSources;
    RunState run;

    int[,] currentFxUidGrid;   // ★ アニメ中の“今見せている個体UID”を保持


    Dictionary<SymbolSO, int> BuildBagCounts()
    {
        var d = new Dictionary<SymbolSO, int>();
        if (currentBag?.Items != null)
        {
            foreach (var s in currentBag.Items)
            {
                if (s.Symbol == null) continue;
                d.TryGetValue(s.Symbol, out int c);
                d[s.Symbol] = c + 1;
            }
        }
        return d;
    }

    void CountCurrentExcluding(
        HashSet<(int x, int y)> exclude,
        out Dictionary<SymbolSO, int> used,
        out int emptyUsed,
        out Dictionary<SymbolSO, HashSet<int>> usedUids,
        int W, int H)
    {
        used = new Dictionary<SymbolSO, int>();
        usedUids = new Dictionary<SymbolSO, HashSet<int>>();
        emptyUsed = 0;

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                if (exclude != null && exclude.Contains((x, y))) continue;

                var s = currentFxGrid[x, y];
                int uid = (currentFxUidGrid != null) ? currentFxUidGrid[x, y] : 0;

                if (s == null)
                {
                    emptyUsed++;
                    continue;
                }

                if (!used.TryGetValue(s, out var c)) c = 0;
                used[s] = c + 1;

                if (!usedUids.TryGetValue(s, out var set)) usedUids[s] = set = new HashSet<int>();
                if (uid > 0) set.Add(uid);
            }
    }


    // 残り枚数（bagCounts - used）から、更新対象セル数 N 分を「非復元で」サンプル
    List<SymbolSO> SampleWithoutReplacement(System.Random rng, Dictionary<SymbolSO, int> remainder, int emptyRemain, int N)
    {
        var pool = new List<SymbolSO>(N * 2);
        foreach (var kv in remainder) for (int i = 0; i < kv.Value; i++) pool.Add(kv.Key);
        for (int i = 0; i < emptyRemain; i++) pool.Add(null);

        if (pool.Count == 0) { for (int i = 0; i < N; i++) pool.Add(null); }

        // シャッフル
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // 先頭から N 件（不足すれば空白で埋め）
        var outList = new List<SymbolSO>(N);
        for (int i = 0; i < N; i++) outList.Add(i < pool.Count ? pool[i] : null);
        return outList;
    }

    public IEnumerator PlaySpinTo(SymbolSO[,] target, System.Random rng)
    {
        int W = target.GetLength(0);
        int H = target.GetLength(1);
        if (GetView(0, 0) == null) { Debug.LogWarning("[SpinAnimator] BoardView cell is null. Skip FX."); yield break; }


        // 初期化：全セル空表示＆内部盤面クリア
        currentFxGrid = new SymbolSO[W, H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var v = GetView(x, y);
                if (v != null) { v.Clear(); }
                currentFxGrid[x, y] = null;
            }

        bagCounts = BuildBagCounts();
        int bagTotal = 0; foreach (var c in bagCounts.Values) bagTotal += c;
        totalEmpty = Mathf.Max(0, W * H - bagTotal);

        float t = 0f;
        float tickAcc = 0f;

        // 各列の“参加開始時間”を事前計算（見た目の段階的スタート）
        var colStart = new float[W];
        for (int x = 0; x < W; x++) colStart[x] = columnStagger * x;

        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / Mathf.Max(0.0001f, baseDuration));
            float interval = Mathf.Lerp(tickIntervalRange.x, tickIntervalRange.y, ease.Evaluate(t));
            tickAcc += Time.deltaTime;

            if (tickAcc >= interval)
            {
                tickAcc = 0f;

                // いま“回す対象”になるセル集合（アクティブ列のみ）を作る
                var activeCells = new List<(int x, int y)>(W * H);
                for (int x = 0; x < W; x++)
                {
                    if (t * baseDuration >= colStart[x])
                    {
                        for (int y = 0; y < H; y++) activeCells.Add((x, y));
                    }
                }
                if (activeCells.Count == 0) { yield return null; continue; }

                // 更新順をランダム化（毎ティックごとにグリッド全体でバラける）
                for (int i = activeCells.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (activeCells[i], activeCells[j]) = (activeCells[j], activeCells[i]);
                }

                // まとめて“非復元”サンプル：
                //   1) 今回更新するセル集合を exclude として、他のセルで使用中の数を集計
                // いま“回す対象”になるセル集合（アクティブ列のみ）を作る …（この上は既存のままでOK）

                var exclude = new HashSet<(int x, int y)>(activeCells);
                CountCurrentExcluding(
                    exclude,
                    out var usedOthers,
                    out int emptyUsedOthers,
                    out var usedUidsOthers,
                    W, H
                );

                // 1) 残り枚数 = bagCounts - usedOthers（0 未満は 0 に丸め）
                var remainder = new Dictionary<SymbolSO, int>();
                foreach (var kv in bagCounts)
                {
                    var sym = kv.Key;
                    var have = kv.Value;
                    usedOthers.TryGetValue(sym, out var already);
                    var left = have - already;
                    if (left > 0) remainder[sym] = left;
                }

                // 2) 残り空白 = totalEmpty - emptyUsedOthers（0 未満は 0）
                int emptyRemain = Mathf.Max(0, totalEmpty - emptyUsedOthers);

                // 3) アクティブセル分だけ非復元サンプル（SymbolSO の列）
                var draws = SampleWithoutReplacement(rng, remainder, emptyRemain, activeCells.Count);

                // UID 配列のサイズ確保
                if (currentFxUidGrid == null
                 || currentFxUidGrid.GetLength(0) != W
                 || currentFxUidGrid.GetLength(1) != H)
                {
                    currentFxUidGrid = new int[W, H];
                }

                // バッグの UID バケット（シンボル → 利用可能 UID 群）
                var uidBuckets = BuildBagUidBuckets();
                // このティックで新規に割り当てた UID を控える（シンボル別）
                var assignedThisTick = new Dictionary<SymbolSO, HashSet<int>>();

                // 4) draws[i] を activeCells[i] に適用（UID は未使用優先で割り当て）
                for (int i = 0; i < activeCells.Count; i++)
                {
                    var (x, y) = activeCells[i];

                    var s = draws[i];
                    int uid = 0;

                    if (s != null)
                    {
                        uidBuckets.TryGetValue(s, out var bucket);
                        if (bucket != null && bucket.Count > 0)
                        {
                            usedUidsOthers.TryGetValue(s, out var usedSet);
                            if (!assignedThisTick.TryGetValue(s, out var tickSet))
                                assignedThisTick[s] = tickSet = new HashSet<int>();

                            int chosen = 0;
                            for (int bi = 0; bi < bucket.Count; bi++)
                            {
                                int cand = bucket[bi];
                                if (cand <= 0) continue;
                                if (usedSet != null && usedSet.Contains(cand)) continue;
                                if (tickSet.Contains(cand)) continue;
                                chosen = cand;
                                break;
                            }
                            uid = (chosen != 0) ? chosen : bucket[0]; // 最悪でも何か1つ
                            tickSet.Add(uid);
                        }
                    }

                    var v = GetView(x, y);
                    currentFxGrid[x, y] = s;
                    currentFxUidGrid[x, y] = uid;

                    if (v != null) BindTempAt(v, s, x, y);
                }




            }
            yield return null;
        }

        // 停止：最終ターゲットを確定描画
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var v = GetView(x, y);
                if (!v) continue;

                var s = target[x, y];
                currentFxGrid[x, y] = s;

                int uid = (run?.CurrentUidGrid != null) ? run.CurrentUidGrid[x, y] : 0;
                if (currentFxUidGrid == null || currentFxUidGrid.GetLength(0) != W || currentFxUidGrid.GetLength(1) != H)
                    currentFxUidGrid = new int[W, H];
                currentFxUidGrid[x, y] = uid;

                BindTempAt(v, s, x, y);
            }

        yield return new WaitForSeconds(0.3f);
    }

    // フィールドの下あたりに追加
    public void SetCountdownSources(List<ICountdownEffect> sources, RunState r)
    {
        countdownSources = sources;
        run = r;
    }

    void BindTempAt(SymbolView v, SymbolSO s, int x, int y)
    {
        if (!v) return;
        if (s == null)
        {
            v.Clear();
            v.SetCountdown(0);
            return;
        }

        v.Bind(s.Icon, "");

        int uid = 0;
        if (currentFxUidGrid != null) uid = currentFxUidGrid[x, y];
        else if (run?.CurrentUidGrid != null) uid = run.CurrentUidGrid[x, y];

        int turns = 0;
        if (uid > 0 && countdownSources != null && run != null)
        {
            for (int i = 0; i < countdownSources.Count; i++)
                turns = Mathf.Max(turns, countdownSources[i].GetCountdownByUid(uid, run));
        }
        v.SetCountdown(turns);
    }


    Dictionary<SymbolSO, List<int>> BuildBagUidBuckets()
    {
        var d = new Dictionary<SymbolSO, List<int>>();
        if (currentBag?.Items == null) return d;
        foreach (var it in currentBag.Items)
        {
            var s = it.Symbol;
            if (s == null) continue;
            if (!d.TryGetValue(s, out var list)) d[s] = list = new List<int>();
            list.Add(it.Uid);
        }
        return d;
    }

}
