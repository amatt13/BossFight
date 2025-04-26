using System;
using System.Collections.Generic;

namespace BossFight.Models
{
    public class LootDropTracker
        {
            private Dictionary<Player, List<Tuple<ILootItem, int>>> _entires = new();

            public LootDropTracker() {}

            public void Add(Player pPlayer, Tuple<ILootItem, int> pLootItem)
            {
                if (_entires.TryGetValue(pPlayer, out List<Tuple<ILootItem, int>> obtainedLoot))
                {
                    obtainedLoot.Add(pLootItem);
                }
                else
                {
                    _entires[pPlayer] = new List<Tuple<ILootItem, int>>{ pLootItem };
                }
            }

            public List<Tuple<ILootItem, int>> this[Player pPlayer]
            {
                get
                {
                    return _entires[pPlayer];
                }
                set
                {
                    _entires[pPlayer] = value;
                }
            }

            public Dictionary<Player, List<Tuple<ILootItem, int>>>.KeyCollection.Enumerator GetEnumerator()
            {
                return _entires.Keys.GetEnumerator();
            }
        }
}
