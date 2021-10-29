import json
import math
import random
from datetime import datetime, timedelta
import discord
from source.games.boss_fight import player, monster, statics, shopManager
from source.games.boss_fight.Event import subscribe, MONSTER_KILLED_EVENT
from source.games.boss_fight.LootManager import LootManager
from source.misc import util

global_game_manager = None
MINUTES_BETWEEN_PLAYER_HEALTH_RESTORE = 3
MINUTES_BETWEEN_PLAYER_MANA_RESTORE = 1
DIFFICULTY_UP_REACTION = "🔺"
DIFFICULTY_DOWN_REACTION = "🔻"
DIFFICULTY_TIER_DOWN_REACTION = "⏬"
BOSS_SPAWN_CHANCE_INCREMENT = 2


class GameManager:
    file_location = "PlayerManager.json"

    def __init__(self, game_manager_dict: dict):
        self.players: list[player.Player] = []
        self.difficulty: int = game_manager_dict["difficulty"]
        self.hp_timestamp: str = game_manager_dict["hp_timestamp"]
        self.mana_timestamp: str = game_manager_dict["mana_timestamp"]
        self.boss_spawn_percentage: int = game_manager_dict["boss_spawn_percentage"]
        self.prev_monster_spawn_idx: int = game_manager_dict["prev_monster_spawn_idx"]
        self.prev_boss_monster_spawn_idx: int = game_manager_dict["prev_boss_monster_spawn_idx"]
        players: dict = game_manager_dict["players"]
        for player_dict in players:
            pl = player.Player.from_dict(player_dict)
            self.players.append(pl)
        self.current_monster: monster.Monster = monster.Monster.from_dict(game_manager_dict["current_monster"])
        global global_game_manager
        global_game_manager = self
        self.setup_monster_event_handlers()

    def setup_monster_event_handlers(self):
        subscribe(MONSTER_KILLED_EVENT, self.spawn_new_monster)

    def persist_changes(self):
        MyEncoder.persist(self)

    def add_player_if_missing(self, author: discord.Member) -> player.Player:
        try:
            # Do nothing if the player exists
            existing_player = next(p for p in self.players if str(p.player_id) == str(author.id))
            return existing_player
        except StopIteration:
            new_player = player.Player(author.display_name, str(author.id))
            new_player.add_all_missing_starting_classes()
            self.players.append(new_player)
            return new_player

    def find_player_by_id(self, player_id: str) -> player.Player:
        return next(p for p in self.players if p.player_id == player_id)

    def update_players_health_and_mana(self):
        self.hp_timestamp = statics.update_players_health_and_mana_helper(self.players, self.hp_timestamp, MINUTES_BETWEEN_PLAYER_HEALTH_RESTORE, statics.regen_player_health)
        self.mana_timestamp = statics.update_players_health_and_mana_helper(self.players, self.mana_timestamp, MINUTES_BETWEEN_PLAYER_MANA_RESTORE, statics.regen_player_mana)

    def datetime_when_player_has_one_hp(self, p: player.Player) -> datetime:
        hp_timestamp = datetime.fromisoformat(self.hp_timestamp)
        remaining_hp_until_1 = 1 - p.hp
        minutes_to_wait = remaining_hp_until_1 * MINUTES_BETWEEN_PLAYER_HEALTH_RESTORE
        one_hp_time = hp_timestamp + timedelta(minutes=minutes_to_wait)
        return one_hp_time

    def should_spawn_boss_monster(self) -> bool:
        result = False
        roll = random.randint(0, 100)
        if roll <= self.boss_spawn_percentage:
            print(f"spawned boss. roll: {roll}, boss_spawn_percentage: {self.boss_spawn_percentage}")
            self.boss_spawn_percentage = 0
            result = True
        else:
            self.boss_spawn_percentage += BOSS_SPAWN_CHANCE_INCREMENT
        return result

    def get_monster_by_tier(self, level: int, spawn_boss: bool) -> monster.Monster:
        new_monster: monster.Monster
        if spawn_boss:
            while True:
                idx = random.randint(0, len(boss_monster_array) - 1)
                if idx != self.prev_boss_monster_spawn_idx:
                    new_monster = boss_monster_array[idx]()
                    self.prev_boss_monster_spawn_idx = idx
                    break
        else:
            tier = int(math.floor(level / monster.TIER_DIVIDER))
            while True:
                idx = random.randint(0, len(monster_array[tier]) - 1)
                if idx != self.prev_monster_spawn_idx:
                    new_monster = monster_array[tier][idx]()
                    self.prev_monster_spawn_idx = idx
                    break
        new_monster.set_level_and_health(level)
        return new_monster

    async def spawn_new_monster(self, data):
        dead_monster: monster.Monster = data[0]
        killer_player_id: str = data[1]
        lm = LootManager(self)
        await lm.split_loot_from_dead_monster(dead_monster, killer_player_id)
        await self.update_difficulty()
        should_spawn_boss_monster = self.should_spawn_boss_monster()
        self.current_monster = self.get_monster_by_tier(self.difficulty, should_spawn_boss_monster)
        await self.reset_reactions()
        await shopManager.update_shop()

    @staticmethod
    async def reset_reactions():
        message = await util.boss_fight_channel.fetch_message(util.boss_status_message.id)
        for r in message.reactions:
            for u in await r.users().flatten():
                if not u.bot:
                    await r.remove(u)
        # re-fetch the message
        message = await util.boss_fight_channel.fetch_message(util.boss_status_message.id)
        if len(message.reactions) != 3:
            await message.add_reaction(DIFFICULTY_UP_REACTION)
            await message.add_reaction(DIFFICULTY_DOWN_REACTION)
            await message.add_reaction(DIFFICULTY_TIER_DOWN_REACTION)

    async def update_difficulty(self):
        # TODO go one diff down if -5 and -1 tied for the lead
        message = await util.boss_fight_channel.fetch_message(util.boss_status_message.id)
        try:
            diff_up = next(up for up in message.reactions if up.emoji == DIFFICULTY_UP_REACTION).count
            diff_down = next(dw for dw in message.reactions if dw.emoji == DIFFICULTY_DOWN_REACTION).count
            diff_tier_down = next(dw for dw in message.reactions if dw.emoji == DIFFICULTY_TIER_DOWN_REACTION).count
            # a bit silly, but it works
            counts = [diff_up, diff_down, diff_tier_down]
            max_count = max(counts)
            winner_index = counts.index(max_count)
            if counts.count(max_count) == 1:
                if winner_index == 0:
                    self.difficulty += 1
                elif winner_index == 1:
                    self.difficulty -= 1
                elif diff_tier_down == 2:
                    self.difficulty -= 5

                if self.difficulty <= 0:
                    self.difficulty = 1
        except StopIteration:
            print("Could not update difficulty. Missing reactions")


