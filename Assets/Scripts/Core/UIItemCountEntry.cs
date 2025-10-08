// UIItemCountEntry.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemCountEntry : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text countText;
    [SerializeField] Button mainButton;

    object payload; // SymbolSO Ç‹ÇΩÇÕ RelicSO Çï€éù

    public void Bind(Sprite sp, string name, int count, object payload, System.Action<object> onClick, bool interactable)
    {
        this.payload = payload;
        if (icon) icon.sprite = sp;
        if (label) label.text = name;
        if (countText) countText.text = $"x{count}";
        if (mainButton)
        {
            mainButton.interactable = interactable;
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => onClick?.Invoke(this.payload));
        }
    }

    public void SetCount(int count)
    {
        if (countText) countText.text = $"x{count}";
    }
}
