// GameController.cs（差分の肝）
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class GameController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] BoardView boardView;
    [SerializeField] SpinSystemEx spinSystem;
    [SerializeField] ScoreResolver resolver;
    [SerializeField] RentScheduleSO rent;//
    [SerializeField] RunState run;                 // ★ 追加：統合状態
    [SerializeField] StartBagSO startBag; // ← 追加
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
            removeModeArmed = true;                 // ★ 武装
            inventoryView.Open(run, removeModeArmed,
                onRemoveSymbol: (sym) => {
                    if (!removeModeArmed) return;
                    if (run.Bag.RemoveOne(sym))
                    {
                        run.Consumables.RemoveToken--;
                        removeModeArmed = false;
                        inventoryView.Close();
                        boardView.RefreshAll();
                    }
                },
                onRemoveRelic: (relic) => {
                    if (!removeModeArmed) return;
                    if (run.Relics.Remove(relic))
                    {
                        run.Consumables.RemoveToken--;
                        removeModeArmed = false;
                        inventoryView.Close();
                        relicBar?.Refresh(run.Relics);
                    }
                }
            );
        });
    }

    void Start()
    {
        // 難易度に応じてRentScheduleを決定
        var entry = difficultyConfig != null
                  ? difficultyConfig.GetByDifficulty1Based(Mathf.Max(1, GameBootOptions.SelectedDifficulty))
                  : null;
        activeRent = entry != null ? entry.RentSchedule : null;

        // 念のためフォールバック
        if (activeRent == null)
        {
            Debug.LogWarning("[GameController] activeRent is null. Please set DifficultyConfigSO.");
        }

        if (victoryPanel) victoryPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        rng = new System.Random(run.Seed);
        StartNewRun();

        spinAnimator.SetCountdownSources(countdownSources, run);
        boardView.SetCountdownSources(countdownSources, run);  // ★ 追加
        // ★ フロアに入ったらまず“見せるだけ”
        ShowPreviewBoard();
    }

    void LoadFloor(int f)
    {
        run.Floor = f;

        // ここで今までの rent.Get(...) を activeRent に差し替える
        var e = activeRent != null ? activeRent.Get(run.Floor) : default;
        spinsLeft = e.Spins;
        rentDue = e.RequiredScore;

        UpdateHud();
        relicBar?.Refresh(run.Relics);
        spinButton.interactable = true;

    }

    void UpdateHud()
    {
        coinsText.text = $"Coins: {run.Coins} / {rentDue}";
        floorText.text = $"Stage: {run.Floor}";
        spinsText.text = $"Turn: {spinsLeft} / {activeRent.Get(run.Floor).Spins}";
    }

    void DoSpinAndResolve(bool showDraft)
    {
        var(target, uidGrid) = spinSystem.SpinDeal(run.Bag, rng);
        run.CurrentUidGrid = uidGrid;

        spinButton.interactable = false;

        // ★ Bagをアニメーターへ
        spinAnimator.SetBag(run.Bag);

        StartCoroutine(SpinRoutine(target, showDraft));
    }

    IEnumerator SpinRoutine(SymbolSO[,] target, bool showDraft)
    {
        // 1) Bag からだけ収集（ドラフト直後でも確実に入る）
        RebuildCountdownSourcesFromBagOnly();
        // 2) 今回回す target（と run.CurrentUidGrid）に対して、UIDで前もって初期化
        PrewarmCountdownsForTarget(target);
        // 3) アニメ中の問い合わせに備えて、先にアニメータへ渡す
        spinAnimator.SetCountdownSources(countdownSources, run);
        yield return spinAnimator.PlaySpinTo(target, rng);
        boardView.SetFromGrid(target);
        InitializeAndApplyCountdowns(target); // ★ 追加

        // 1) ★ 効果適用（BoardSnapshot + EffectSO.Evaluate → ScoreBuilder に積む）
        // BoardSnapshot のコンストラクタ引数はあなたの定義に合わせてください。
        var snapshot = new BoardSnapshot(target); // 例：盤面参照だけで足りる場合
        var sb = new ScoreBuilder { Run = run };

        // 盤面上の全セルを走査して効果を発火
        int W = target.GetLength(0), H = target.GetLength(1);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                var s = target[x, y];
                if (s == null || s.Effects == null) continue;

                var ctx = new SymbolContext { Pos = new Vector2Int(x, y), Self = s };

                for (int i = 0; i < s.Effects.Count; i++)
                {
                    var eff = s.Effects[i];
                    if (eff == null) continue;

                    if (eff is ICountdownEffect ce)
                    {
                        int uid = run.CurrentUidGrid?[x, y] ?? 0;
                        if (uid > 0) ce.EnsureInitializedByUid(uid, run, ctx.Pos); // ★ 新API
                        sb.RequestCountdownStep(ce);
                    }

                    eff.Evaluate(ctx, snapshot, sb);
                }
            }
        run.ActiveRarityBoosts.Clear();
        run.ActiveRarityBoosts.AddRange(sb.DraftBoosts);

        // 2) ★ 効果のコミット＆演出（破壊→描画更新→アニメ→追加ゲイン popup）
        //    SO ではコルーチン不可なので MonoBehaviour 側で必ず待つ
        if (sb.DestroyQueue.Count > 0 || sb.AnimEvents.Count > 0 || sb.CellGains.Count > 0)
        {
            yield return StartCoroutine(CommitScoreCoroutine(target, run, sb));
        }

        // ★ スピン終了時：このスピンで要求されたカウントダウンだけ Step() 実行
        foreach (var stepper in sb.GetCountdownSteppers())
            stepper.Step(run);

        ApplyPendingBoardOps(target);                 // ★ ここで盤面変更を適用
        boardView.SetFromGrid(target);                // 見た目を更新
        InitializeAndApplyCountdowns(target);         // カウント表示も再適用（あなたの統合関数）

        // 3) 従来の通常スコア解決（target は破壊反映済み）
        var score = resolver.ResolveDetailed(target, out var gains, out var log);
        run.Coins += score; spinsLeft--;
        lastScoreText.text = $"+{score}";
        UpdateHud();

        if (popupManager != null && gains != null && gains.Count > 0)
        {
            yield return popupManager.ShowAscending(boardView, gains);
        }

        // 4) ドラフト・家賃チェック（既存どおり）
        draftSystem.CurrentRun = run; // ★ これだけ
        if (showDraft && spinsLeft > 0)
            yield return PresentSymbolDraftRoutine();

        if (spinsLeft == 0)
        {
            if (run.Coins >= rentDue)
            {
                run.Coins -= rentDue;
                yield return PresentRelicDraftRoutine();
                if (run.Floor >= MaxFloorToWin) ShowVictory();
                else LoadFloor(run.Floor + 1);
                yield break;
            }
            else
            {
                ShowGameOver();
                yield break;
            }
        }

        spinButton.interactable = true;
    }


    void StartNewRun(int seed = 0)
    {
        run.Seed = seed;
        run.Coins = 0;
        run.Floor = 1;
        run.Relics.Clear();
        run.DestroyHistory.Clear();
        run.ActiveRarityBoosts.Clear();

        run.Consumables = new ConsumableCounts
        {
            RemoveToken = 1,
            RerollItem = 1,
            RerollRelic = 1
        };

        // ランタイム用バッグを毎回生成して中身をコピー
        run.Bag = ScriptableObject.CreateInstance<Bag>();
        foreach (var s in startBag.Items) run.Bag.Add(s);   // ★ UID を振って追加

        rng = new System.Random(run.Seed);
        LoadFloor(run.Floor);

    }

    void ShowGameOver()
    {
        spinButton.interactable = false;
        // もしドラフトUIが開いていたら閉じる等のガードを入れる（任意）
        // draftView.Hide(); など

        if (lastScoreText) lastScoreText.text += "\n<color=#f55>GAME OVER</color>";
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (backToTitleButton)
        {
            backToTitleButton.onClick.RemoveAllListeners();
            backToTitleButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadTitle();
            });
        }
    }

    void ShowVictory()
    {
        spinButton.interactable = false;
        if (victoryPanel) victoryPanel.SetActive(true);
        if (backToTitleOnWinButton)
        {
            backToTitleOnWinButton.onClick.RemoveAllListeners();
            backToTitleOnWinButton.onClick.AddListener(() =>
            {
                SceneLoader.LoadTitle();
            });
        }
        UnlockNextDifficultyIfNeeded();
    }

    // GameController.cs（差分：勝利時アンロック）
    void UnlockNextDifficultyIfNeeded()
    {
        const string KeyUnlocked = "UnlockedDifficultyMax";
        int current = Mathf.Max(1, GameBootOptions.SelectedDifficulty);

        int maxByConfig = Mathf.Max(1, difficultyConfig != null ? difficultyConfig.Count : 1);

        int unlocked = PlayerPrefs.GetInt(KeyUnlocked, 1);
        int target = Mathf.Min(current + 1, maxByConfig); // Configの件数を上限

        if (target > unlocked)
        {
            PlayerPrefs.SetInt(KeyUnlocked, target);
            PlayerPrefs.Save();
        }
    }

    // GameController.cs（差分）
    // GameController.cs（差分：ドラフトルーチン）
    IEnumerator PresentSymbolDraftRoutine()
    {
        var options = draftSystem.GenerateSymbolOptions(rng, 3);
        bool decided = false;

        // これをUIDraftViewから受け取る
        System.Action<List<SymbolSO>> setOptions = null;

        // ★ 購読（+= / -=）
        System.Action rerollHandler = null;
        rerollHandler = () => {
            if (decided) return;
            if (run.Consumables.RerollItem <= 0) return;

            run.Consumables.RerollItem--;
            options = draftSystem.GenerateSymbolOptions(rng, 3);

            // ★ UIに新候補を渡して即再Bind（ShowSymbolsSlideInEx 内で行われる）
            setOptions?.Invoke(options);

            // ボタン活性を更新
            draftView.SetRerollInteractable(run.Consumables.RerollItem > 0);
        };
        draftView.OnRerollRequested += rerollHandler;

        // ★ setup で SetOptions と EnableReroll を受け取る
        yield return draftView.ShowSymbolsSlideInEx(
            options,
            onPick: (picked) => { run.Bag.Add(picked); decided = true; },
            onSkip: () => { decided = true; },
            setup: (set, enableReroll) => {
                setOptions = set;
                enableReroll(run.Consumables.RerollItem > 0); // 初期活性
            }
        );

        draftView.OnRerollRequested -= rerollHandler;
    }



    IEnumerator PresentRelicDraftRoutine()
    {
        var options = draftSystem.GenerateRelicOptions(rng, 3);
        bool decided = false;

        System.Action<List<RelicSO>> setOptions = null;

        System.Action rerollHandler = null;
        rerollHandler = () => {
            if (decided) return;
            if (run.Consumables.RerollRelic <= 0) return;

            run.Consumables.RerollRelic--;
            options = draftSystem.GenerateRelicOptions(rng, 3);

            setOptions?.Invoke(options);
            draftView.SetRerollInteractable(run.Consumables.RerollRelic > 0);
        };
        draftView.OnRerollRequested += rerollHandler;

        yield return draftView.ShowRelicsSlideInEx(
            options,
            onPick: (picked) => { run.Relics.Add(picked); picked.OnAcquire(run); relicBar?.Refresh(run.Relics); decided = true; },
            onSkip: () => { decided = true; },
            setup: (set, enableReroll) => {
                setOptions = set;
                enableReroll(run.Consumables.RerollRelic > 0); // ★ ドラフトを開くたびに正しく初期化
            }
        );

        draftView.OnRerollRequested -= rerollHandler;
    }


    void ShowPreviewBoard()
    {
        // ★ メインrngを進めないため、フロア依存の擬似シードで一時rngを作る
        //   好きな式でOK：フロアが同じなら毎回同じプレビューになる
        int previewSeed = run.Seed ^ (run.Floor * 7919);
        var tempRng = new System.Random(previewSeed);

        var(preview, uidPreview) = spinSystem.SpinDeal(run.Bag, tempRng);
        run.CurrentUidGrid = uidPreview;                    // ★ ここも保存
        boardView.SetFromGrid(preview);
        lastScoreText.text = "+0";
        InitializeAndApplyCountdowns(preview); // ★ 初回からカウントが見える
    }

    // GameController.cs - CommitScoreCoroutine
    IEnumerator CommitScoreCoroutine(SymbolSO[,] grid, RunState run, ScoreBuilder score)
    {
        // --- A) まずアニメ再生（= 見た目が残っているうちに） ---
        var playedShatter = new HashSet<Vector2Int>();
        foreach (var ev in score.AnimEvents)
        {
            var view = boardView.GetCellView(ev.Pos.x, ev.Pos.y);
            if (!view) continue;

            switch (ev.Type)
            {
                case AnimEventType.Bounce:
                    yield return StartCoroutine(effectFx.Bounce(view, ev.Duration));
                    break;

                case AnimEventType.Shatter:
                    if (!playedShatter.Add(ev.Pos)) break; // 同じセルは1回だけ
                    effectFx.Shatter(view, ev.Duration);   // フェード＆揺れを非同期待ちで
                    break;
            }
        }

        // ほんの少し待つとシャッター演出が乗りやすい（任意）
        if (playedShatter.Count > 0) yield return new WaitForSeconds(0.1f);

        // --- B) 破壊反映 + Bag から1枚だけ除去 + 履歴 ---
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

            run.DestroyHistory.Add(new DestroyEntry
            {
                Floor = run.Floor,
                SpinIndex = run.TotalSpins,
                Symbol = victim,
                PosX = pos.x,
                PosY = pos.y,
                Cause = req.Meta?.Cause ?? "Unknown"
            });
        }

        // --- C) 盤面を初めてクリア描画（ここで見た目が消える） ---
        if (score.SpawnQueue != null && score.SpawnQueue.Count > 0)
        {
            // 空マスのリストを作る（Owner優先に使ったら更新する）
            var empties = new List<Vector2Int>();
            int W = grid.GetLength(0), H = grid.GetLength(1);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (grid[x, y] == null)
                        empties.Add(new Vector2Int(x, y));

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
