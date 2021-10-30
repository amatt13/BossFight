from typing import Optional
import discord
import source.games.boss_fight.gameManager as gameManager
import source.games.boss_fight.statics as statics
from source.games.boss_fight import playerClass
from source.misc import util


async def refresh_status_messages(player_id: str = None):
    messages = await util.get_channel_messages(util.boss_status_channel, 100)
    filtered_players = [p for p in gameManager.global_game_manager.players if player_id is None or player_id == p.player_id]
    for p in filtered_players:
        found_message = False
        existing_message = None
        for m in messages:
            if found_message:
                break
            if m.author.bot and len(m.mentions) > 0:
                for member in m.mentions:
                    if p.player_id == str(member.id):
                        existing_message = m
                        break
        await show_player_status(p, existing_message)


async def show_player_status(status_player, message: Optional[discord.Message]):
    inventory_text = f"<@!{status_player.player_id}> ({ status_player.hp }/{ status_player.player_class.max_hp } hp - { status_player.mana }/{ status_player.player_class.max_mana } mana)" \
                     f"\nLevel { status_player.player_class.level } { status_player.player_class.name } - { statics.xp_needed_to_next_level(status_player.player_class) } xp to next level" \
                     f"\nCrit chancce: {status_player.get_attack_crit_chance()}%" \
                     f"\nGold: { status_player.gold }" \
                     f"\nSpell list:" \
                     f"\n{ '; '.join(str(status_player.player_class.abilities[a]) for a in status_player.player_class.abilities.keys()) }"\
                     f"\nEquipped weapon: { status_player.weapon.inventory_str() }"
    if len(status_player.loot):
        inventory_text += "\nInventory:\n"
        inventory_text += "\n".join(statics.find_weapon_by_weapon_id(w).inventory_str() for w in sorted(status_player.loot))
    if message is None:
        await util.boss_status_channel.send(inventory_text)
    else:
        await message.edit(content=inventory_text)


async def equip_item(item_to_equip_id: str, message: discord.Message):
    equip_player = gameManager.global_game_manager.find_player_by_id(str(message.author.id))
    try:
        equip_player.change_weapon(int(item_to_equip_id))
        msg = f"You equipped '{ equip_player.weapon.loot_name}'"
        if not equip_player.is_proficient_with_weapon(equip_player.weapon):
            msg += f"\n**Warning: you are not proficient with weapon type '{equip_player.weapon.weapon_type}'**"
        await message.reply(msg)
    except StopIteration:
        await message.reply(f"Could not find item '{ item_to_equip_id }'")
    except ValueError:
        await message.reply(f"'{ item_to_equip_id }' is not a number")


async def change_players_class(class_id, message: discord.Message):
    player = gameManager.global_game_manager.find_player_by_id(str(message.author.id))
    if len(str(class_id)) > 0 and str(class_id) != "info":
        # change class
        try:
            player.change_class(class_id)
            await message.reply(f"You have changed your class to { player.player_class.name }")
        except AssertionError as ae:
            await message.reply(f"Could not change class: { ae }")
    else:
        # print class info
        info = player.unlocked_classes_info()
        await message.reply(info)


async def handle_message(message: discord.Message):
    if not message.author.bot:
        gameManager.global_game_manager.add_player_if_missing(message.author)
        message_content = str(message.content).lower()
        if message_content.startswith("equip "):
            await equip_item(message_content.replace("equip ", '').strip(), message)
        elif message_content.startswith("class"):
            await change_players_class(message_content.replace("class", '').strip(), message)
        if message_content.startswith("info "):
            reply_message = playerClass.display_info(message_content.replace("info ", '').strip())
            await message.reply(reply_message)
        gameManager.global_game_manager.persist_changes()
        await util.clean_up_channel(message)
