using System;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePresenter
{
    readonly GameObject gameOverPanel;
    readonly Button backToTitleButton;
    readonly GameObject victoryPanel;
    readonly Button backToTitleOnWinButton;
    readonly HudPresenter hudPresenter;

    public EndGamePresenter(GameObject gameOverPanel, Button backToTitleButton, GameObject victoryPanel, Button backToTitleOnWinButton, HudPresenter hudPresenter)
    {
        this.gameOverPanel = gameOverPanel;
        this.backToTitleButton = backToTitleButton;
        this.victoryPanel = victoryPanel;
        this.backToTitleOnWinButton = backToTitleOnWinButton;
        this.hudPresenter = hudPresenter;
    }

    public void HideAll()
    {
        if (victoryPanel) victoryPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(Action onBackToTitle)
    {
        hudPresenter?.AppendGameOverMessage();
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (backToTitleButton != null)
        {
            backToTitleButton.onClick.RemoveAllListeners();
            backToTitleButton.onClick.AddListener(() => onBackToTitle?.Invoke());
        }
    }

    public void ShowVictory(Action onBackToTitle)
    {
        if (victoryPanel) victoryPanel.SetActive(true);
        if (backToTitleOnWinButton != null)
        {
            backToTitleOnWinButton.onClick.RemoveAllListeners();
            backToTitleOnWinButton.onClick.AddListener(() => onBackToTitle?.Invoke());
        }
    }
}
