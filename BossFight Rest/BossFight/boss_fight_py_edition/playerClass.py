import abc
import math
import source.games.boss_fight.ability as abilities
from source.games.boss_fight.FightEnum import WeaponType


class PlayerClassRequirement:
    def __init__(self, class_id: int, lvl_req: int, class_name: str):
        self.class_id = class_id
        self.lvl_req = lvl_req
        self.class_name = class_name

    def __str__(self):
        return f"{self.class_name}:{self.class_id} - lvl {self.lvl_req}"


class PlayerClass(abc.ABC):
    _default_hp_scale = 2
    _default_mana_scale = 2
    player_class_id = -1
    base_health = 10
    base_mana = 10

    def __init__(self, name: str, xp: int, level: int, player, proficient_weapon_types_list: list[WeaponType], hp_scale: float = None, mana_scale: float = None):
        self.name: str = name
        self.xp: int = xp
        self.level: int = level
        self.player = player
        self.abilities: dict = self.prepare_available_abilities()
        self.max_hp: int = 0
        self.max_mana: int = 0
        if hp_scale is not None:
            self.hp_scale: float = hp_scale
        else:
            self.hp_scale: float = self._default_hp_scale
        if mana_scale is not None:
            self.mana_scale: float = mana_scale
        else:
            self.mana_scale: float = self._default_mana_scale
        self.hp_regen_rate: int = 1
        self.mana_regen_rate: int = 1
        self.player_class_crit_chance: int = 0
        self.attack_power_bonus: int = 0
        self.spell_power_bonus: int = 0
        self.class_id: int = self.player_class_id
        self.purchase_price: int = 0
        self.proficient_weapon_types_list: list[WeaponType] = proficient_weapon_types_list
        self.recalculate()

    @abc.abstractmethod
    def prepare_available_abilities(self) -> dict:
        pass

    @classmethod
    @abc.abstractmethod
    def from_dict(cls, player_class_dict: dict, player):
        pass

    @staticmethod
    def get_class_unlock_requirements() -> [PlayerClassRequirement]:
        return []

    def to_dict(self) -> dict:
        player_class_dict = {}
        player_class_dict["name"] = self.name
        player_class_dict["xp"] = self.xp
        player_class_dict["level"] = self.level
        return player_class_dict

    def recalculate(self):
        self.level = self.level
        self.max_hp = math.floor(self.hp_scale * self.level) + self.base_health
        self.max_mana = math.floor(self.mana_scale * self.level) + self.base_mana
        self.abilities: dict = self.prepare_available_abilities()

    def __str__(self):
        return self.name

    def shop_str(self, length_of_longest_player_class_name: int, length_of_longest_player_class_cost: int) -> str:
        purchase_price_str = f"{self.purchase_price:,}"
        purchase_price_str = purchase_price_str.replace(",", ".")
        player_class_req = self.get_class_unlock_requirements()
        player_class_req_str = ", ".join([f"Level {req.lvl_req} {req.class_name}" for req in player_class_req])
        return f"{self.name.ljust(length_of_longest_player_class_name, '.')} {purchase_price_str.ljust(length_of_longest_player_class_cost)} gold {player_class_req_str}"

    def info_str(self) -> str:
        class_name_str = f"Class: {self.name}"
        crit_chance_str = f"Base critical chance: {self.player_class_crit_chance}%"
        base_hp_mana_str = f"Start hp: {self.base_health}; start mana: {self.base_mana}"
        scales_str = f"{self.hp_scale} hp per level; {self.mana_scale} mana per level"
        regen_str = f"{self.hp_regen_rate} hp per regen tick; {self.mana_regen_rate} mana per regen tick"
        proficient_weapons_str = f"Is proficient with: {', '.join([str(p) for p in self.proficient_weapon_types_list])}"
        prev_lvl = self.level
        self.level = 99
        all_spells = self.prepare_available_abilities()
        self.level = prev_lvl
        abilities_str = "Spell list:\n" + f"\n".join([str(s) for s in all_spells.values()])
        return "\n".join([class_name_str, crit_chance_str, base_hp_mana_str, scales_str, regen_str,  abilities_str, proficient_weapons_str])

    def get_health_regen_rate(self) -> int:
        return self.hp_regen_rate

    def get_mana_regen_rate(self) -> int:
        return self.mana_regen_rate

    def get_attack_power_bonus(self) -> int:
        return self.attack_power_bonus

    def get_spell_power_bonus(self) -> int:
        return self.spell_power_bonus

    def level_up(self):
        self.level += 1
        self.xp = 0
        self.recalculate()
        self.player.restore_all_health_and_mana()

    def have_met_requirements(self, player_class_list: ['PlayerClass']) -> bool:
        requirements_met = True
        class_requirements = self.get_class_unlock_requirements()

        for class_req in class_requirements:
            try:
                next(pc for pc in player_class_list if pc.player_class_id == class_req.class_id)
            except StopIteration:
                raise AssertionError(f"You need to unlock class '{class_req.class_name}' first")

            for pc in player_class_list:
                if pc.player_class_id == class_req.class_id:
                    assert pc.level >= class_req.lvl_req, f"Did not meet level requirements for {class_req.class_name}"

        return requirements_met


