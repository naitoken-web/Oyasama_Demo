// BoardSnapshot.cs
using UnityEngine;
using System.Collections.Generic;
public class BoardSnapshot
{
    public readonly int Width, Height;
    public readonly SymbolSO[,] Grid;
    public BoardSnapshot(SymbolSO[,] grid)
    {
        Grid = grid; Width = grid.GetLength(0); Height =
grid.GetLength(1);
    }
    public IEnumerable<Vector2Int> Neighbors4(Vector2Int p)
    {
        var d = new[] { new Vector2Int(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
        foreach (var v in d) { var q = p + v; if (q.x >= 0 && q.y >= 0 && q.x < Width && q.y < Height) yield return q; }
    }
    public IEnumerable<Vector2Int> Neighbors8(Vector2Int p)
    {
        for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue; var q = new Vector2Int(p.x + dx, p.y + dy);
                if (q.x >= 0 && q.y >= 0 && q.x < Width && q.y < Height) yield return q;
            }
    }
}