def load_player_manager() -> GameManager:
    with open(GameManager.file_location) as text_file:
        all_text = text_file.read()
    player_manager_dict: dict = json.loads(all_text)
    return GameManager(player_manager_dict)


monster_array = [
    [monster.CuteGoblin, monster.Slime, monster.VillageIdiot, monster.Cripple, monster.GiantCentipede, monster.DuckSizedHorse, monster.DabbingSkeleton],  # 7
    [monster.Murloc, monster.DireRat, monster.BoBo, monster.Boar, monster.Gremlin, monster.Imp, monster.Fallen],  # 7
    [monster.GiantFrog, monster.Whelp, monster.Wolf, monster.Zombie, monster.Troglodyte, monster.Boggard, monster.Brightwing, monster.CorruptRogueArcher, monster.Cultist],  # 9
    [monster.Gladiator, monster.KillerRabbit, monster.ManAtArms, monster.Werebear, monster.Troll, monster.Lillend, monster.GorillaDire, monster.HellBovine],  # 8
    [monster.EdgeLord, monster.Lyander, monster.Bebilith, monster.Succubus, monster.CouncilMember, monster.Giant, monster.Minotaur],  # 7
    [monster.Angel, monster.GreaterDemon, monster.MinionOfDestruction, monster.Hydra]  # 4
]
boss_monster_array = [monster.NinjaBrian, monster.DoomGuy, monster.TheLichKing, monster.Deathwing, monster.Diablo]


class MyEncoder(json.JSONEncoder):
    @staticmethod
    def persist(to_encode: GameManager):
        encoded = json.dumps(to_encode, sort_keys=True, indent=4, cls=MyEncoder)
        with open(GameManager.file_location, "w") as text_file:
            text_file.write(encoded)

    def default(self, o: GameManager):
        players_list: [dict] = []
        for p in o.players:
            p_dict = p.to_dict()
            players_list.append(p_dict)
        result_dict = {
            "boss_spawn_percentage": o.boss_spawn_percentage,
            "current_monster": o.current_monster.to_dict(),
            "difficulty": o.difficulty,
            "hp_timestamp": o.hp_timestamp,
            "mana_timestamp": o.mana_timestamp,
            "players": players_list,
            "prev_monster_spawn_idx": o.prev_monster_spawn_idx,
            "prev_boss_monster_spawn_idx": o.prev_boss_monster_spawn_idx
        }
        return result_dict
