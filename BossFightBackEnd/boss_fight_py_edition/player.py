import math
import random
import source.games.boss_fight.statics as statics
import source.games.boss_fight.playerClass as playerClass
from source.games.boss_fight.AttackMessage import AttackMessage
from source.games.boss_fight.lootItem import LootItem
from source.games.boss_fight.target import Target
from source.games.boss_fight.weapon import Weapon


class Player(Target):
    def __init__(self, name: str, player_id: str, gold: int = 0, loot: list = None, weapon_id: int = 1, hp: int = 10, player_class_id: int = 0, mana: int = 10, player_class_list: list = None):
        super().__init__(hp, name, -1)
        self.player_id: str = player_id
        self.gold: int = gold
        if loot is None:
            self.loot: list = []
        else:
            self.loot: list = loot
        self.weapon_id: int = weapon_id
        self.current_player_class_id: int = player_class_id
        self.mana: int = mana
        self.bonus_magic_dmg: int = 0
        self.bonus_magic_dmg_duration: int = 0
        if player_class_list is None:
            self.player_class_list: list[playerClass.PlayerClass] = []
            self.player_class: playerClass.PlayerClass = playerClass.Cleric(self)
        else:
            self.player_class_list: [playerClass.PlayerClass] = player_class_list
            self.player_class: playerClass.PlayerClass = next(pc for pc in self.player_class_list if pc.class_id == self.current_player_class_id)
            self.level = self.player_class.level
        self.weapon = statics.find_weapon_by_weapon_id(self.weapon_id)
        self.auto_sell_list: list[int] = []

    @staticmethod
    def _from_dict_player_class_helper(player_class_dict: dict, p) -> playerClass.PlayerClass:
        result: playerClass.PlayerClass
        player_class_name = str(player_class_dict["name"]).lower()
        if player_class_name == "cleric":
            result = playerClass.Cleric.from_dict(player_class_dict, p)
        elif player_class_name == "executioner":
            result = playerClass.Executioner.from_dict(player_class_dict, p)
        elif player_class_name == "ranger":
            result = playerClass.Ranger.from_dict(player_class_dict, p)
        elif player_class_name == "hexer":
            result = playerClass.Hexer.from_dict(player_class_dict, p)
        elif player_class_name == "mage":
            result = playerClass.Mage.from_dict(player_class_dict, p)
        elif player_class_name == "barbarian":
            result = playerClass.Barbarian.from_dict(player_class_dict, p)
        elif player_class_name == "paladin":
            result = playerClass.Paladin.from_dict(player_class_dict, p)
        elif player_class_name == "monster hunter":
            result = playerClass.MonsterHunter.from_dict(player_class_dict, p)
        else:
            error_msg = "Could not convert find class with name " + player_class_name
            print(error_msg)
            raise ValueError(error_msg)
        return result

    @classmethod
    def from_dict(cls, player_dict: dict):
        p = cls("NO NAME", "-1", -1, [], 1, 10, 10)
        p.name = player_dict["name"]
        p.player_id = str(player_dict["player_id"])
        p.gold = int(player_dict["gold"])
        p.loot = player_dict["loot"]
        p.weapon_id = player_dict["weapon_id"]
        p.hp = int(player_dict["hp"])
        p.current_player_class_id = int(player_dict["current_player_class_id"])
        p.mana = int(player_dict["mana"])
        p.bonus_magic_dmg = int(player_dict["bonus_magic_dmg"])
        p.bonus_magic_dmg_duration = int(player_dict["bonus_magic_dmg_duration"])
        p.player_class_list = []
        p.auto_sell_list = player_dict["auto_sell_list"]
        if "player_class_list" in player_dict.keys():
            player_class_dictionaries: dict = player_dict["player_class_list"]
            for player_class_dict in player_class_dictionaries:
                try:
                    p.player_class_list.append(cls._from_dict_player_class_helper(player_class_dict, p))
                except ValueError as ve:
                    quit(str(ve))
        p.player_class = next(pc for pc in p.player_class_list if pc.class_id == p.current_player_class_id)
        p.level = p.player_class.level
        p.weapon = statics.find_weapon_by_weapon_id(p.weapon_id)
        return p

    def to_dict(self) -> dict:
        player_dict = {}
        player_class_list: [dict] = []
        for player_class in self.player_class_list:
            player_class_list.append(player_class.to_dict())
        player_dict["player_class_list"] = player_class_list
        player_dict["gold"] = self.gold
        player_dict["hp"] = self.hp
        player_dict["loot"] = self.loot
        player_dict["mana"] = self.mana
        player_dict["name"] = self.name
        player_dict["current_player_class_id"] = self.player_class.class_id
        player_dict["player_id"] = self.player_id
        player_dict["weapon_id"] = self.weapon.loot_id
        player_dict["bonus_magic_dmg"] = self.bonus_magic_dmg
        player_dict["bonus_magic_dmg_duration"] = self.bonus_magic_dmg_duration
        player_dict["auto_sell_list"] = self.auto_sell_list
        return player_dict

    def __str__(self):
        return self.name

    def shop_str(self, length_of_longest_player_name: int, length_of_longest_player_total_gold: int) -> str:
        gold_str = f"{self.gold:,}"
        gold_str = f"{gold_str.replace(',', '.')}".ljust(length_of_longest_player_total_gold)
        return f"{self.name.ljust(length_of_longest_player_name, '.')} {gold_str} gold"

    def get_max_hp(self) -> int:
        return self.player_class.max_hp

    def get_max_mana(self) -> int:
        return self.player_class.max_mana

    def get_level(self) -> int:
        return self.player_class.level

    def is_knocked_out(self):
        return self.hp <= 0

    def gain_xp(self, gained_xp: int, monster_level: int = None) -> playerClass.PlayerClass:
        gained_xp = statics.calc_xp_penalty(gained_xp, self.get_level(), monster_level)
        self.player_class.xp += gained_xp
        xp_needed_to_next_level = statics.xp_needed_to_next_level(self.player_class)
        if xp_needed_to_next_level <= 0:
            self.player_class.level_up()
            if xp_needed_to_next_level < 0:
                self.gain_xp(-xp_needed_to_next_level, monster_level)  # add remainder to player
        return self.player_class

    def has_enough_mana_for_ability(self, ability):
        return self.mana >= ability.mana_cost

    def get_attack_bonus(self) -> int:
        return math.floor(self.level / 2) + self.bonus_magic_dmg + self.player_class.get_attack_power_bonus()

    def get_spell_bonus(self) -> int:
        return math.floor(self.level / 2) + self.player_class.get_spell_power_bonus()

    def get_attack_crit_chance(self) -> int:
        crit_chance = self.weapon.attack_crit_chance
        crit_chance += self.player_class.player_class_crit_chance
        return crit_chance

    def get_spell_crit_chance(self) -> int:
        crit_chance = self.weapon.spell_crit_chance
        crit_chance += self.player_class.player_class_crit_chance
        return crit_chance

    def player_attack_is_crit(self, bonus_crit_chance: int = 0) -> bool:
        crit_chance = self.get_attack_crit_chance()
        crit_chance += bonus_crit_chance
        roll = random.randint(1, 100)
        return roll <= crit_chance

    def player_spell_is_crit(self, bonus_crit_chance: int = 0) -> bool:
        crit_chance = self.get_spell_crit_chance()
        crit_chance += bonus_crit_chance
        roll = random.randint(1, 100)
        return roll <= crit_chance

    def is_proficient_with_weapon(self, weapon: Weapon) -> bool:
        return weapon.weapon_type in self.player_class.proficient_weapon_types_list

    def change_weapon(self, new_weapon_id: int):
        item_to_equip = next(i for i in self.loot if i == int(new_weapon_id))
        self.loot.remove(item_to_equip)
        self.add_loot(self.weapon_id)
        self.weapon_id = item_to_equip
        self.weapon = statics.find_weapon_by_weapon_id(new_weapon_id)

    def change_class(self, new_class_id):
        try:
            new_class_id = int(new_class_id)
            search_by_class_name = False
        except ValueError:
            search_by_class_name = True
        # return if new class is current class
        if search_by_class_name and str(new_class_id).lower() == self.player_class.name.lower() or not search_by_class_name and int(new_class_id) == self.current_player_class_id:
            return
        try:
            if search_by_class_name:
                new_class = next(pc for pc in self.player_class_list if pc.name.lower() == str(new_class_id).lower())
            else:
                new_class = next(pc for pc in self.player_class_list if pc.class_id == new_class_id)
            current_health_percentage = self.hp / self.player_class.max_hp
            if int(current_health_percentage) > 1:
                current_health_percentage = 1
            self.player_class = new_class
            self.current_player_class_id = new_class.class_id
            self.hp = math.floor(self.player_class.max_hp * current_health_percentage)
            if self.hp < -3:
                self.hp = -3
            self.mana = 0
        except StopIteration:
            self.player_class_list.sort(key=lambda x: x.class_id)
            unlocked_classes = ", ".join(f"{pc.class_id}/{pc.name}" for pc in self.player_class_list)
            raise AssertionError(f"Class with id '{new_class_id}' is not unlocked. Your unlocked classes is: {unlocked_classes}")

    def unlocked_classes_info(self) -> str:
        info = "Unlocked classes\n"
        info += "\n".join(f"{pc.name} level {pc.level}\n\tHp {pc.max_hp} Mana {pc.max_mana}" for pc in self.player_class_list)
        return info

    def buff_bonus_damage_if_stronger(self, bonus_damage: int, duration: int) -> bool:
        buff_applied = False
        if bonus_damage > self.bonus_magic_dmg or bonus_damage == self.bonus_magic_dmg and duration > self.bonus_magic_dmg_duration:
            self.bonus_magic_dmg_duration = duration
            self.bonus_magic_dmg = bonus_damage
            buff_applied = True
        return buff_applied

    def subtract_bonus_magic_dmg_duration(self, n: int) -> bool:
        duration_subtracted = False
        if self.bonus_magic_dmg_duration > 0:
            self.bonus_magic_dmg_duration -= n
            duration_subtracted = True
            if self.bonus_magic_dmg_duration <= 0:
                self.bonus_magic_dmg_duration = 0
                self.bonus_magic_dmg = 0
        return duration_subtracted

    async def attack_monster_with_weapon(self, target_monster, player_weapon: Weapon, retaliate: bool = True) -> AttackMessage:
        bonus_crit_chance = 0
        if target_monster.easier_to_crit_duration > 0:
            bonus_crit_chance = target_monster.easier_to_crit_percentage
            target_monster.easier_to_crit_percentage -= 1
        is_crit = self.player_attack_is_crit(bonus_crit_chance)
        damage_dealt = await target_monster.receive_damage_from_player(self, player_weapon, is_crit)
        total_damage_over_time, players_that_dealt_damage_over_time = await target_monster.receive_damage_from_damage_over_time_effects()
        xp_earned = statics.calculate_experience_from_damage_dealt_to_monster(damage_dealt, target_monster)
        self.gain_xp(xp_earned, target_monster.level)

        attack_message = AttackMessage(self, target_monster)
        attack_message.player_crit = is_crit
        attack_message.weapon_attack_message = f"{player_weapon.attack_message} and dealt **{damage_dealt} **damage!"
        attack_message.player_xp_earned = xp_earned

        if target_monster.is_alive() and retaliate:
            attack_message.monster_retaliate_message = target_monster.deal_damage_to_player(self)
        if self.subtract_bonus_magic_dmg_duration(1):
            attack_message.player_extra_damage_from_buffs = True
        if len(players_that_dealt_damage_over_time) > 0:
            attack_message.monster_affected_by_dots = f"Monster took an additional {total_damage_over_time} damage from various spell effects by {', '.join([p for p in players_that_dealt_damage_over_time])}"

        return attack_message

    def receive_damage_from_monster(self, damage: int, monster_name: str, new_line: bool = True):
        self.hp -= damage
        damage_text = f"You received {damage} damage from {monster_name}"
        if self.is_knocked_out():
            if self.hp < -3:
                self.hp = -3
            damage_text += f", and is knocked out"
        if new_line:
            damage_text += "\n"
        return damage_text

    def add_all_missing_starting_classes(self):
        cleric = playerClass.Cleric(self)
        executioner = playerClass.Executioner(self)
        ranger = playerClass.Ranger(self)
        for pc in [cleric, executioner, ranger]:
            try:
                next(player_class for player_class in self.player_class_list if player_class.class_id == pc.class_id)
            except StopIteration:
                self.player_class_list.append(pc)

    def add_loot(self, loot):
        loot_id: int
        if isinstance(loot, LootItem):
            loot_id = loot.loot_id
        elif str(loot).isdigit():
            loot_id = int(loot)
        else:
            raise Exception(f"Could not add loot '{loot}' to {self.possessive_name()} inventory")
        if self.loot_is_in_auto_sell_list(loot_id):
            wp = statics.find_weapon_by_weapon_id(loot_id)
            self.gold += wp.get_sell_price()
        else:
            self.loot.append(loot_id)
            self.loot = sorted(self.loot)

    def sell_loot(self, loot_to_sell: LootItem) -> str:
        self.loot.remove(loot_to_sell.loot_id)
        sell_price = loot_to_sell.get_sell_price()
        return f"You sold '{loot_to_sell.get_name()}' for {sell_price} gold"

    def loot_is_in_auto_sell_list(self, loot_id: int) -> bool:
        return loot_id in self.auto_sell_list

    def restore_all_health_and_mana(self):
        self.hp = self.player_class.max_hp
        self.mana = self.player_class.max_mana

    def regen_health(self, times_to_regen=1):
        if self.is_knocked_out():
            hp_to_regen = 1 * times_to_regen
        else:
            hp_to_regen = self.player_class.get_health_regen_rate() * times_to_regen
        new_hp = self.hp + hp_to_regen
        if new_hp > self.get_max_hp():
            self.hp = self.get_max_hp()
        else:
            self.hp = new_hp

    def regen_mana(self, times_to_regen=1):
        mana_to_regen = self.player_class.get_mana_regen_rate() * times_to_regen
        new_mana = self.mana + mana_to_regen
        if new_mana > self.get_max_mana():
            self.mana = self.get_max_mana()
        else:
            self.mana = new_mana
