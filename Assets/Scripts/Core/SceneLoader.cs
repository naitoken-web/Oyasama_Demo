// SceneLoader.cs
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string Title = "TitleScene";
    public const string Game = "GameScene";

    public static void LoadTitle() => SceneManager.LoadScene(Title);
    public static void LoadGame() => SceneManager.LoadScene(Game);
}
