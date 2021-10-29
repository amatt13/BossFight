import collections
import inspect
import math
import random
import sys
from source.games.boss_fight import target
from source.games.boss_fight.DamageTrackerEntry import DamageTrackerEntry
from source.games.boss_fight.Event import post_event, MONSTER_KILLED_EVENT
from source.games.boss_fight.FightEnum import MonsterType
from source.games.boss_fight.lootItem import LootItem
from source.games.boss_fight.weapon import Weapon, WeaponList

ERROR_IMAGE_URL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTZ5TWeaqjdWuvgco4oq5N50bWNGwE-eJDGpg&usqp=CAU"
TIER_DIVIDER = 5


def recreate_monster(monster_name: str) -> "Monster":
    for name, obj in inspect.getmembers(sys.modules[__name__]):
        if inspect.isclass(obj) and issubclass(obj, Monster):
            try:
                monster_instance = obj()
                if monster_instance.name == monster_name:
                    return monster_instance
            except TypeError:
                pass


class Monster(target.Target):
    monster_crit_modifier = 1.5
    dmg_over_time_tuple = collections.namedtuple("X", "sad")

    def __init__(self, name: str, image_url: str, damage_tracker=None, monster_type=None, boss_monster: bool = False):
        super().__init__(name=name)
        if monster_type is None:
            monster_type = [MonsterType.HUMANOID]
        self.damage_tracker: dict = damage_tracker if damage_tracker is not None else {}
        self.max_hp: int = 1
        self.image_url: str = image_url
        if isinstance(monster_type, list):
            self.monster_type_list: list[MonsterType] = monster_type
        else:
            self.monster_type_list: list[MonsterType] = [monster_type]
        self.boss_monster: bool = boss_monster
        self.blind_duration: int = 0
        self.stun_duration: int = 0
        self.lower_attack_duration: int = 0
        self.lower_attack_percentage: float = 0.0
        self.easier_to_crit_duration: int = 0
        self.easier_to_crit_percentage: int = 0
        self.damage_over_time_tracker: dict[str, DamageTrackerEntry] = {}  # key is player_id
        self.frenzy_stack_tracker: dict[str, int] = {}  # key is player_id, value is frenzy stack lvl/size

    @classmethod
    def from_dict(cls, monster_dict: dict):
        m = recreate_monster(monster_dict["name"])
        m.hp = monster_dict["hp"]
        m.max_hp = monster_dict["max_hp"]
        m.level = monster_dict["level"]
        m.damage_tracker = monster_dict["damage_tracker"]
        m.blind_duration = int(monster_dict["blind_duration"])
        m.stun_duration = int(monster_dict["stun_duration"])
        m.lower_attack_duration = monster_dict["lower_attack_duration"]
        m.lower_attack_percentage = monster_dict["lower_attack_percentage"]
        m.easier_to_crit_duration = monster_dict["easier_to_crit_duration"]
        m.easier_to_crit_percentage = monster_dict["easier_to_crit_percentage"]
        m.frenzy_stack_tracker = monster_dict["frenzy_stack_tracker"]
        damage_tracker_entry_dict_list = monster_dict["damage_over_time_tracker"]
        for entry in damage_tracker_entry_dict_list:
            p_id = entry["player_id"]
            m.damage_over_time_tracker[p_id] = DamageTrackerEntry(p_id, entry["player_name"], entry["duration"], entry["damage"])
        return m

    def to_dict(self) -> dict:
        monster_dict = {}
        dmg_over_time_list: [dict] = [d.to_dict() for d in self.damage_over_time_tracker.values()]
        monster_dict["damage_over_time_tracker"] = dmg_over_time_list
        monster_dict["blind_duration"] = self.blind_duration
        monster_dict["stun_duration"] = self.blind_duration
        monster_dict["damage_tracker"] = self.damage_tracker
        monster_dict["hp"] = self.hp
        monster_dict["level"] = self.level
        monster_dict["lower_attack_duration"] = self.lower_attack_duration
        monster_dict["lower_attack_percentage"] = self.lower_attack_percentage
        monster_dict["easier_to_crit_duration"] = self.easier_to_crit_duration
        monster_dict["easier_to_crit_percentage"] = self.easier_to_crit_percentage
        monster_dict["max_hp"] = self.max_hp
        monster_dict["name"] = self.name
        monster_dict["frenzy_stack_tracker"] = self.frenzy_stack_tracker
        return monster_dict

    def __str__(self):
        is_dead = ""
        boss = ""
        dots = ""
        monster_type = ""
        has_loot = ""
        try:
            next(mt for mt in self.monster_type_list if mt != MonsterType.HUMANOID)
            monster_type = f" ({self.monster_types_str()})"
        except StopIteration:
            pass
        if len(self.get_item_drops()) > 0:
            has_loot = "⭐"
        if self.is_dead():
            is_dead = "\n**Monster is dead**"
        if self.boss_monster:
            boss = "**---BOSS---**\n"
        if len(self.damage_over_time_tracker.keys()) > 0:
            dots = f"\nDot damage next turn: { sum([d.get_damage() for d in self.damage_over_time_tracker.values()]) }"
        debuffs = self.debuffs_string()
        return f"{ boss }{ self.name }{ has_loot }{ monster_type } : Level { self.level }\nHP: { self.hp }/{ self.max_hp }{ debuffs }{ dots }{ is_dead }"

    def has_monster_type_from_list(self, monster_type_list: list[MonsterType]):
        has_type = False
        for mt in monster_type_list:
            if self.has_monster_type(mt):
                has_type = True
                break
        return has_type

    def has_monster_type(self, monster_type: MonsterType):
        return monster_type in self.monster_type_list

    def monster_types_str(self) -> str:
        number_of_types = len(self.monster_type_list)
        types_str = ""
        if number_of_types == 1:
            types_str = str(self.monster_type_list[0])
        elif number_of_types == 2:
            types_str = f"{self.monster_type_list[0]} and {self.monster_type_list[1]}"
        elif number_of_types > 2:
            types_str = ", ".join([str(x) for x in self.monster_type_list[:-1]])
            types_str += f", and {self.monster_type_list[-1:]}"
        return types_str

    def debuffs_string(self) -> str:
        debuffs: list[str] = []
        if self.blind_duration > 0:
            debuffs.append(f"blinded {self.blind_duration}")
        if self.lower_attack_duration > 0:
            debuffs.append(f"lowered attack by {self.lower_attack_percentage * 100}% for {self.lower_attack_duration} attacks")
        if self.stun_duration > 0:
            debuffs.append(f"stunned {self.stun_duration}")
        if self.easier_to_crit_duration > 0:
            debuffs.append(f"easier to crit {self.easier_to_crit_percentage}% for {self.easier_to_crit_duration} attacks")
        result = ""
        if len(debuffs):
            result += "\n"
        return result + "\n".join(debuffs)

    def set_level_and_health(self, level: int):
        self.level = level
        self.calc_health()

    def get_max_hp(self):
        return self.max_hp

    def calc_health(self):
        hp = 10
        for i in range(self.level):
            if self.boss_monster:
                hp += hp * 0.22
            else:
                hp += hp * 0.15
        hp = math.floor(hp)
        if not self.boss_monster:
            variance = math.floor(hp/100*15)
            hp += random.randint(-variance, variance)
        self.max_hp = hp
        self.hp = hp

    def debuff_blind(self, duration: int):
        assert self.blind_duration < duration, "Cant blind monster. A stronger blind is already in effect"
        self.blind_duration = duration

    def debuff_stun(self, duration: int):
        assert self.stun_duration < duration, "Cant stun monster. A stronger stun is already in effect"
        self.stun_duration = duration

    def debuff_lower_dmg(self, duration: int, lower_attack_percentage: float):
        assert self.lower_attack_duration < duration or self.lower_attack_percentage < lower_attack_percentage, "Cant lower attack of monster. A stronger weaken is already in effect"
        self.lower_attack_duration = duration
        self.lower_attack_percentage = lower_attack_percentage

    def debuff_dmg_over_time(self, duration: int, damage: int, player):
        for tracker_player_id in self.damage_over_time_tracker.keys():
            assert tracker_player_id != player.player_id, "Each players is limited to a single damage over time effect"
        self.damage_over_time_tracker[player] = DamageTrackerEntry(player.player_id, player.name, duration, damage)

    def debuff_apply_frenzy_stack(self, frenzy_stacks_to_apply: int, player) -> int:
        if player.player_id in self.frenzy_stack_tracker.keys():
            self.frenzy_stack_tracker[player.player_id] += frenzy_stacks_to_apply
        else:
            self.frenzy_stack_tracker[player.player_id] = frenzy_stacks_to_apply
        # return current stack size for player
        return self.frenzy_stack_tracker[player.player_id]

    def debuff_easier_to_crit(self, duration: int, extra_crit_chance: int):
        assert self.easier_to_crit_duration < duration or self.easier_to_crit_percentage < extra_crit_chance, "Cant increase critical chance on monster. A stronger effect is already applied"
        self.easier_to_crit_duration = duration
        self.easier_to_crit_percentage = extra_crit_chance

    async def _take_damage(self, damage: int, attacking_player_id):
        self.hp -= damage
        if self.is_dead():
            await post_event(MONSTER_KILLED_EVENT, (self, attacking_player_id))

    async def receive_damage_from_player(self, attacking_player, player_weapon: Weapon, is_crit: bool) -> int:
        damage = player_weapon.attack_power + attacking_player.get_attack_bonus()
        if is_crit:
            damage *= 1.5
        if damage <= 0.0:
            damage = 1
        # variance
        variance = (attacking_player.player_class.level / 5) - 1
        if variance > 0.0:
            damage += random.randint(math.floor(-variance), math.ceil(variance))
        damage = math.ceil(damage)
        if attacking_player.player_id in self.damage_tracker:
            self.damage_tracker[attacking_player.player_id] += damage
        else:
            self.damage_tracker[attacking_player.player_id] = damage
        await self._take_damage(damage, attacking_player.player_id)

        return damage

    async def receive_damage_from_damage_over_time_effects(self) -> (int, [str]):
        extra_damage = 0
        player_names = []
        # count up damage and record the players
        for entry in self.damage_over_time_tracker.values():
            if self.is_alive():
                await self._take_damage(entry.get_damage(), entry.get_player_id())
                extra_damage += entry.get_damage()
                player_names.append(entry.get_player_name())
                entry.subtract_turn()
        # remove debuffs where the duration is 0
        keys_to_remove = [key for key in self.damage_over_time_tracker.keys() if self.damage_over_time_tracker[key].get_duration() <= 0]
        for key in keys_to_remove:
            self.damage_over_time_tracker.pop(key)
        return extra_damage, player_names

    def _monster_attack_is_crit(self) -> bool:
        crit_chance = 3
        if self.boss_monster:
            crit_chance = 15
        roll = random.randint(1, 100)
        return roll <= crit_chance

    def deal_damage_to_player(self, player_to_attack) -> str:
        damage_text = ""
        deal_damage = self.is_alive()
        if self.blind_duration > 0:
            self.blind_duration -= 1
            if deal_damage:
                affected_by_blind = random.randint(1, 100) > 33
                if affected_by_blind:
                    damage_text += "Monster is currently blind and cannot hit you. "
                    deal_damage = False
                else:
                    damage_text += "Monster resisted blind! "

        if self.stun_duration > 0:
            self.stun_duration -= 1
            if deal_damage:
                deal_damage = False
                damage_text += "Monster is currently stunned and cannot hit you. "

        if deal_damage:
            # base damage
            damage_dealt = self.level / 2
            # variance
            variance = (self.level / 5) - 1
            if variance > 0.0:
                damage_dealt += random.randint(math.floor(-variance), math.ceil(variance))
            # round up to 1
            if damage_dealt <= 0.0:
                damage_dealt = 1
            # bonus boss damage
            if self.boss_monster:
                damage_dealt = damage_dealt * 1.25
            # crit bonus
            if self._monster_attack_is_crit():
                damage_dealt *= Monster.monster_crit_modifier
                damage_text += f"{ self.name } LANDED A CRITICAL HIT!\n"
            # subtract "lowered attack" debuff
            if self.lower_attack_duration > 0:
                damage_dealt -= damage_dealt * self.lower_attack_percentage
                self.lower_attack_duration -= 1
            # round up
            damage_dealt = math.ceil(damage_dealt)
            damage_text += player_to_attack.receive_damage_from_monster(damage_dealt, self.name)
        result_text = f"{ damage_text }**Player hp:** { player_to_attack.hp }/{ player_to_attack.player_class.max_hp }"
        return result_text

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return []


