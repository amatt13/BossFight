import datetime
import re
import source.misc.util as util
import source.games.boss_fight.statics as statics
import source.games.boss_fight.gameManager as gameManager
from source.games.boss_fight.player import Player
from source.games.boss_fight.target import Target
import discord


# TODO
#   Tilføj potions (ting som man blive ved med at bruge guld på)
#   (Ikke alle classes kan bruge alle våben? Fjern/equip våben i player.change_class)
#   Boss monstre skal kunne kaste spells
class FightManager:
    def __init__(self):
        self.game_manager: gameManager.GameManager = gameManager.load_player_manager()

    async def handle_message(self, message: discord.Message):
        if not message.author.bot:
            self.game_manager.add_player_if_missing(message.author)
            message_text = str.lower(message.content)
            if "attack" in message_text:
                await self.attack_monster(message)
                await self.update_boss_status()
            elif message_text.lower().startswith("cast "):
                await self.cast(message)
                await self.update_boss_status()
            await util.clean_up_channel(message, 2)
        self.game_manager.persist_changes()

    async def set_boss_message(self):
        messages = await util.get_channel_messages(util.boss_fight_channel, 1, True)
        boss_status_message = next(mes for mes in messages if mes.author.bot)
        util.boss_status_message = boss_status_message

    def create_knocked_out_message(self, player) -> str:
        knocked_out_message = "You are still knocked out and cannot attack or cast spells/abilities"
        date_one_hp = self.game_manager.datetime_when_player_has_one_hp(player)
        if date_one_hp > datetime.datetime.now():
            knocked_out_message += f"\nYou must wait until {date_one_hp.strftime('%H:%M')}"
        return knocked_out_message

    async def update_boss_status(self):
        current_monster = self.game_manager.current_monster
        embed = discord.Embed(description=str(current_monster), type="rich")
        embed.set_image(url=current_monster.image_url)
        await util.boss_status_message.edit(embed=embed)

    async def attack_monster(self, message: discord.Message):
        attacking_player = self.game_manager.find_player_by_id(str(message.author.id))
        current_monster = self.game_manager.current_monster
        if attacking_player.is_knocked_out():
            knocked_out_message = self.create_knocked_out_message(attacking_player)
            await message.reply(knocked_out_message)
        elif int(current_monster.hp) > 0:
            attack_message = await attacking_player.attack_monster_with_weapon(current_monster, attacking_player.weapon)
            await message.reply(str(attack_message))

    async def cast(self, message: discord.Message):
        caster: Player = self.game_manager.find_player_by_id(str(message.author.id))
        if caster.is_knocked_out():
            knocked_out_message = self.create_knocked_out_message(caster)
            await message.reply(knocked_out_message)
        else:
            target: Target = None
            mentions = message.mentions
            if len(mentions) >= 1:
                target_member: discord.Member = (mentions[0])
                target_member_id = str(target_member.id)
                try:
                    target = self.game_manager.find_player_by_id(target_member_id)
                except StopIteration:
                    target = self.game_manager.add_player_if_missing(target_member)
            cast_text = str(message.clean_content).lower().removeprefix("cast ")
            cast_text = re.sub("@.+$", "", cast_text)
            cast_text = cast_text.strip()
            ability_name = cast_text.replace(" ", "")
            try:
                assert caster.player_class is not None, "Player has no class"
                assert ability_name in caster.player_class.abilities.keys(), f"Spell '{ability_name}' is not available to {caster.player_class.name}"
                ability = caster.player_class.abilities[cast_text]
                if ability.mana_cost >= 1:
                    assert caster.has_enough_mana_for_ability(ability), f"Not enough mana. You have {caster.mana}, {ability.name} requires {ability.mana_cost}"
                if ability.only_target_monster:
                    target = self.game_manager.current_monster
                if ability.affects_all_players:
                    ability.affects_all_players_effect(self.game_manager.players)
                string_result: str = await ability.use_ability(caster, target)
                string_result.replace("\n\n", "\n")
                await message.reply(string_result)
            except TypeError as te:
                await message.reply("Could not cast spell: " + str(te))
            except IndexError:
                await message.reply("Could not cast spell: spell is missing arguments")
            except AssertionError as ae:
                await message.reply(f"Could not cast spell: {ae}")
            except statics.WTFException as wtf:
                await message.reply(f"Could not cast spell: {wtf}")
