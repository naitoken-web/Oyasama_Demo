// ICountdownEffect.cs（置き換え）
using UnityEngine;

public interface ICountdownEffect
{
    int GetCountdownByUid(int uid, RunState run);
    void EnsureInitializedByUid(int uid, RunState run, Vector2Int pos); // posは演出/ログ用に渡せる
    void Step(RunState run); // 1 減らし、0到達で効果発火（Bag/盤面変更は PendingBoardOps へ）
}
