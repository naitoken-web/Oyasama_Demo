// UIDraftCard.cs（置換/拡張）
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIDraftCard : MonoBehaviour
{
    [Header("Wires")]
    [SerializeField] Image background;        // ★ 追加：背景
    [SerializeField] Image icon;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text desc;           // ★ 追加：説明
    [SerializeField] Button pickButton;

    [Header("Style")]
    [SerializeField] DraftCardStyleSO style;  // ★ 追加：レアリティ→見た目

    public void Bind(Sprite sp, string name, string description, Action onPick)
    {
        if (icon) icon.sprite = sp;
        if (title) title.text = name;
        if (desc) desc.text = string.IsNullOrEmpty(description) ? "" : RichTextIconFormatter.Format(description, size:"170%", voffset:10); ;

        pickButton.onClick.RemoveAllListeners();
        if (onPick != null) pickButton.onClick.AddListener(() => onPick.Invoke());
    }

    public void ApplyRarity(Rarity r)
    {       // ★ レアリティ適用
        if (!style) return;
        var e = style.Get(r);
        if (e == null) return;

        if (background)
        {
            if (e.Background) { background.sprite = e.Background; background.enabled = true; }
            background.color = e.Background ? Color.white : e.FallbackColor;
        }
    }

    public void SetOnPick(System.Action onPick)
    {
        pickButton.onClick.RemoveAllListeners();
        pickButton.onClick.AddListener(() => onPick?.Invoke());
    }
}
