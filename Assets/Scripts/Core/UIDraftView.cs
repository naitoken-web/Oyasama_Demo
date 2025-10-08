// UIDraftView.cs（追加・置換）
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDraftView : MonoBehaviour
{
    [SerializeField] GameObject root;         // パネルのルート
    [SerializeField] RectTransform panel;     // 動かす対象
    [SerializeField] CanvasGroup cg;          // 透明度管理（無ければAddComponentでも可）
    [SerializeField] UIDraftCard[] slots;     // 3枚
    [SerializeField] Button skipButton;

    [Header("Slide FX")]
    [SerializeField] Vector2 offscreenOffset = new Vector2(0, -600f); // 画面下から
    [SerializeField, Min(0.05f)] float slideDuration = 0.25f;
    [SerializeField] AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] Button rerollButton;
    public event System.Action OnRerollRequested; // GameControllerがハンドル

    Vector2 panelHome;

    void Awake()
    {
        if (panel == null) panel = root.GetComponent<RectTransform>();
        if (cg == null) cg = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        cg.interactable = true;   // ← クリックをブロックしない
        cg.blocksRaycasts = true;   // ← パネル内のボタンがクリック可
        root.SetActive(false);
        cg.alpha = 0f;
    }

    void WireReroll(bool interactable)
    {
        if (!rerollButton) return;                  // ← インスペクタ未設定ガード
        rerollButton.gameObject.SetActive(true);    // 常に見せる（状態は interactable で制御）
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(() => OnRerollRequested?.Invoke());
        rerollButton.interactable = interactable;
    }

    IEnumerator SlideIn()
    {
        root.SetActive(true);
        panel.anchoredPosition = panelHome + offscreenOffset;
        cg.alpha = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            float e = slideEase.Evaluate(Mathf.Clamp01(t));
            panel.anchoredPosition = Vector2.Lerp(panelHome + offscreenOffset, panelHome, e);
            cg.alpha = e;
            yield return null;
        }
        panel.anchoredPosition = panelHome;
        cg.alpha = 1f;
    }

    IEnumerator SlideOut()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            float e = slideEase.Evaluate(Mathf.Clamp01(t));
            panel.anchoredPosition = Vector2.Lerp(panelHome, panelHome + offscreenOffset, e);
            cg.alpha = 1f - e;
            yield return null;
        }
        cg.alpha = 0f;
        root.SetActive(false);
    }

    // --- 遺物ドラフト：同様に ---
    public IEnumerator ShowRelicsSlideInEx(
        List<RelicSO> options,
        Action<RelicSO> onPick,
        Action onSkip,
        Action<Action<List<RelicSO>>, Action<bool>> setup
    )
    {
        List<RelicSO> current = new(options);

        void BindCards()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var r = current[i];
                slots[i].Bind(r.Icon, string.IsNullOrEmpty(r.Id) ? "Relic" : r.Id,
                              r.Description, null);
                slots[i].ApplyRarity(r.Rarity); // ★ レアリティ背景
            }
        }

        void SetOptions(List<RelicSO> newOptions) { current = new(newOptions); BindCards(); }
        void EnableReroll(bool on) { WireReroll(on); }

        WireReroll(false);          // 先にリスナー張って見せるだけ
        setup?.Invoke(SetOptions, EnableReroll);

        BindCards();
        yield return SlideIn();

        bool decided = false;
        for (int i = 0; i < slots.Length; i++)
        {
            int idx = i;
            slots[i].SetOnPick(() => {
                if (decided) return;
                decided = true;
                onPick?.Invoke(current[idx]);
                StartCoroutine(SlideOut());
            });
        }
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() => {
            if (decided) return;
            decided = true;
            onSkip?.Invoke();
            StartCoroutine(SlideOut());
        });

        while (!decided) yield return null;
        yield return new WaitForSeconds(slideDuration);
    }

    // UIDraftView.cs（新規：ShowSymbolsSlideInEx）
    // UIDraftView.cs（差し替え）
    public IEnumerator ShowSymbolsSlideInEx(
        List<SymbolSO> options,
        Action<SymbolSO> onPick,
        Action onSkip,
        Action<Action<List<SymbolSO>>, Action<bool>> setup
    )
    {
        List<SymbolSO> current = new(options);

        void BindCards()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var s = current[i];
                slots[i].Bind(s.Icon, string.IsNullOrEmpty(s.Id) ? "Symbol" : s.Id,
                              s.Description, null);
                slots[i].ApplyRarity(s.Rarity); // ★ レアリティ背景
            }
        }

        void SetOptions(List<SymbolSO> newOptions) { current = new(newOptions); BindCards(); }
        void EnableReroll(bool on) { WireReroll(on); }   // ← 状態変更はここだけで行う

        // ★ 先に“見た目の初期化”だけ（リスナー配線＆表示）。ここで disable しない
        WireReroll(false);          // Listenersを張る＆表示はする（interactableだけfalse）
                                    // ★ 呼び出し側にフックを渡す → ここで所持数に応じて enableReroll(true/false) を呼んでもらう
        setup?.Invoke(SetOptions, EnableReroll);

        BindCards();
        yield return SlideIn();

        bool decided = false;
        for (int i = 0; i < slots.Length; i++)
        {
            int idx = i;
            slots[i].SetOnPick(() => {
                if (decided) return;
                decided = true;
                onPick?.Invoke(current[idx]); // ★ current から確定
                StartCoroutine(SlideOut());
            });
        }
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() => {
            if (decided) return;
            decided = true;
            onSkip?.Invoke();
            StartCoroutine(SlideOut());
        });

        while (!decided) yield return null;
        yield return new WaitForSeconds(slideDuration);
    }


    // Rerollボタンだけ切り替えたい時のヘルパ
    public void SetRerollInteractable(bool on)
    {
        if (rerollButton) rerollButton.interactable = on;
    }


}
