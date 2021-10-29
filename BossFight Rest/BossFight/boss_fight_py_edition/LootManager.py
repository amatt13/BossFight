import math
import random
from source.games.boss_fight import monster, player, lootItem
from source.games.boss_fight.LootEntry import LootEntry
from source.misc import util


class LootManager:
    def __init__(self, game_manager):
        self.game_manager = game_manager
        self.loot_entry_list: list[LootEntry] = []

    def select_loot_to_drop_from_dead_monster(self, dead_monster: monster.Monster) -> []:
        loot_list = []
        all_possible_loot = dead_monster.get_item_drops()
        for dropped_loot in all_possible_loot:
            roll: float = random.randint(1, 100) / 100
            if roll <= dropped_loot.loot_drop_chance:
                loot_list.append(dropped_loot)
        return loot_list

    async def split_loot_from_dead_monster(self, dead_monster: monster.Monster, killer_player_id: str):
        player_updates = self.distribute_gold_to_players(dead_monster)
        sorted_by_dmg_dealt = sorted(player_updates, key=lambda x: x.damage_dealt_by_player, reverse=True)
        for entry in sorted_by_dmg_dealt:
            p: player.Player = self.game_manager.find_player_by_id(entry.player_id)
            p.gold += entry.gold_earned
            entry.set_player(p)
        length_of_longest_name = max(len(x.player_object.name) for x in sorted_by_dmg_dealt)
        dropped_loot: list[lootItem] = self.select_loot_to_drop_from_dead_monster(dead_monster)
        who_won_what_loot_list: list[tuple[player.Player, lootItem]] = []
        for loot in dropped_loot:
            roll: float = random.randint(1, 100) / 100
            progress = 0.0
            for entry in player_updates:
                progress += entry.relative_damage_dealt_by_player
                if progress >= roll:
                    entry.player_object.add_loot(loot)
                    who_won_what_loot_list.append((entry.player_object, loot))
                    break
        boss_kill_message = ""
        if dead_monster.boss_monster:
            boss_kill_message = "BOSS KILL\n"
        loot_message = f"""{boss_kill_message}The monster has been slain!
<@!{killer_player_id}> dealt the killing blow to {dead_monster.name}
```python\nName - damage dealt - gold earned
""" + "\n".join(f"{x.player_object.name.ljust(length_of_longest_name, '.')} - {x.damage_dealt_by_player} damage - {x.gold_earned} gold" for x in sorted_by_dmg_dealt)
        loot_message += "```"
        if len(who_won_what_loot_list) > 0:
            loot_message += "Dropped loot!"
            loot_message += "```python\n"
            for x in who_won_what_loot_list:
                p: player.Player = x[0]
                l: lootItem.LootItem = x[1]
                loot_message += f"\n{p.name} won {l.loot_name}"
                if p.loot_is_in_auto_sell_list(l.loot_id):
                    loot_message += f" (auto-sold for {l.get_sell_price()} gold)"
            loot_message += "```"
        await util.write_msg_to_channel(loot_message, util.boss_loot_channel)

    def add_to_loot_entry_list(self, loot_entry: LootEntry):
        self.loot_entry_list.append(loot_entry)
        total_damage_dealt_to_monster = sum(d.damage_dealt_by_player for d in self.loot_entry_list)
        # recalculate relative_damage_dealt_by_player
        for entry in self.loot_entry_list:
            entry.relative_damage_dealt_by_player = entry.damage_dealt_by_player / total_damage_dealt_to_monster

    def distribute_gold_to_players(self, dead_monster: monster.Monster) -> list[LootEntry]:
        for player_id in dead_monster.damage_tracker:
            damage_dealt_by_player = dead_monster.damage_tracker[player_id]
            gold_earned = math.floor(1 + (damage_dealt_by_player * dead_monster.level * 0.50) / 10)
            if dead_monster.boss_monster:
                gold_earned = math.floor(gold_earned * 1.2)
            self.add_to_loot_entry_list(LootEntry(player_id, damage_dealt_by_player, gold_earned))
        return self.loot_entry_list


