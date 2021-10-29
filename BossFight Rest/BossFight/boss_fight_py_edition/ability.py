import copy
import math
import random
import source.games.boss_fight.monster as monster
import source.games.boss_fight.statics
import source.games.boss_fight.target as target
import source.games.boss_fight.weapon as weapon


class Ability:
    def __init__(self, name: str, description: str, mana_cost: int, magic_word: str):
        self.name = name
        self.description = description
        self.caster: target = None
        self.use_ability_text: str = ""
        self.only_target_monster: bool = False
        self.mana_cost = mana_cost  # -1 is any positive amount of mana
        self.magic_word: str = magic_word.lower()
        self.affects_all_players = False
        self.affects_all_players_str = ""

    def __str__(self):
        only_target_monster = ""
        if self.only_target_monster:
            only_target_monster = "*"
        return f"{self.mana_cost} mana - **{self.magic_word}**/**{self.name}**{only_target_monster} -> {self.description}"

    async def use_ability(self, caster, target_target: target.Target, dont_use_caster_effect: bool = False) -> str:
        self.use_ability_text = ""
        self.caster = caster
        if self.only_target_monster and not isinstance(target_target, target.Target):
            raise TypeError("Can only target monsters")
        if not dont_use_caster_effect:
            self.caster_effect()
        if target_target is not None:
            await self.target_effect(target_target)
        self.subtract_mana_cost_from_caster()
        self.add_mana_text()
        return self.affects_all_players_str + self.use_ability_text

    # The effect that will be executed on the caster
    def caster_effect(self):
        pass

    # The effect that will be executed on the target
    async def target_effect(self, target_target: target.Target):
        pass

    def affects_all_players_effect(self, all_players: list[target.Target]):
        pass

    def subtract_mana_cost_from_caster(self):
        if self.caster.mana < self.mana_cost:
            raise source.games.boss_fight.statics.WTFException(f"caster mana: {self.caster.mana} mana cost: {self.mana_cost}")
        self.caster.mana -= self.mana_cost

    def add_mana_text(self):
        if len(self.use_ability_text) > 0 and self.use_ability_text[-1:] != "\n":
            self.use_ability_text += "\n"
        self.use_ability_text += f"**Mana:** You have { self.caster.mana } mana left"

    def bold_name_with_colon(self):
        return f"**{self.name}:**"


# Cleric spells
class Heal(Ability):
    def __init__(self):
        super().__init__("Heal", "Restores some of the target's hp", 6, "heal")
        self._heal_limit = 16
        self.floor_heal = 3

    def caster_effect(self):
        self.heal(self.caster)

    async def target_effect(self, target_target: target.Target):
        self.heal(target_target)

    def heal(self, target_target: target.Target):
        assert not target_target.is_at_full_health(), f"Target need to be wounded ({target_target.hp}/{target_target.get_max_hp()} hp)"
        amount_restored = self.floor_heal + math.floor(self.caster.level * 1.5)
        if amount_restored > self._heal_limit:
            amount_restored = self._heal_limit

        if target_target.hp + amount_restored > target_target.get_max_hp():
            amount_restored = target_target.get_max_hp() - target_target.hp

        target_target.hp += amount_restored
        xp_gained = math.ceil(amount_restored / 2)
        self.caster.gain_xp(xp_gained)
        self.use_ability_text = f"You restored {amount_restored} of {target_target.possessive_name()} hp ({target_target.hp}/{target_target.get_max_hp()} hp)\nYou gained {xp_gained} xp"

    async def use_ability(self, caster, target_target: target.Target, dont_use_caster_effect: bool = False) -> str:
        if target_target is not None:
            dont_use_caster_effect = True
        return await super(Heal, self).use_ability(caster, target_target, dont_use_caster_effect)


class Smite(Ability):
    def __init__(self):
        super().__init__("Smite", "Deals bonus damage to undead monsters", 6, "smite")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        monster_target: monster.Monster = target_target
        if monster_target.has_monster_type(monster.MonsterType.UNDEAD):
            bonus_smite_damage = self.caster.level
            enhanced_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
            enhanced_weapon.set_name("Enhanced " + enhanced_weapon.get_name())
            enhanced_weapon.attack_power += bonus_smite_damage
            attack_message = await self.caster.attack_monster_with_weapon(monster_target, enhanced_weapon)
            attack_message.ability_extra = f"{self.bold_name_with_colon()} You smited {monster_target.name} for {bonus_smite_damage} bonus damage!"
            self.use_ability_text = str(attack_message)
        else:
            raise TypeError(f"Can only target undeads (monster is of type {monster_target.monster_types_str()})")


