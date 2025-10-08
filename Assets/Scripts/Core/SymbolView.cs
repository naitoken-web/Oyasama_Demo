// SymbolView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SymbolView : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] Image icon;
    [SerializeField] TMP_Text sub;

    // 見た目だけ動かす用のルート。未設定なら自分自身を使う
    [SerializeField] RectTransform visualRoot;

    [Header("Countdown (optional)")]
    [Tooltip("破壊までの残りターンなどを表示するテキスト。未指定なら無視されます。")]
    [SerializeField] TMP_Text countDownText;   // ★ これをヒエラルキーに追加して割り当て

    RectTransform rt;
    CanvasGroup cg;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        if (!visualRoot) visualRoot = rt;   // フォールバック
        ResetVisual();

        // カウントダウンは初期非表示
        if (countDownText) countDownText.gameObject.SetActive(false);
    }

    void ResetVisual()
    {
        if (visualRoot)
        {
            visualRoot.localScale = Vector3.one;   // バウンス解除
            // 位置は触らない（GridLayout と干渉回避）
        }
        if (cg) cg.alpha = 1f;                     // フェード解除
    }

    public void Bind(Sprite sp, string subText = "")
    {
        ResetVisual();                              // 表示初期化（位置は維持）
        if (icon)
        {
            icon.sprite = sp;
            icon.enabled = (sp != null);
        }
        if (sub) sub.text = subText ?? "";
        // Bind ではカウントダウンは触らない（外部から SetCountdown を呼ぶ前提）
    }

    public void Clear()
    {
        ResetVisual();                              // 表示初期化（位置は維持）
        if (icon)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if (sub) sub.text = "";

        // クリア時はカウントダウンも消す
        if (countDownText) countDownText.gameObject.SetActive(false);
    }

    // Animator から参照しやすいよう公開
    public RectTransform VisualRoot => visualRoot ? visualRoot : rt;

    // ===== Countdown API =====

    /// <summary>
    /// 残りターンを表示します。turns <= 0 なら非表示。
    /// </summary>
    public void SetCountdown(int turns)
    {
        if (!countDownText) return;

        if (turns <= 0)
        {
            countDownText.gameObject.SetActive(false);
            return;
        }
        countDownText.gameObject.SetActive(true);
        countDownText.text = turns.ToString();
    }
}
