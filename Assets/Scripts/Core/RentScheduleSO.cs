using UnityEngine;
using System;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Lucklike/RentSchedule")]
public class RentScheduleSO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public int Floor = 1; public int RequiredScore = 25; public int
Spins = 5;
    }
    public List<Entry> Entries = new()
    {
        new Entry { Floor = 1, RequiredScore = 25, Spins = 5 },
        new
    Entry
        { Floor = 2, RequiredScore = 50, Spins = 5 },
        new Entry { Floor = 3, RequiredScore = 100, Spins = 6 }
    };
    public Entry Get(int floor) => Entries.Find(e => e.Floor == floor) ?? Entries[^1];
}