class CuteGoblin(Monster):
    def __init__(self):
        super().__init__("Cute goblin", "https://static.vecteezy.com/system/resources/previews/003/009/853/original/cute-goblin-mascot-character-cartoon-icon-illustration-vector.jpg")


class Slime(Monster):
    def __init__(self):
        super().__init__("Slime", "https://www.toplessrobot.com/wp-content/uploads/2011/09/Slime.jpg", monster_type=MonsterType.MAGIC_CREATURE)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.rusted_sword]


class VillageIdiot(Monster):
    def __init__(self):
        super().__init__("Village idiot", "https://thedromedarytales.files.wordpress.com/2013/03/12europe03_1092_cropped.jpg?w=460&h=575")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.apprentice_staff]


class Cripple(Monster):
    def __init__(self):
        super().__init__("Cripple", "https://upload.wikimedia.org/wikipedia/commons/6/64/A_crippled_beggar_moves_with_crutches_accompanied_by_a_littl_Wellcome_V0020357.jpg")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.sharp_stick]


class GiantCentipede(Monster):
    def __init__(self):
        super().__init__("Giant centipede", "https://2e.aonprd.com/Images/Monsters/Centipede_TitanCentipede.png", monster_type=MonsterType.BEAST)


class DuckSizedHorse(Monster):
    def __init__(self):
        super().__init__("Duck sized horse", "https://external-preview.redd.it/Qt8kCZYvj_nG7q02v-jV2I10GEQKB298ax3f4u2zVzY.jpg?auto=webp&s=b51bb82d5810d33e6f9a8c09becb8031178eadc8", monster_type=MonsterType.BEAST)


