import math
from source.games.boss_fight.FightEnum import WeaponType
from source.games.boss_fight.lootItem import LootItem

DEFAULT_WEAPON_DROP_CHANCE = 1.0


class Weapon(LootItem):
    def __init__(self, weapon_id: int, name: str, attack_message: str, weapon_type: WeaponType = WeaponType.IMPROVISED, attack_power: int = 1, cost: int = 0, attack_crit_chance: int = 3, drop_chance: float = DEFAULT_WEAPON_DROP_CHANCE,
                 spell_power: int = 0, spell_crit_chance: int = 0, boss_weapon: bool = False, weapon_lvl: int = 1):
        super().__init__(weapon_id, name, drop_chance, cost)
        self.weapon_type = weapon_type
        self.attack_message: str = attack_message
        self.boss_weapon: bool = boss_weapon
        self.weapon_lvl = weapon_lvl
        self.attack_power: int = attack_power
        if boss_weapon:
            self.calc_weapon_stats()
        self.attack_crit_chance: int = attack_crit_chance
        self.spell_power: int = spell_power
        self.spell_crit_chance: int = spell_crit_chance

    def set_boss_weapon_properties(self, weapon_level):
        self.boss_weapon = True
        self.weapon_lvl = weapon_level
        self.calc_weapon_stats()

    def inventory_str(self):
        spell_str = ""
        if self.spell_power != 0 or self.spell_crit_chance != 0:
            spell_str = f" - spell power { self.spell_power } - spell crit chance { self.spell_crit_chance }"
        return f"{ self.loot_name } ({self.weapon_type}) - atk. power { self.attack_power } - atk. crit chance - { self.attack_crit_chance }{spell_str}"

    def get_weapon_type_str(self) -> str:
        return str(self.weapon_type)

    def shop_str(self, length_of_longest_name, length_of_longest_type_name, longest_attack_digit, longest_gold_price_digit, longest_crit_chance_digit, longest_spell_power_digit, longest_spell_crit_chance_digit):
        w_name = f"{self.loot_name} ({self.get_weapon_type_str()})".ljust(length_of_longest_type_name + length_of_longest_name, '.')
        w_cost = f"{self.cost:,}".ljust(longest_gold_price_digit, ' ')
        w_attack = str(self.attack_power).ljust(longest_attack_digit, ' ')
        w_crit = f"{self.attack_crit_chance}%".ljust(longest_crit_chance_digit + 1, ' ')
        w_spell_power = ""
        if self.spell_power != 0:
            w_spell_power = f"{str(self.spell_power).ljust(longest_spell_power_digit, ' ')} spell power"
        w_spell_crit = ""
        if self.spell_crit_chance != 0:
            w_spell_crit = f" {self.spell_crit_chance}%".ljust(longest_spell_crit_chance_digit + 1, ' ') + " spell crit chance"
        return f"{ w_name } { w_cost} gold { w_attack } atk. power { w_crit } atk. crit chance {w_spell_power}{w_spell_crit}"

    def calc_weapon_stats(self):
        if self.weapon_type is WeaponType.STAFF:
            attack_power = 1
            attack_crit_chance = 3
        else:
            attack_power = math.floor(self.weapon_lvl / 2)
            attack_crit_chance = math.floor(self.weapon_lvl / 3)
            if attack_crit_chance > 20:
                attack_crit_chance = 20
        self.attack_power = attack_power
        self.attack_crit_chance = attack_crit_chance


