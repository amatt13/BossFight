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