# Executioner spells
class Execute(Ability):
    _monster_health_percentage = 20.0

    def __init__(self):
        super().__init__("Execute", f"Deals massive damage to monsters that are near death (hp below { int(Execute._monster_health_percentage) }%)", -1, "execute")
        self.bonus_execute_damage: int = 0
        self.only_target_monster = True

    def subtract_mana_cost_from_caster(self):
        self.caster.mana = 0

    async def target_effect(self, target_target: monster.Monster):
        assert self.caster.mana > 0, "Must have at least 1 mana to use Execute"
        assert math.ceil(target_target.hp / target_target.max_hp * 100) <= Execute._monster_health_percentage, f"Monster cannot have more than { math.ceil(target_target.max_hp / 100 * Execute._monster_health_percentage) } health"
        self.bonus_execute_damage = self.caster.mana
        enhanced_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
        enhanced_weapon.attack_power += self.bonus_execute_damage
        attack_message = await self.caster.attack_monster_with_weapon(target_target, enhanced_weapon)
        attack_message.ability_extra = f"{self.bold_name_with_colon()} You executed { target_target.name } for { self.bonus_execute_damage } bonus damage!"
        self.use_ability_text = str(attack_message)


class SackOnHead(Ability):
    _blind_duration = 2

    def __init__(self):
        super().__init__("Sack on head", f"Blinds the target by putting a sack on their head (duration {SackOnHead._blind_duration})", 8, "sack")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        target_target.debuff_blind(SackOnHead._blind_duration)
        self.use_ability_text = f"Monster blinded for { SackOnHead._blind_duration } turns."


# Ranger spells
class DoubleStrike(Ability):
    _nerfed_damage_percentage = 0.5

    def __init__(self):
        super().__init__("Double strike", f"Hit the monster twice", 5, "dstrike")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        nerfed_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
        nerfed_weapon.attack_power = nerfed_weapon.attack_power * self._nerfed_damage_percentage
        attack_message = await self.caster.attack_monster_with_weapon(target_target, nerfed_weapon, retaliate=False)
        if target_target.hp > 0:
            ability_attack_message = await self.caster.attack_monster_with_weapon(target_target, nerfed_weapon, retaliate=True)
            attack_message.ability_extra = f"{self.bold_name_with_colon()} Double strike!\n{ability_attack_message}"
        self.use_ability_text = str(attack_message)
        if self.use_ability_text.endswith("\n\n"):
            self.use_ability_text = self.use_ability_text[:-1]


class PoisonedBait(Ability):
    def __init__(self):
        super().__init__("Poisoned bait", f"Lure in the monster with some poisoned food", 4, "bait")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        if target_target.has_monster_type(monster.MonsterType.BEAST):
            bait_weapon = weapon.Weapon(-1, "Bait", 5, weapon_type=weapon.WeaponType.IMPROVISED, attack_message="You tricked the monster to eat some poisoned food")
            attack_message = await self.caster.attack_monster_with_weapon(target_target, bait_weapon, retaliate=False)
            self.use_ability_text = str(attack_message)
        else:
            raise TypeError(f"Can only target beasts (monster is of type {target_target.monster_types_str()})")


# Hexer spells
class Hex(Ability):
    _hex_duration = 3
    _hex_percentage = 0.35

    def __init__(self):
        super().__init__("Hex", f"Curse the monster, lowering its attack damage (duration {self._hex_duration})", 3, "hex")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        target_target.debuff_lower_dmg(self._hex_duration, self._hex_percentage)
        self.use_ability_text = f"Monster's attack lowered for { self._hex_duration } turns."


class FractureSkin(Ability):
    _fractureSkin_skin_duration = 5
    _fractureSkin_skin_cont_dmg = 3

    def __init__(self):
        super().__init__("Fracture skin", f"Curse the monster, making its skin crack and fracture to deal continuous damage (duration {self._fractureSkin_skin_duration})", 6, "fracture")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        target_target.debuff_dmg_over_time(self._fractureSkin_skin_duration, self._fractureSkin_skin_cont_dmg, self.caster)
        self.use_ability_text = f"Monster will bleed for { self._fractureSkin_skin_duration } turns."