class Cleric(PlayerClass):
    player_class_id = 1

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Cleric, self).__init__("Cleric", xp, level, player, [WeaponType.SWORD, WeaponType.MACE, WeaponType.DAGGER, WeaponType.STAFF], hp_scale=1.5, mana_scale=2.5)

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        heal = abilities.Heal()
        unlocked_abilities[heal.magic_word] = heal
        if self.level >= 5:
            smite = abilities.Smite()
            unlocked_abilities[smite.magic_word] = smite
        return unlocked_abilities


class Executioner(PlayerClass):
    player_class_id = 2

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Executioner, self).__init__("Executioner", xp, level, player, [WeaponType.SWORD, WeaponType.MACE, WeaponType.DAGGER, WeaponType.POLEARM], hp_scale=2.5, mana_scale=1.5)
        self.player_class_crit_chance = 2

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        sack_on_head = abilities.SackOnHead()
        unlocked_abilities[sack_on_head.magic_word] = sack_on_head
        if self.level >= 5:
            execute = abilities.Execute()
            unlocked_abilities[execute.magic_word] = execute
        return unlocked_abilities


class Ranger(PlayerClass):
    player_class_id = 3

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Ranger, self).__init__("Ranger", xp, level, player, [WeaponType.DAGGER, WeaponType.THROWN, WeaponType.BOW, WeaponType.IMPROVISED])
        self.player_class_crit_chance = 4

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        double_strike = abilities.DoubleStrike()
        unlocked_abilities[double_strike.magic_word] = double_strike
        if self.level >= 5:
            poisoned_bait = abilities.PoisonedBait()
            unlocked_abilities[poisoned_bait.magic_word] = poisoned_bait
        return unlocked_abilities


class Hexer(PlayerClass):
    player_class_id = 4

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Hexer, self).__init__("Hexer", xp, level, player, [WeaponType.DAGGER, WeaponType.STAFF], hp_scale=3.5, mana_scale=4)
        self.player_class_crit_chance = 3
        self.purchase_price = 1000
        self.base_health = 12
        self.base_mana = 13
        self.mana_regen_rate = 2
        self.attack_power_bonus = 1

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        hex = abilities.Hex()
        unlocked_abilities[hex.magic_word] = hex
        if self.level >= 5:
            fracture_skin = abilities.FractureSkin()
            unlocked_abilities[fracture_skin.magic_word] = fracture_skin
        return unlocked_abilities


class Mage(PlayerClass):
    player_class_id = 5

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Mage, self).__init__("Mage", xp, level, player, [WeaponType.DAGGER, WeaponType.STAFF], hp_scale=3, mana_scale=4.5)
        self.player_class_crit_chance = 4
        self.purchase_price = 1000
        self.base_health = 11
        self.base_mana = 14
        self.mana_regen_rate = 2
        self.spell_power_bonus = 1

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        ignite = abilities.Ignite()
        unlocked_abilities[ignite.magic_word] = ignite
        if self.level >= 5:
            enchant_weapon = abilities.EnchantWeapon()
            unlocked_abilities[enchant_weapon.magic_word] = enchant_weapon
        return unlocked_abilities


