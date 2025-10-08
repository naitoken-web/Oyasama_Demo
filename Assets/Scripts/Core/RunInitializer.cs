using UnityEngine;

public class RunInitializer
{
    readonly RunState run;
    readonly StartBagSO startBag;

    public RunInitializer(RunState run, StartBagSO startBag)
    {
        this.run = run;
        this.startBag = startBag;
    }

    public void InitializeRun(int seed)
    {
        if (run == null) return;

        run.Seed = seed;
        run.Coins = 0;
        run.Floor = 1;
        run.Relics.Clear();
        run.DestroyHistory.Clear();
        run.ActiveRarityBoosts.Clear();

        run.Consumables = new ConsumableCounts
        {
            RemoveToken = 1,
            RerollItem = 1,
            RerollRelic = 1
        };

        run.Bag = ScriptableObject.CreateInstance<Bag>();
        if (startBag?.Items != null)
        {
            foreach (var s in startBag.Items)
            {
                run.Bag.Add(s);
            }
        }
    }
}