class DabbingSkeleton(Monster):
    def __init__(self):
        super().__init__("Dabbing skeleton", "https://i.etsystatic.com/13035387/r/il/0b48dc/1310488516/il_570xN.1310488516_g5is.jpg", monster_type=MonsterType.UNDEAD)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.bone]


class Murloc(Monster):
    def __init__(self):
        super().__init__("Murloc", "https://wow.zamimg.com/uploads/screenshots/normal/37172-murloc-lurker.jpg")


class DireRat(Monster):
    def __init__(self):
        super().__init__("Dire Rat", "https://www.pngkey.com/png/detail/21-210947_dire-rat-pathfinder-dire-rat.png", monster_type=MonsterType.BEAST)


class BoBo(Monster):
    def __init__(self):
        super().__init__("Bo Bo", "https://thumbs.dreamstime.com/b/cartoon-caveman-wooden-club-27864592.jpg")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.bobos_wooden_club]


class HellBovine(Monster):
    def __init__(self):
        super().__init__("Hell Bovine", "https://static.wikia.nocookie.net/diablo/images/d/de/Cow.gif", monster_type=[MonsterType.DEMON, MonsterType.BEAST])

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return WeaponList.DIABLO2_WEAPONS


class CouncilMember(Monster):
    def __init__(self):
        super().__init__("Council Member", "https://static.wikia.nocookie.net/diablo_gamepedia/images/9/94/Council_Member_%28Diablo_II%29.gif", monster_type=[MonsterType.DEMON, MonsterType.HUMANOID])

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return WeaponList.DIABLO2_WEAPONS


