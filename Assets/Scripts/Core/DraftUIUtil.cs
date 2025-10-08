// DraftUIUtil.cs（新規の小ユーティリティ）
using System.Collections.Generic;
using UnityEngine;

public static class DraftUIUtil
{
    public static List<CellGain> ToGains(IReadOnlyList<(Vector2Int pos, int amount)> src)
    {
        var list = new List<CellGain>(src.Count);
        for (int i = 0; i < src.Count; i++)
        {
            var (pos, amount) = src[i];
            list.Add(new CellGain(pos.x, pos.y, amount));
            // もし CellGain が (x,y,amount) ではなくプロパティ型なら↓に合わせてください
            // list.Add(new CellGain { X=pos.x, Y=pos.y, Amount=amount });
        }
        return list;
    }
}
