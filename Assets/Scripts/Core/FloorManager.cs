using UnityEngine.UI;

public class FloorManager
{
    readonly HudPresenter hudPresenter;
    readonly UIRelicBar relicBar;
    readonly Button spinButton;

    public FloorManager(HudPresenter hudPresenter, UIRelicBar relicBar, Button spinButton)
    {
        this.hudPresenter = hudPresenter;
        this.relicBar = relicBar;
        this.spinButton = spinButton;
    }

    public void LoadFloor(RentScheduleSO schedule, RunState run, out int spinsLeft, out int rentDue)
    {
        spinsLeft = 0;
        rentDue = 0;
        if (run == null) return;

        var entry = schedule != null ? schedule.Get(run.Floor) : default;
        spinsLeft = entry.Spins;
        rentDue = entry.RequiredScore;

        hudPresenter?.UpdateHud(run, rentDue, spinsLeft, entry.Spins);

        if (relicBar) relicBar.Refresh(run.Relics);
        if (spinButton) spinButton.interactable = true;
    }
}
