using UnityEngine;

[CreateAssetMenu(fileName = "Spin", menuName = "Scriptable Objects/SpinSystem")]
public class SpinSystem : ScriptableObject
{
    public int Width = 5, Height = 4;
    protected int[,] fxUidGrid;

    public SymbolSO[,] Spin(Bag bag, System.Random rng)
    {
        var grid = new SymbolSO[Width, Height];
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                grid[x, y] = WeightedPick(bag, rng); // Bag.WeightedPick → WeightedPick(bag, rng)に修正
        return grid;
    }

    // BagからランダムにWeightedPickするメソッドを追加
    private SymbolSO WeightedPick(Bag bag, System.Random rng)
    {
        var items = bag.Items;
        float totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.Symbol.Weight;
        }
        float pick = (float)rng.NextDouble() * totalWeight;
        float cumulative = 0f;
        foreach (var item in items)
        {
            cumulative += item.Symbol.Weight;
            if (pick <= cumulative)
                return item.Symbol;
        }
        return items.Count > 0 ? items[items.Count - 1].Symbol : null;
    }
}