class Giant(Monster):
    def __init__(self):
        super().__init__("Giant", "https://static.wikia.nocookie.net/elderscrolls/images/4/4f/Grok.png")


class Minotaur(Monster):
    def __init__(self):
        super().__init__("Minotaur", "https://cdnb.artstation.com/p/assets/images/images/005/191/851/large/peter-csanyi-minotaur5.jpg?1489173777")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.battle_axe]


class MinionOfDestruction(Monster):
    def __init__(self):
        super().__init__("Minion of Destruction", "https://static.wikia.nocookie.net/diablo_gamepedia/images/1/11/Minion_of_Destruction_%28Diablo_II%29.gif", monster_type=MonsterType.DEMON)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return WeaponList.DIABLO2_WEAPONS


class Cultist(Monster):
    def __init__(self):
        super().__init__("Cultist", "https://i.pinimg.com/originals/26/46/ab/2646ab16302d0bd5d93628854ca1ee61.png")


class CorruptRogueArcher(Monster):
    def __init__(self):
        super().__init__("Corrupt Rogue Archer", "https://static.wikia.nocookie.net/diablo_gamepedia/images/8/88/Dark_Ranger_%28Diablo_II%29.gif", monster_type=[MonsterType.DEMON, MonsterType.HUMANOID])

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return WeaponList.DIABLO2_WEAPONS


class Fallen(Monster):
    def __init__(self):
        super().__init__("Fallen", "https://static.wikia.nocookie.net/diablo_gamepedia/images/d/db/Fallen_%28Diablo_II%29.gif", monster_type=MonsterType.DEMON)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return WeaponList.DIABLO2_WEAPONS


class Boar(Monster):
    def __init__(self):
        super().__init__("Boar", "https://www.drawize.com/drawings/images/52fdc3wild-boar?width=1200", monster_type=MonsterType.BEAST)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.boar_fang]


