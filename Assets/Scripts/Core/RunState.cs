// RunState.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lucklike/RunState")]
public class RunState : ScriptableObject
{
    public int Floor = 1;
    public int Coins = 0;              // 今回はコスト無視だが将来拡張に残す
    public Bag Bag;                    // 所持シンボル
    public List<RelicSO> Relics = new();
    public int Seed = 0;               // 再現性確保

    public ConsumableCounts Consumables = new();
    public List<RarityBoostDraftEffect> ActiveRarityBoosts = new(); // ★ドラフト時に参照
    public Dictionary<ScriptableObject, Dictionary<Vector2Int, int>> countdownByPos
        = new Dictionary<ScriptableObject, Dictionary<Vector2Int, int>>();
    public List<PendingBoardOp> PendingBoardOps = new();

    public Dictionary<ScriptableObject, Dictionary<int, int>> countdownByUid = new();
    public int[,] CurrentUidGrid; // 直近スピンの “位置→uid”

    // ★ 破壊履歴
    public List<DestroyEntry> DestroyHistory = new();

    // 任意：総スピン通番（加算しておく）
    public int TotalSpins = 0;

    [System.Serializable]
    public struct PendingBoardOp
    {
        public string Op;     // "DestroySelf" など
        public int Uid;       // 対象個体
    }

    void OnEnable()
    {
        countdownByUid ??= new();
        PendingBoardOps ??= new();
    }
}

[System.Serializable]
public class ConsumableCounts
{
    public int RemoveToken = 0;
    public int RerollItem = 0;
    public int RerollRelic = 0;
}

[System.Serializable]
public class DestroyEntry
{
    public int Floor;
    public int SpinIndex;   // 通しスピン番号（なければFloor内カウントでも）
    public SymbolSO Symbol;
    public int PosX, PosY;
    public string Cause;    // "AdjCoin" 等
}