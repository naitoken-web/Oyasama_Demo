// ConsumableSO.cs（拡張余地を残すためSO化。必要最低限）
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/Consumable")]
public class ConsumableSO : ScriptableObject
{
    public ConsumableType Type;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;
}
