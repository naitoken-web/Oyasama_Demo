using System.Collections;
using System.Collections.Generic;

public class DraftPresenter
{
    readonly DraftSystem draftSystem;
    readonly UIDraftView draftView;
    readonly UIRelicBar relicBar;

    public DraftPresenter(DraftSystem draftSystem, UIDraftView draftView, UIRelicBar relicBar)
    {
        this.draftSystem = draftSystem;
        this.draftView = draftView;
        this.relicBar = relicBar;
    }

    public IEnumerator PresentSymbolDraft(RunState run, System.Random rng)
    {
        var options = draftSystem.GenerateSymbolOptions(rng, 3);
        bool decided = false;

        System.Action<List<SymbolSO>> setOptions = null;
        System.Action rerollHandler = null;
        rerollHandler = () =>
        {
            if (decided) return;
            if (run.Consumables.RerollItem <= 0) return;

            run.Consumables.RerollItem--;
            options = draftSystem.GenerateSymbolOptions(rng, 3);
            setOptions?.Invoke(options);
            draftView.SetRerollInteractable(run.Consumables.RerollItem > 0);
        };
        draftView.OnRerollRequested += rerollHandler;

        yield return draftView.ShowSymbolsSlideInEx(
            options,
            onPick: (picked) =>
            {
                run.Bag.Add(picked);
                decided = true;
            },
            onSkip: () => { decided = true; },
            setup: (set, enableReroll) =>
            {
                setOptions = set;
                enableReroll(run.Consumables.RerollItem > 0);
            }
        );

        draftView.OnRerollRequested -= rerollHandler;
    }

    public IEnumerator PresentRelicDraft(RunState run, System.Random rng)
    {
        var options = draftSystem.GenerateRelicOptions(rng, 3);
        bool decided = false;

        System.Action<List<RelicSO>> setOptions = null;
        System.Action rerollHandler = null;
        rerollHandler = () =>
        {
            if (decided) return;
            if (run.Consumables.RerollRelic <= 0) return;

            run.Consumables.RerollRelic--;
            options = draftSystem.GenerateRelicOptions(rng, 3);
            setOptions?.Invoke(options);
            draftView.SetRerollInteractable(run.Consumables.RerollRelic > 0);
        };
        draftView.OnRerollRequested += rerollHandler;

        yield return draftView.ShowRelicsSlideInEx(
            options,
            onPick: (picked) =>
            {
                run.Relics.Add(picked);
                picked.OnAcquire(run);
                relicBar?.Refresh(run.Relics);
                decided = true;
            },
            onSkip: () => { decided = true; },
            setup: (set, enableReroll) =>
            {
                setOptions = set;
                enableReroll(run.Consumables.RerollRelic > 0);
            }
        );

        draftView.OnRerollRequested -= rerollHandler;
    }
}