# Mage spells
class Ignite(Ability):
    _ignite_duration = 2
    _ignite_cont_dmg = 2
    _ignite_initial_damage = 8

    def __init__(self):
        super().__init__("Ignite", f"Ignite the monster, setting it on fire and dealing some additional damage (duration {self._ignite_duration})", 5, "ignite")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        ignite_spell = weapon.Weapon(-1, "Ignite", self._ignite_initial_damage, attack_message="You lit the monster on fire and scorched it", attack_crit_chance=15, weapon_type=weapon.WeaponType.STAFF)
        try:
            target_target.debuff_dmg_over_time(self._ignite_duration, self._ignite_cont_dmg, self.caster)
        except AssertionError:
            pass
        attack_message = await self.caster.attack_monster_with_weapon(target_target, ignite_spell)
        attack_message.ability_extra = f"{self.bold_name_with_colon()} Monster will burn for { self._ignite_duration } turns."
        self.use_ability_text += str(attack_message)


class EnchantWeapon(Ability):
    _enchant_weapon_duration = 2
    _enchant_weapon_dmg = 2

    def __init__(self):
        super().__init__("Enchant Weapon", "Enchant the target's weapon, making it deal bonus magical damage (cast on self if no target is designated)", 4, "enchant")

    def caster_effect(self):
        self.enchant_weapon(self.caster)

    async def target_effect(self, target_target: target.Target):
        self.enchant_weapon(target_target)

    def enchant_weapon(self, target_target):
        assert target_target.buff_bonus_damage_if_stronger(self._enchant_weapon_dmg, self._enchant_weapon_duration), "Failed to apply enchantment. Player is already affected by a stronger effect"
        self.use_ability_text = f"You enchanted {target_target.possessive_name()} weapon for {self._enchant_weapon_duration} turns."

    async def use_ability(self, caster, target_target: target.Target, dont_use_caster_effect: bool = False) -> str:
        if target_target is not None:
            dont_use_caster_effect = True
        return await super(EnchantWeapon, self).use_ability(caster, target_target, dont_use_caster_effect)


# Barbarian spells
class Shout(Ability):
    _shout_buff_duration = 1
    _shout_damage = 5
    _shout_debuff_duration = 3
    _shout_debuff_percentage = 0.20

    def __init__(self):
        super().__init__("Shout", "Increase all players' attack and lower monster's", 8, "shout")
        self.affects_all_players = True
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        debuff_was_applied: bool
        try:
            target_target.debuff_lower_dmg(self._shout_debuff_duration, self._shout_debuff_percentage)
            debuff_was_applied = True
        except AssertionError:
            debuff_was_applied = False
        if debuff_was_applied:
            self.use_ability_text += f"Monster's attack lowered for { self._shout_debuff_duration } turns."
        else:
            self.use_ability_text += f"Could not lower Monster's attack. A stronger weaken is in effect"

    def affects_all_players_effect(self, all_players: list[target.Target]):
        self.affects_all_players_str = ""
        affected_players = 0
        for player in all_players:
            if player.buff_bonus_damage_if_stronger(self._shout_damage, self._shout_buff_duration):
                affected_players += 1
        assert affected_players > 0, "Failed to apply Shout. Every players is currently affected by a stronger effect."
        s_ending = ""
        if affected_players > 1:
            s_ending = "s"
        self.affects_all_players_str += f"\nYour shout boosted { affected_players } player{ s_ending }!\n"


class Frenzy(Ability):
    _frenzy_stack_damage = 4

    def __init__(self):
        super().__init__("Frenzy", f"Swing you weapon in blood raged frenzy and apply a Frenzy Stack! Frenzy deals bonus damage depending on Frenzy Stacks", 10, "frenzy")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        frenzy_stack = target_target.debuff_apply_frenzy_stack(1, self.caster)
        frenzy_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
        frenzy_weapon.attack_power += frenzy_stack * self._frenzy_stack_damage
        attack_message = await self.caster.attack_monster_with_weapon(target_target, frenzy_weapon)
        attack_message.ability_extra = f"{self.bold_name_with_colon()} Frenzy strike level {frenzy_stack}!"
        self.use_ability_text += str(attack_message)


# Paladin spells
class Sacrifice(Ability):
    _bonus_sacrifice_damage = 8
    _self_sacrifice_damage = 3
    _bonus_undead_damage = 5  # caster level is added as well

    def __init__(self):
        super().__init__("Sacrifice", f"Sanctify weapon with your blood to deal bonus holy damage. Also deals additional damage to undead monsters", 8, "sacrifice")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        monster_target: monster.Monster = target_target
        total_bonus_damage = self._bonus_sacrifice_damage
        if monster_target.has_monster_type(monster.MonsterType.UNDEAD):
            total_bonus_damage += self.caster.level + self._bonus_undead_damage

        assert self._self_sacrifice_damage < self.caster.hp, "Not enough health"
        enhanced_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
        enhanced_weapon.set_name("Enhanced " + enhanced_weapon.get_name())
        enhanced_weapon.attack_power += total_bonus_damage
        attack_message = await self.caster.attack_monster_with_weapon(target_target, enhanced_weapon)
        attack_message.ability_extra = f"{self.bold_name_with_colon()} Bonus damage to monster: {total_bonus_damage}; Self damage: {self.caster.receive_damage_from_monster(self._self_sacrifice_damage, self.caster.name, new_line=False)}"
        self.use_ability_text += str(attack_message)


