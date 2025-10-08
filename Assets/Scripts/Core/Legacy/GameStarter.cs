// GameStarter.cs
using UnityEngine;
public class GameStarter : MonoBehaviour
{
    [SerializeField] BoardView boardView;
    [SerializeField] Sprite[] dummySprites; // 何でもOK（仮アイコン）
    void Start()
    {
        //boardView.FillDummy(dummySprites);
    }
}
