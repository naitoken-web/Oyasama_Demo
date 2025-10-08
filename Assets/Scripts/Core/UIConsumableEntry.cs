// UIConsumableEntry.csÅiíuä∑Åj
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConsumableEntry : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text desc;
    [SerializeField] TMP_Text countText;
    [SerializeField] Button useButton;

    ConsumableSO so;

    public void Bind(ConsumableSO so, int count, System.Action<ConsumableSO> onUse)
    {
        this.so = so;
        if (icon) icon.sprite = so?.Icon;
        if (label) label.text = so?.DisplayName ?? "-";
        if (desc) desc.text = so?.Description ?? "";
        if (countText) countText.text = $"x{count}";
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() => onUse?.Invoke(this.so));
        useButton.interactable = count > 0;
    }

    public void SetCount(int count)
    {
        if (countText) countText.text = $"x{count}";
        if (useButton) useButton.interactable = count > 0;
    }
}