class GreaterHeal(Heal):
    def __init__(self):
        super().__init__()
        self.name = "Greater Heal"
        self.description = f"A potent healing spell"
        self.mana_cost = 15
        self.magic_word = "gheal"
        self._heal_limit = 60
        self.floor_heal = 30


# Monster Hunter abilities
class TurnWeaponToSilver(Ability):
    _bonus_silver_damage_range = range(5, 9)  # 5-8

    def __init__(self):
        super().__init__("Turn weapon to silver", "Turn your weapon's material into silver to deliver a devastating blow to the exotic monster", 6, "silver")
        self.only_target_monster = True

    async def target_effect(self, target_target: target.Target):
        monster_target: monster.Monster = target_target
        valid_types = [monster.MonsterType.UNDEAD, monster.MonsterType.DEMON, monster.MonsterType.DRAGON, monster.MonsterType.MAGIC_CREATURE]
        if monster_target.has_monster_type_from_list(valid_types):
            bonus_damage = random.choice(self._bonus_silver_damage_range)
            silver_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
            silver_weapon.set_name("Silver " + silver_weapon.get_name())
            silver_weapon.attack_power += bonus_damage
            attack_message = await self.caster.attack_monster_with_weapon(monster_target, silver_weapon)
            attack_message.ability_extra = f"{self.bold_name_with_colon()} You struck {monster_target.name} with your {silver_weapon.get_name()} for {bonus_damage} bonus damage!"
            self.use_ability_text += str(attack_message)
        else:
            raise TypeError(f"Can only target {', '.join(str(x) for x in valid_types)} (monster is of type {monster_target.monster_types_str()})")


class OverSizedBearTrap(Ability):
    _stun_duration = 1
    _bear_trap_damage = 5
    _bear_trap_bleed_duration = 3
    _bear_trap_bleed_damage = 3

    def __init__(self):
        super().__init__("Over sized bear trap", f"Place a trap for the monster to step into, stunning it and making it bleed in the process", 12, "bear")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        target_target.debuff_stun(self._stun_duration)
        enhanced_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
        enhanced_weapon.attack_power = self._bear_trap_damage
        enhanced_weapon.attack_message = f"{target_target.name} walked into your over sized bear trap which stunned it, made it bleed,"
        target_target.debuff_dmg_over_time(self._bear_trap_bleed_duration, self._bear_trap_bleed_damage, self.caster)
        attack_message = await self.caster.attack_monster_with_weapon(target_target, enhanced_weapon, retaliate=False)
        attack_message.ability_extra = f"{self.bold_name_with_colon()} {target_target.name} will bleed for {self._bear_trap_bleed_duration} turns."
        self.use_ability_text += str(attack_message)


# Extra dmg, gain bounty on kill, easier to crit monster
class BigGameTrophy(Ability):
    _extra_crit_duration = 2
    _extra_crit_chance = 25
    _extra_damage = 6

    def __init__(self):
        super().__init__("Big game trophy", f"Claim the monster's head as your trophy. Making it easier to crit, and killing it gives you bonus gold.", 14, "trophy")
        self.only_target_monster = True

    async def target_effect(self, target_target: monster.Monster):
        target_target.debuff_easier_to_crit(self._extra_crit_duration, self._extra_crit_chance)
        enhanced_weapon: weapon.Weapon = copy.deepcopy(self.caster.weapon)
        enhanced_weapon.attack_power += self._extra_damage
        attack_message = await self.caster.attack_monster_with_weapon(target_target, enhanced_weapon)
        if target_target.is_dead():
            bonus_gold = math.floor(target_target.level * 1.25)
            if bonus_gold < 10:
                bonus_gold = 10
            attack_message.ability_extra = f"{self.bold_name_with_colon()} You dealt {self._extra_damage} and gained a gold bonus! You earned {bonus_gold} trophy gold!"
            self.caster.gold += bonus_gold
        else:
            attack_message.ability_extra = f"{self.bold_name_with_colon()} You dealt {self._extra_damage} bonus damage, and made {target_target.name} easier to crit!"
        self.use_ability_text += str(attack_message)
