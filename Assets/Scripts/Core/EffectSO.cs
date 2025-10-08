// EffectSO.csi’ŠÛj
using UnityEngine;
public abstract class EffectSO : ScriptableObject
{
    public abstract void Evaluate(SymbolContext ctx, BoardSnapshot board, ScoreBuilder score);
}

public class SymbolContext
{
    public Vector2Int Pos;
    public SymbolSO Self;
}

public partial class ScoreBuilder
{
    System.Text.StringBuilder _log = new();
    public int Total { get; private set; }
    public void Add(int amount, string reason)
    {
        Total += amount; _log.AppendLine($"+{amount}{ reason}"); }
        public string BuildLog() => _log.ToString();
    }