class Barbarian(PlayerClass):
    player_class_id = 6

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Barbarian, self).__init__("Barbarian", xp, level, player, [WeaponType.SWORD, WeaponType.MACE, WeaponType.DAGGER, WeaponType.POLEARM, WeaponType.IMPROVISED, WeaponType.THROWN, WeaponType.AXE, WeaponType.BOW], hp_scale=6, mana_scale=3)
        self.player_class_crit_chance = 7
        self.purchase_price = 1500
        self.base_health = 20
        self.base_mana = 14
        self.hp_regen_rate = 2
        self.attack_power_bonus = 2
        self.spell_power_bonus = 1

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    @staticmethod
    def get_class_unlock_requirements() -> [PlayerClassRequirement]:
        return [PlayerClassRequirement(Executioner.player_class_id, 10, "Executioner")]

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        shout = abilities.Shout()
        unlocked_abilities[shout.magic_word] = shout
        if self.level >= 5:
            frenzy = abilities.Frenzy()
            unlocked_abilities[frenzy.magic_word] = frenzy
        return unlocked_abilities


class Paladin(PlayerClass):
    player_class_id = 7

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(Paladin, self).__init__("Paladin", xp, level, player, [WeaponType.SWORD, WeaponType.MACE, WeaponType.DAGGER, WeaponType.POLEARM, WeaponType.AXE], hp_scale=5.5, mana_scale=3.5)
        self.player_class_crit_chance = 6
        self.purchase_price = 1500
        self.base_health = 16
        self.base_mana = 16
        self.hp_regen_rate = 2
        self.attack_power_bonus = 1
        self.spell_power_bonus = 1

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    @staticmethod
    def get_class_unlock_requirements() -> [PlayerClassRequirement]:
        return [PlayerClassRequirement(Cleric.player_class_id, 10, "Cleric")]

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        sacrifice = abilities.Sacrifice()
        unlocked_abilities[sacrifice.magic_word] = sacrifice
        heal = abilities.Heal()
        unlocked_abilities[heal.magic_word] = heal
        if self.level >= 5:
            full_restore = abilities.GreaterHeal()
            unlocked_abilities[full_restore.magic_word] = full_restore
        return unlocked_abilities


class MonsterHunter(PlayerClass):
    player_class_id = 8

    def __init__(self, player, xp: int = 0, level: int = 1):
        super(MonsterHunter, self).__init__("Monster Hunter", xp, level, player, [WeaponType.SWORD, WeaponType.MACE, WeaponType.DAGGER, WeaponType.POLEARM, WeaponType.IMPROVISED, WeaponType.THROWN, WeaponType.AXE, WeaponType.BOW], hp_scale=5, mana_scale=4)
        self.player_class_crit_chance = 7
        self.purchase_price = 1500
        self.base_health = 18
        self.base_mana = 16
        self.mana_regen_rate = 2
        self.attack_power_bonus = 2
        self.spell_power_bonus = 1

    @classmethod
    def from_dict(cls, player_class_dict: dict, player):
        xp = player_class_dict["xp"]
        level = int(player_class_dict["level"])
        player_class = cls(player, xp, level)
        player_class.recalculate()
        return player_class

    @staticmethod
    def get_class_unlock_requirements() -> [PlayerClassRequirement]:
        return [PlayerClassRequirement(Ranger.player_class_id, 10, "Ranger")]

    def prepare_available_abilities(self) -> dict:
        unlocked_abilities = {}
        over_sized_bear_trap = abilities.OverSizedBearTrap()  # Monster misses next attack, starts bleeding, and takes upfront damage
        unlocked_abilities[over_sized_bear_trap.magic_word] = over_sized_bear_trap
        turn_weapon_to_silver = abilities.TurnWeaponToSilver()  # deals extra damage to exotic monster types
        unlocked_abilities[turn_weapon_to_silver.magic_word] = turn_weapon_to_silver
        if self.level >= 5:
            big_game_trophy = abilities.BigGameTrophy()  # Extra dmg, gain bounty on kill, easier to crit monster
            unlocked_abilities[big_game_trophy.magic_word] = big_game_trophy
        return unlocked_abilities


ALL_CLASSES: list[PlayerClass] = [Cleric(None, level=99), Ranger(None, level=99), Executioner(None, level=99), Hexer(None, level=99), Mage(None, level=99), Barbarian(None, level=99), Paladin(None, level=99), MonsterHunter(None, level=99)]


def display_info(class_name: str) -> str:
    try:
        class_to_print = next(pc for pc in ALL_CLASSES if pc.name.lower() == class_name.lower())
        text = class_to_print.info_str()
    except StopIteration:
        text = f"Could not find class '{class_name}'"
    return text
