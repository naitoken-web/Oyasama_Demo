using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] BoardView boardView;
    [SerializeField] SpinSystemEx spinSystem;
    [SerializeField] ScoreResolver resolver;
    [SerializeField] RentScheduleSO rent;//
    HudPresenter hudPresenter;
    RunInitializer runInitializer;
    FloorManager floorManager;
    CountdownCoordinator countdownCoordinator;
    DraftPresenter draftPresenter;
    ScoreCommitExecutor scoreCommitExecutor;
    EndGamePresenter endGamePresenter;
    [SerializeField] UIRelicBar relicBar;
    [SerializeField] DraftSystem draftSystem;      // ★ 追加：抽選
    [SerializeField] SymbolCatalogSO symbolCatalog;// draftSystemへ割当済なら省略可
    [SerializeField] RelicCatalogSO relicCatalog;  // 同上
    [SerializeField] UIDraftView draftView;        // ★ 追加：UI

    [Header("Config")]
    [SerializeField] DifficultyConfigSO difficultyConfig;

    RentScheduleSO activeRent; // ← 選択難易度の家賃テーブルを保持

    [Header("HUD")]
    [SerializeField] TMP_Text coinsText;[SerializeField] TMP_Text floorText;
    [SerializeField] TMP_Text spinsText;
    [SerializeField] TMP_Text lastScoreText;
    [SerializeField] Button spinButton;

    [Header("Game Over UI")]
    [SerializeField] GameObject gameOverPanel;   // 非表示のパネル
    [SerializeField] Button backToTitleButton;   // 「タイトルに戻る」

    [SerializeField] GameObject victoryPanel;
    [SerializeField] Button backToTitleOnWinButton;
    const int MaxFloorToWin = 12;

    [Header("Animation")]
    [SerializeField] SpinAnimator spinAnimator;
    [SerializeField] EffectAnimator effectFx;
    [SerializeField] CoinPopupManager popupManager;

    [Header("Consumable")]
    [SerializeField] Button inventoryButton;
    [SerializeField] UIInventoryView inventoryView;
    [SerializeField] Button removeTokenButton;

    private List<ICountdownEffect> countdownSources = new();

    bool removeModeArmed = false; // 除去モード待機フラグ（下部の除去トークンボタンでON）


    int spinsLeft = 0, rentDue = 0;
    System.Random rng;

    void Awake()
    {
        spinButton.onClick.RemoveAllListeners();
        spinButton.onClick.AddListener(() => {
            if (spinsLeft > 0) DoSpinAndResolve(showDraft: true);
        });
        inventoryButton.onClick.AddListener(() => {
            inventoryView.Open(
                run,
                removeModeArmed,
                onRemoveSymbol: (sym) => {
                    if (!removeModeArmed) return;
                    // 1つだけ削除（最初の一致）
                    if (run.Bag.RemoveOne(sym))
                    {
                        run.Consumables.RemoveToken = Mathf.Max(0, run.Consumables.RemoveToken - 1);
                        removeModeArmed = false;
                        inventoryView.Close();
                        ShowPreviewBoard(); // 必要なら一覧を再描画する関数を用意
                    }
                },
                onRemoveRelic: (relic) => {
                    if (!removeModeArmed) return;
                    if (run.Relics.Remove(relic))
                    {
                        run.Consumables.RemoveToken = Mathf.Max(0, run.Consumables.RemoveToken - 1);
                        removeModeArmed = false;
                        inventoryView.Close();
                        relicBar?.Refresh(run.Relics);
                    }
                },
                onClickConsumable: (type) => {
                    // インベントリ内の“使う”押下。除去は下部ボタン推奨だが、ここから武装しても可
                    if (type == ConsumableType.RemoveToken && run.Consumables.RemoveToken > 0)
                    {
                        removeModeArmed = true;
                        // UIの見た目を更新したかったら、そのまま再Openか SetConsumableCount などを利用
                        inventoryView.Open(run, removeModeArmed, (s) => { }, (r) => { });
                    }
                }
            );
        });
        removeTokenButton.onClick.AddListener(() => {
            if (run.Consumables.RemoveToken <= 0) return;

        hudPresenter = new HudPresenter(coinsText, floorText, spinsText, lastScoreText);
        runInitializer = new RunInitializer(run, startBag);
        floorManager = new FloorManager(hudPresenter, relicBar, spinButton);
        countdownCoordinator = new CountdownCoordinator(boardView, spinAnimator);
        draftPresenter = new DraftPresenter(draftSystem, draftView, relicBar);
        scoreCommitExecutor = new ScoreCommitExecutor(boardView, effectFx, popupManager, inventoryView);
        endGamePresenter = new EndGamePresenter(gameOverPanel, backToTitleButton, victoryPanel, backToTitleOnWinButton, hudPresenter);
        endGamePresenter?.HideAll();
        floorManager?.LoadFloor(activeRent, run, out spinsLeft, out rentDue);
        int totalSpins = 0;
        if (activeRent != null)
        {
            var entry = activeRent.Get(run.Floor);
            totalSpins = entry.Spins;
        }
        hudPresenter?.UpdateHud(run, rentDue, spinsLeft, totalSpins);
        // 1) Bag 炾Wihtgłmɓj
        countdownCoordinator?.RebuildFromBagOnly(run);
        // 2)  targeti run.CurrentUidGridjɑ΂āAUIDőOď
        countdownCoordinator?.PrewarmForTarget(run, target);
        // 3) Aj̖₢킹ɔāAɃAj[^֓n
        countdownCoordinator?.SyncWithAnimator(run);
        countdownCoordinator?.InitializeAndApply(run, target); //  ǉ
            yield return StartCoroutine(scoreCommitExecutor.CommitScoreCoroutine(target, run, sb, rng));
            countdownCoordinator?.InitializeAndApply(run, target);
        scoreCommitExecutor?.ApplyPendingBoardOps(target, run);                 //  ŔՖʕύXKp
        countdownCoordinator?.InitializeAndApply(run, target);         // JEg\ēKpiȂ̓֐j
        hudPresenter?.SetLastScore(score);
        if (showDraft && spinsLeft > 0 && draftPresenter != null)
            yield return draftPresenter.PresentSymbolDraft(run, rng);

                if (draftPresenter != null)
                    yield return draftPresenter.PresentRelicDraft(run, rng);
        runInitializer?.InitializeRun(seed);

        endGamePresenter?.ShowGameOver(() =>
            SceneLoader.LoadTitle();
        });


    {
        endGamePresenter?.ShowVictory(() =>
            SceneLoader.LoadTitle();
        });

        //  Crngi߂Ȃ߁AtAˑ̋[V[hňꎞrng
        //   DȎOKFtAȂ疈񓯂vr[ɂȂ
        run.CurrentUidGrid = uidPreview;                    //  ۑ
        hudPresenter?.SetLastScoreRaw("+0");
        countdownCoordinator?.InitializeAndApply(run, preview); //  񂩂JEg
            // リクエスト順に処理
            for (int i = 0; i < score.SpawnQueue.Count; i++)
            {
                var sreq = score.SpawnQueue[i];
                if (sreq == null || sreq.Def == null) continue;

                Vector2Int dst = sreq.Pos;

                // 希望座標が空いていなければ、空きからランダムで拾う
                bool canUsePreferred = InRange(dst, grid) && grid[dst.x, dst.y] == null;
                if (!canUsePreferred)
                {
                    if (empties.Count == 0) break; // 置けない
                    int pick = rng != null ? rng.Next(empties.Count) : UnityEngine.Random.Range(0, empties.Count);
                    dst = empties[pick];
                }

                // 盤面に置く
                grid[dst.x, dst.y] = sreq.Def;

                // Bag にも追加（破壊で減った分と整合を取る）
                run?.Bag?.Add(sreq.Def);

                // 消費した空きを除去
                for (int k = 0; k < empties.Count; k++)
                    if (empties[k].x == dst.x && empties[k].y == dst.y)
                    { empties.RemoveAt(k); break; }
            }

            // 使い終わったキューはクリア
            score.SpawnQueue.Clear();
        }
        boardView.SetFromGrid(grid);
        InitializeAndApplyCountdowns(grid); // ★追加

        // （任意）インベントリUIの反映
        if (inventoryView != null && removedVictims.Count > 0)
            foreach (var v in removedVictims) inventoryView.OnBagOneRemoved(v);

        // --- D) 破壊ボーナスの加算＆ポップアップ（前回の修正どおり） ---
        if (score.CellGains != null && score.CellGains.Count > 0)
        {
            int extra = 0;
            for (int i = 0; i < score.CellGains.Count; i++) extra += score.CellGains[i].amount;
            run.Coins += extra;
            UpdateHud();

            var gains = DraftUIUtil.ToGains(score.CellGains);
            if (popupManager != null) yield return popupManager.ShowAscending(boardView, gains);
        }
    }

    bool InRange(Vector2Int p, SymbolSO[,] grid)
    {
        int W = grid.GetLength(0), H = grid.GetLength(1);
        return p.x >= 0 && p.x < W && p.y >= 0 && p.y < H;
    }

    // GameController.cs に追加
    void InitializeAndApplyCountdowns(SymbolSO[,] grid)
    {
        if (grid == null) return;

        RebuildCountdownSourcesFromRun(grid);

        // 初期化：盤面上の uid に対して EnsureInitializedByUid
        int W = grid.GetLength(0), H = grid.GetLength(1);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var s = grid[x, y];
                if (s == null || s.Effects == null) continue;
                int uid = run.CurrentUidGrid?[x, y] ?? 0;
                if (uid <= 0) continue;

                var pos = new Vector2Int(x, y);
                for (int i = 0; i < s.Effects.Count; i++)
                    if (s.Effects[i] is ICountdownEffect ce)
                        ce.EnsureInitializedByUid(uid, run, pos);
            }

        // 反映
        spinAnimator.SetCountdownSources(countdownSources, run);
        boardView.SetCountdownSources(countdownSources, run);
        boardView.ApplyCountdownsForCurrentGrid();
    }

    void RebuildCountdownSourcesFromRun(SymbolSO[,] grid)
    {
        countdownSources.Clear();
        var uniq = new HashSet<ScriptableObject>();

        // Bag から（実際に存在する個体の種類）
        foreach (var it in run.Bag.Items)
        {
            var s = it.Symbol;
            if (s?.Effects == null) continue;
            for (int i = 0; i < s.Effects.Count; i++)
            {
                var so = s.Effects[i] as ScriptableObject;
                if (so is ICountdownEffect ce && uniq.Add(so)) countdownSources.Add(ce);
            }
        }
        // 盤面から（保険）
        if (grid != null)
        {
            int W = grid.GetLength(0), H = grid.GetLength(1);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    var s = grid[x, y];
                    if (s?.Effects == null) continue;
                    for (int i = 0; i < s.Effects.Count; i++)
                    {
                        var so = s.Effects[i] as ScriptableObject;
                        if (so is ICountdownEffect ce && uniq.Add(so)) countdownSources.Add(ce);
                    }
                }
        }
    }

    void PrewarmCountdownsForTarget(SymbolSO[,] target)
    {
        if (target == null || run?.CurrentUidGrid == null || countdownSources.Count == 0) return;

        int W = target.GetLength(0), H = target.GetLength(1);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var s = target[x, y];
                if (s == null || s.Effects == null) continue;

                int uid = run.CurrentUidGrid[x, y];
                if (uid <= 0) continue;

                var pos = new Vector2Int(x, y); // ログ/演出位置の目安用
                for (int i = 0; i < s.Effects.Count; i++)
                {
                    if (s.Effects[i] is ICountdownEffect ce)
                        ce.EnsureInitializedByUid(uid, run, pos);   // ★ UID 版で事前登録
                }
            }
    }

    void RebuildCountdownSourcesFromBagOnly()
    {
        countdownSources.Clear();
        var uniq = new HashSet<ScriptableObject>();

        if (run?.Bag?.Items == null) return;

        foreach (var it in run.Bag.Items)
        {
            var s = it.Symbol;
            if (s?.Effects == null) continue;
            for (int i = 0; i < s.Effects.Count; i++)
            {
                var so = s.Effects[i] as ScriptableObject;
                if (so is ICountdownEffect ce && uniq.Add(so))
                    countdownSources.Add(ce);
            }
        }
    }

    void ApplyPendingBoardOps(SymbolSO[,] grid)
    {
        if (run.PendingBoardOps == null || run.PendingBoardOps.Count == 0) return;
        int W = grid.GetLength(0), H = grid.GetLength(1);

        foreach (var op in run.PendingBoardOps)
        {
            if (op.Op == "DestroySelf" && op.Uid > 0)
            {
                // 現在の盤面上で uid のセルを探して消す
                for (int y = 0; y < H; y++)
                    for (int x = 0; x < W; x++)
                    {
                        if (run.CurrentUidGrid != null && run.CurrentUidGrid[x, y] == op.Uid)
                        {
                            var s = grid[x, y];
                            grid[x, y] = null;
                            if (run.CurrentUidGrid != null) run.CurrentUidGrid[x, y] = 0;
                            if (s != null) run.Bag.RemoveOneByUid(op.Uid);
                        }
                    }
            }
        }
        run.PendingBoardOps.Clear();
    }


}