class Gremlin(Monster):
    def __init__(self):
        super().__init__("Gremlin", "https://cdn1.tedsby.com/tb/large/storage/3/6/3/363895/collectible-fantasy-creature-gremlin-mr-grimm.jpg")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.apprentice_staff]


class Whelp(Monster):
    def __init__(self):
        super().__init__("Whelp", "https://cdn2.warcraftpets.com/images/pets/big/green_whelp.vb00ce173b1da0717e09aa4e6f336b35da109bf14.jpg", monster_type=MonsterType.DRAGON)


class Zombie(Monster):
    def __init__(self):
        super().__init__("Zombie", "https://i.pinimg.com/originals/db/6c/ce/db6ccebffed0da50cbafb8b895673ead.png", monster_type=MonsterType.UNDEAD)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.bone, WeaponList.rusted_sword]


class GiantFrog(Monster):
    def __init__(self):
        super().__init__("Giant Frog", "https://4.bp.blogspot.com/-8iZ33JJtEQ4/WHNjFa587HI/AAAAAAAABFk/pUsoNTzuLkA02nPgkb-38rxKSwW_P5WPQCLcB/s1600/giant_toad.jpg", monster_type=MonsterType.BEAST)


class Wolf(Monster):
    def __init__(self):
        super().__init__("Wolf", "https://i.pinimg.com/originals/4c/e9/ca/4ce9caa4f8fb034792434f4280a95722.png", monster_type=MonsterType.BEAST)


class Troglodyte(Monster):
    def __init__(self):
        super().__init__("Troglodyte", "https://64.media.tumblr.com/6fc9f2a3c8c1a14408ad1a9faa13d24b/9f240ebb94c93d0b-62/s540x810/9e249d4056e1b5c29268b621fb02174b2361b740.png")


class Boggard(Monster):
    def __init__(self):
        super().__init__("Boggard", "https://guildberkeley.files.wordpress.com/2020/03/boggard2-1.jpg")


class ManAtArms(Monster):
    def __init__(self):
        super().__init__("Man at arms", "https://shop.bestsoldiershop.com/WebRoot/StoreIT5/Shops/14739/5662/B7BF/F14D/9483/A8E5/3E95/9311/21C9/TB_54025_MEN_AT_ARMS_14th_24_TIN_BERLIN.jpg")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.arming_sword]


class Gladiator(Monster):
    def __init__(self):
        super().__init__("Gladiator", "https://i.pinimg.com/originals/66/cb/f8/66cbf892eb5f0c8e829bf7e3cbff5e00.jpg")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.gladius]


class Werebear(Monster):
    def __init__(self):
        super().__init__("Werebear", "https://cdna.artstation.com/p/assets/images/images/000/239/056/large/grzegorz-pedrycz-warewolf.jpg?1412699713", monster_type=[MonsterType.MAGIC_CREATURE, MonsterType.HUMANOID])


class Troll(Monster):
    def __init__(self):
        super().__init__("Troll", "https://i.pinimg.com/originals/12/72/95/1272957523d7e212eb3bd9f8dc7491de.png", monster_type=MonsterType.MAGIC_CREATURE)


class Lillend(Monster):
    def __init__(self):
        super().__init__("Lillend", "https://64.media.tumblr.com/980e9ba4cd8ee3ff85bc9a109b903d22/tumblr_inline_pqnmp83F7y1robfbt_640.png", monster_type=MonsterType.MAGIC_CREATURE)


class KillerRabbit(Monster):
    def __init__(self):
        super().__init__("Killer Rabbit", "https://static.wikia.nocookie.net/montypython/images/d/dd/Killer_rabbit.JPG/revision/latest?cb=20070904000613", monster_type=MonsterType.BEAST)

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.holy_hand_grenade]


class EdgeLord(Monster):
    def __init__(self):
        super().__init__("Edge Lord", "https://preview.redd.it/v32cdps3yeqy.png?auto=webp&s=5d7627e25ef7821fe806aced07f40c2927ba575e")

    @staticmethod
    def get_item_drops() -> list[LootItem]:
        return [WeaponList.katana, WeaponList.apprentice_staff]


class Lyander(Monster):
    def __init__(self):
        super().__init__("Lyander the half-elf cleric", "https://cdna.artstation.com/p/assets/images/images/033/743/432/large/william-hallett-tilanis-revision.jpg?1610461935")


