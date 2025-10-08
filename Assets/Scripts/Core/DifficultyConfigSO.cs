// DifficultyConfigSO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/DifficultyConfig")]
public class DifficultyConfigSO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        [Tooltip("タイトルで表示する名前（例: 難易度1 / Normal / Hard）")]
        public string DisplayName = "難易度 1";

        [Tooltip("この難易度で使う家賃テーブル")]
        public RentScheduleSO RentSchedule;
    }

    [Tooltip("低い難易度から順に並べる（index=0 が 難易度1）")]
    public List<Entry> Difficulties = new();

    public int Count => Difficulties?.Count ?? 0;

    public Entry GetByDifficulty1Based(int difficulty)
    {
        int idx = Mathf.Clamp(difficulty - 1, 0, Mathf.Max(0, Count - 1));
        return Count > 0 ? Difficulties[idx] : null;
    }
}
