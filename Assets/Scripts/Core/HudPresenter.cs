using TMPro;
using UnityEngine;

public class HudPresenter
{
    readonly TMP_Text coinsText;
    readonly TMP_Text floorText;
    readonly TMP_Text spinsText;
    readonly TMP_Text lastScoreText;

    public HudPresenter(TMP_Text coinsText, TMP_Text floorText, TMP_Text spinsText, TMP_Text lastScoreText)
    {
        this.coinsText = coinsText;
        this.floorText = floorText;
        this.spinsText = spinsText;
        this.lastScoreText = lastScoreText;
    }

    public void UpdateHud(RunState run, int rentDue, int spinsLeft, int totalSpins)
    {
        if (run == null) return;
        if (coinsText) coinsText.text = $"Coins: {run.Coins} / {rentDue}";
        if (floorText) floorText.text = $"Stage: {run.Floor}";
        if (spinsText)
        {
            if (totalSpins <= 0) spinsText.text = $"Turn: {spinsLeft}";
            else spinsText.text = $"Turn: {spinsLeft} / {totalSpins}";
        }
    }

    public void SetLastScore(int score)
    {
        if (lastScoreText) lastScoreText.text = $"+{score}";
    }

    public void SetLastScoreRaw(string text)
    {
        if (lastScoreText) lastScoreText.text = text;
    }

    public void AppendGameOverMessage()
    {
        if (!lastScoreText) return;
        lastScoreText.text += "\n<color=#f55>GAME OVER</color>";
    }
}