class Bebilith(Monster):
    def __init__(self):
        super().__init__("Bebilith", "https://dnd35.files.wordpress.com/2016/04/bebilith.png", monster_type=MonsterType.MAGIC_CREATURE)


class Angel(Monster):
    def __init__(self):
        super().__init__("Angel", "https://i.pinimg.com/originals/3c/73/a6/3c73a62d3b71b53672c86bc77fa1c472.png", monster_type=MonsterType.MAGIC_CREATURE)


class GorillaDire(Monster):
    def __init__(self):
        super().__init__("Dire Gorilla", "https://pathfinderwiki.com/mediawiki/images/1/15/Gorilla.jpg", monster_type=MonsterType.BEAST)


class TRex(Monster):
    def __init__(self):
        super().__init__("T-Rex", "https://cdn.mos.cms.futurecdn.net/acwNent8xhiprPHCYj8bvK-1200-80.jpg", monster_type=MonsterType.BEAST)


class Brightwing(Monster):
    def __init__(self):
        super().__init__("Brightwing", "https://static.wikia.nocookie.net/wowpedia/images/9/9c/BrightwingArt.jpg/revision/latest/scale-to-width-down/1000?cb=20190902141719", monster_type=MonsterType.MAGIC_CREATURE)


class Hydra(Monster):
    def __init__(self):
        super().__init__("Hydra", "https://img1.goodfon.com/original/1920x1080/4/37/fantastika-monstr-golovy.jpg", monster_type=MonsterType.MAGIC_CREATURE)


class GreaterDemon(Monster):
    def __init__(self):
        super().__init__("Greater demon", "https://cdnb.artstation.com/p/assets/images/images/028/536/711/medium/masaki-hayashi-demon-final.jpg?1594752931", monster_type=MonsterType.DEMON)


class Imp(Monster):
    def __init__(self):
        super().__init__("Imp", "https://cdna.artstation.com/p/assets/images/images/032/096/084/large/aleksandr-golubev-cute-devil-girl2-2.jpg?1605471333", monster_type=MonsterType.DEMON)


class Succubus(Monster):
    def __init__(self):
        super().__init__("Succubus", "https://cdna.artstation.com/p/assets/images/images/027/194/238/large/bbang-q-09.jpg?1590916369", monster_type=MonsterType.DEMON)


# ONLY BOSSES BELOW


class NinjaBrian(Monster):
    def __init__(self):
        super().__init__("Ninja Brian", "https://pbs.twimg.com/media/D8a0hsiUcAY-1J-?format=jpg&name=large", boss_monster=True)


class DoomGuy(Monster):
    def __init__(self):
        super().__init__("Doomguy", "https://static.wikia.nocookie.net/fatecrossover/images/2/2e/Doom_eternal.jpg.jpg", boss_monster=True)


class TheLichKing(Monster):
    def __init__(self):
        super().__init__("The Lich King", "https://static.wikia.nocookie.net/wowwiki/images/5/5c/Fanart-0827-large.jpg/", boss_monster=True, monster_type=[MonsterType.UNDEAD, MonsterType.HUMANOID])


class Diablo(Monster):
    def __init__(self):
        super().__init__("Diablo, the lord of Terror", "https://static.wikia.nocookie.net/diablo/images/4/42/Diablo.gif", boss_monster=True, monster_type=MonsterType.DEMON)


class Deathwing(Monster):
    def __init__(self):
        super().__init__("Deathwing", "https://static.wikia.nocookie.net/wowwiki/images/1/10/Earthwarder.png/revision/latest/scale-to-width-down/720?cb=20140503155011", boss_monster=True, monster_type=MonsterType.DRAGON)


if __name__ == '__main__':
    all_monsters: list[Monster] = []
    for name, obj in inspect.getmembers(sys.modules[__name__]):
        if inspect.isclass(obj) and issubclass(obj, Monster):
            try:
                m = monster_instance = obj()
                all_monsters.append(m)
            except TypeError:
                pass
    for m in all_monsters:
        s = sum(x.name == m.name or x.image_url == m.image_url for x in all_monsters)
        if s > 1:
            raise AttributeError(f"Monster url or name used twice for monster '{m.name}'")
        elif len(m.monster_type_list) == 0:
            raise AttributeError(f"Monster has no MonsterType '{m.name}'")
    print("Checked these monsters:" + ", ".join([m.name for m in all_monsters]))
    print("Printing all")
    for m in all_monsters:
        print(m)
