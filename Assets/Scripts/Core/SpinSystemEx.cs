// Bag.cs に追加してもよいが、Spin 起点の方が見通しが良い
// SpinSystem.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Lucklike/SpinSystemEx")]
public class SpinSystemEx : SpinSystem
{
    public (SymbolSO[,], int[,]) SpinDeal(Bag bag, System.Random rng)
    {
        int total = Width * Height;

        // 1) プール作成：バッグの実個数をそのままコピー
        var items = new List<BagItem>(bag.Items); // BagItem のコピー

        // 2) もし持ち札が盤面より多い場合は total 枚だけ無作為抽出
        if (items.Count > total)
        {
            // フィッシャー–イェーツでシャッフル後に先頭 total を採用
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
            items = items.GetRange(0, total);
        }

        // 3) 空白を“不足分だけ”追加（＝固定個数）
        int empties = Mathf.Max(0, total - items.Count);
        for (int i = 0; i < empties; i++)
        {
            items.Add(new BagItem { Uid = 0, Symbol = null }); // 空白分の BagItem（Uid=0）
        }

        // 4) もう一度シャッフルして盤面へ「配る」（非復元）
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }

        var grid = new SymbolSO[Width, Height];
        var uidGrid = new int[Width, Height];
        int k = 0;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                
                var bi = items[k++];            // BagItem（Uid, Symbol）
                grid[x, y] = bi.Symbol;           // 表示用シンボル
                uidGrid[x, y] = bi.Uid;              // 個体 UID（同一スピン内で重複し得る）
            }

        return (grid, uidGrid);
    }
}