// TitleController.cs（差分）
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] TMP_Text difficultyText;

    [Header("Config")]
    [SerializeField] DifficultyConfigSO difficultyConfig;

    const string KeyUnlocked = "UnlockedDifficultyMax";
    const string KeyLastSel = "LastSelectedDifficulty";

    int unlockedMax;
    int selected;

    void Awake()
    {
        // Config未設定ガード
        int configMax = Mathf.Max(1, difficultyConfig != null ? difficultyConfig.Count : 1);

        // セーブから解放上限を取得し、Config件数に丸める
        unlockedMax = PlayerPrefs.GetInt(KeyUnlocked, 1);
        unlockedMax = Mathf.Clamp(unlockedMax, 1, configMax);

        // 前回選択値を復元しつつ丸める
        selected = Mathf.Clamp(PlayerPrefs.GetInt(KeyLastSel, 1), 1, unlockedMax);

        RefreshUI();

        leftButton.onClick.AddListener(() => { if (selected > 1) { selected--; RefreshUI(); } });
        rightButton.onClick.AddListener(() => { if (selected < unlockedMax) { selected++; RefreshUI(); } });

        startButton.onClick.AddListener(() => {
            GameBootOptions.SelectedDifficulty = selected;
            PlayerPrefs.SetInt(KeyLastSel, selected);
            PlayerPrefs.Save();
            SceneLoader.LoadGame();
        });
    }

    void RefreshUI()
    {
        if (difficultyConfig != null && difficultyConfig.Count > 0)
        {
            var entry = difficultyConfig.GetByDifficulty1Based(selected);
            string name = entry != null && !string.IsNullOrEmpty(entry.DisplayName)
                        ? entry.DisplayName
                        : $"{selected}F";
            difficultyText.text = name;
        }
        else
        {
            difficultyText.text = $"{selected}F";
        }

        leftButton.interactable = selected > 1;
        rightButton.interactable = selected < unlockedMax;
    }
}