class WeaponList:
    # atc_w = Weapon(weapon_id=ID, name="NAME", attack=ATTACK, weapon_type=WeaponType, attack_message="MESSAGE", cost=COST, attack_crit_chance=DEFAULT_WEAPON_CRIT_CHANCE, drop_chance=DEFAULT_WEAPON_DROP_CHANCE)
    # cas_w = Weapon(weapon_id=ID, name="NAME", attack_power=ATTACK, weapon_type=WeaponType.STAFF, attack_message="MESSAGE", cost=COST, spell_power=SPELL_P, spell_crit_chance=SPELL_C, drop_chance=DEFAULT_WEAPON_DROP_CHANCE)
    # tier1
    fists = Weapon(weapon_id=1, name="Fists", attack_power=1, attack_message="You swung your fists", cost=0, attack_crit_chance=0)
    long_stick = Weapon(weapon_id=2, name="Long stick", attack_power=1, weapon_type=WeaponType.STAFF, attack_message="You poked the monster with your stick", cost=15, attack_crit_chance=2, spell_power=2, spell_crit_chance=1)
    bag_of_pebbles = Weapon(weapon_id=3, name="Bag of pebbles", attack_power=2, weapon_type=WeaponType.THROWN, attack_message="You took a pebble from your bag and threw it at the monster", cost=12)
    lemon = Weapon(weapon_id=4, name="Lemon", attack_power=2, attack_message="You squeezed some lemon juice into the monster's eyes", cost=4, attack_crit_chance=4)
    wooden_club = Weapon(weapon_id=5, name="Wooden club", attack_power=3, weapon_type=WeaponType.MACE, attack_message="You bashed the monster with your club", cost=30)
    scalpel = Weapon(weapon_id=6, name="Scalpel", attack_power=3, weapon_type=WeaponType.DAGGER, attack_message="You (very precisely) cut the monster with your scalpel", cost=33, attack_crit_chance=6)
    lego_pieces = Weapon(weapon_id=7, name="Lego pieces", attack_power=2, attack_message="You threw some Lego© on the ground, the monster steps on some and cry a little", cost=50, attack_crit_chance=5)
    broken_ukulele = Weapon(weapon_id=8, name="Broken Ukulele", attack_power=2, attack_message="You played a terrible song, and it hurt the monster's ears", cost=5)
    # tier2
    longbow = Weapon(weapon_id=9, name="Longbow", attack_power=5, weapon_type=WeaponType.BOW, attack_message="You shot an arrow at the monster", cost=200)
    longsword = Weapon(weapon_id=10, name="Longsword", attack_power=5, weapon_type=WeaponType.SWORD, attack_message="You slashed the monster", cost=205)
    birch_staff = Weapon(weapon_id=27, name="Birch staff", attack_power=3, weapon_type=WeaponType.STAFF, attack_message="You swung your staff at the monster", cost=185, spell_power=5, spell_crit_chance=3)
    # tier3
    battle_axe = Weapon(weapon_id=11, name="Battle axe", attack_power=8, weapon_type=WeaponType.AXE, attack_message="You violently struck the monster with your axe", cost=500, attack_crit_chance=8)
    beech_staff = Weapon(weapon_id=28, name="Beech staff", attack_power=5, weapon_type=WeaponType.STAFF, attack_message="You bonked the monster on the head with your beech staff", cost=545, spell_power=8, spell_crit_chance=6)
    # monster weapons
    bobos_wooden_club = Weapon(weapon_id=5, name="BoBo's Wooden club", attack_power=wooden_club.attack_power + 1, weapon_type=WeaponType.MACE, attack_message=wooden_club.attack_message, cost=60, attack_crit_chance=wooden_club.attack_crit_chance, drop_chance=0.35)
    rusted_sword = Weapon(weapon_id=12, name="Rusted sword", attack_power=2, weapon_type=WeaponType.SWORD, attack_message="You stabbed the monster with your rusted sword", cost=10, attack_crit_chance=3, drop_chance=0.66)
    sharp_stick = Weapon(weapon_id=13, name="Sharp stick", attack_power=3, weapon_type=WeaponType.POLEARM, attack_message="You poked the monster in the eye with your sharp stick. Ouch!", cost=12, attack_crit_chance=4, drop_chance=0.50)
    bone = Weapon(weapon_id=14, name="Bone", attack_power=2, weapon_type=WeaponType.MACE, attack_message="You bonked the monster with your bone", cost=6, attack_crit_chance=2, drop_chance=0.66)
    boar_fang = Weapon(weapon_id=15, name="Boar fang", attack_power=3, attack_message="You stabbed the monster with your boar fang", cost=32, attack_crit_chance=5, drop_chance=0.20)
    arming_sword = Weapon(weapon_id=16, name="Arming sword", attack_power=longsword.attack_power, weapon_type=WeaponType.SWORD, attack_message=longsword.attack_message, cost=250, attack_crit_chance=longsword.attack_crit_chance + 2, drop_chance=0.20)
    gladius = Weapon(weapon_id=29, name="Gladius", attack_power=9, weapon_type=WeaponType.SWORD, attack_message=longsword.attack_message, cost=520, attack_crit_chance=longsword.attack_crit_chance + 2, drop_chance=0.10)
    holy_hand_grenade = Weapon(weapon_id=17, name="Holy hand grenade", attack_power=6, weapon_type=WeaponType.THROWN, attack_message="The holy hand grenade exploded at the feet of the monster", cost=44, attack_crit_chance=8, drop_chance=0.05)
    katana = Weapon(weapon_id=18, name="Katana", attack_power=longsword.attack_power, weapon_type=WeaponType.SWORD, attack_message="You masterfully sliced up the monster with your glorious nippon steel that has been folded 500 times", cost=280, attack_crit_chance=longsword.attack_crit_chance + 2, drop_chance=0.20)
    apprentice_staff = Weapon(weapon_id=29, name="Apprentice's staff", attack_power=2, weapon_type=WeaponType.STAFF, attack_message="You clumsily swung your staff towards the monster", cost=45, spell_power=3, spell_crit_chance=2, drop_chance=0.20)
    # D2 normal weapons
    brainhew = Weapon(weapon_id=19, name="Brainhew", attack_power=battle_axe.attack_power + 1, weapon_type=WeaponType.AXE, attack_message="You attempt the split your foes skull", cost=700, attack_crit_chance=battle_axe.attack_crit_chance + 1, drop_chance=0.05)
    rogues_bow = Weapon(weapon_id=20, name="Rogue's Bow", attack_power=longbow.attack_power, weapon_type=WeaponType.BOW, attack_message="You feel Hefaetrus aid as you fire you arrow", cost=300, attack_crit_chance=longbow.attack_crit_chance + 2, drop_chance=0.05)
    the_jade_tan_do = Weapon(weapon_id=21, name="The Jade Tan Do", attack_power=4, weapon_type=WeaponType.DAGGER, attack_message="You struck the monster with your ritual kris", cost=100, drop_chance=0.05)
    the_battlebranch = Weapon(weapon_id=22, name="The Battlebranch", attack_power=7, weapon_type=WeaponType.POLEARM, attack_message="You sliced the monster with your Battlebranch", cost=475, attack_crit_chance=7, drop_chance=0.05)
    Spire_of_Lazarus = Weapon(weapon_id=23, name="Spire of Lazarus", attack_power=5, weapon_type=WeaponType.STAFF, attack_message="You wacked the monster", cost=280, drop_chance=0.05)
    griswolds_edge = Weapon(weapon_id=24, name="Griswold's Edge", attack_power=6, weapon_type=WeaponType.SWORD, attack_message="You stabbed the monster with Griswold's Edge", cost=220, attack_crit_chance=9, drop_chance=0.05)
    ripsaw = Weapon(weapon_id=25, name="Ripsaw", attack_power=7, weapon_type=WeaponType.SWORD, attack_message="Ripsaw tears into your foe", cost=460, attack_crit_chance=7, drop_chance=0.05)
    gravenspine = Weapon(weapon_id=26, name="Gravenspine", attack_power=6, weapon_type=WeaponType.STAFF, attack_message="You hit you opponent with Gravespine", cost=420, attack_crit_chance=6, drop_chance=0.05)
    # Boss weapons
    frostmourne = Weapon(weapon_id=30, boss_weapon=True, name="Frostmourne", weapon_type=WeaponType.SWORD, attack_message="The power of the Lich King flows trough you", cost=1000, drop_chance=0.08)

    ALL_WEAPONS = [fists, long_stick, bag_of_pebbles, lemon, wooden_club, scalpel, lego_pieces, broken_ukulele, longbow, longsword, battle_axe, bobos_wooden_club, rusted_sword, sharp_stick, bone, boar_fang, arming_sword, holy_hand_grenade, katana, brainhew,
                   rogues_bow, the_jade_tan_do, the_battlebranch, Spire_of_Lazarus, griswolds_edge, ripsaw, gravenspine, birch_staff, beech_staff, apprentice_staff, gladius, frostmourne]
    BASIC_WEAPONS = [long_stick, bag_of_pebbles, lemon, wooden_club, scalpel, lego_pieces, longbow, longsword, battle_axe, birch_staff, beech_staff]
    MONSTER_WEAPONS = [bobos_wooden_club, rusted_sword, sharp_stick, bone, boar_fang, arming_sword, holy_hand_grenade, katana, brainhew, rogues_bow, the_jade_tan_do, the_battlebranch, Spire_of_Lazarus, griswolds_edge, ripsaw, gravenspine, apprentice_staff, gladius]
    DIABLO2_WEAPONS = [brainhew, rogues_bow, the_jade_tan_do, the_battlebranch, Spire_of_Lazarus, griswolds_edge, ripsaw, gravenspine]
    BOSS_WEAPONS = [frostmourne]
