// BoardBinder.cs（BoardViewに実データを流し込む）
using UnityEngine;
public class BoardBinder : MonoBehaviour
{
    /*
    [SerializeField] BoardView view;
    [SerializeField] SpinSystem spinSystem;
    [SerializeField] Bag bag;
    [SerializeField] int seed = 0;
    void Start()
    {
        var rng = new System.Random(seed);
        var grid = spinSystem.Spin(bag, rng);
        // grid から Sprite 配列を作ってバインド
        var sprites = new Sprite[spinSystem.Width * spinSystem.Height];
        int i = 0; for (int y = 0; y < spinSystem.Height; y++)
            for (int x = 0; x < spinSystem.Width; x++) sprites[i++] = grid[x, y]?.Icon;
        view.FillDummy(sprites); // 今は subText固定（+1）。後で SymbolView.Bind を拡張
    }*/
}
