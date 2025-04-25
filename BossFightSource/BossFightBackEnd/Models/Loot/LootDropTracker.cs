using System.Collections.Generic;

namespace BossFight.Models
{
    public class LootDropTracker
        {
            private Dictionary<Player, List<ILootItem>> _entires = new();

            public LootDropTracker() {}

            public void Add(Player pPlayer, ILootItem pLootItem)
            {
                if (_entires.TryGetValue(pPlayer, out List<ILootItem> obtainedLoot))
                {
                    obtainedLoot.Add(pLootItem);
                }
                else
                {
                    _entires[pPlayer] = new List<ILootItem>{ pLootItem };
                }
            }

            public List<ILootItem> this[Player pPlayer]
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

            public Dictionary<Player, List<ILootItem>>.KeyCollection.Enumerator GetEnumerator()
            {
                return _entires.Keys.GetEnumerator();
            }
        }
}
