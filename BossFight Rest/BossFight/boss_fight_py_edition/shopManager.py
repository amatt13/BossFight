import math

import discord

import source.games.boss_fight.statics as statics
from source.misc import util
from source.games.boss_fight import weapon, gameManager, playerClass, lootItem

current_shop_weapons: [weapon.Weapon] = weapon.WeaponList.BASIC_WEAPONS
weapons_sorted_by_cost: [weapon.Weapon] = sorted(current_shop_weapons, key=lambda w: w.cost)
classes_that_can_be_purchased: [playerClass.PlayerClass] = [playerClass.Hexer(None), playerClass.Mage(None), playerClass.Barbarian(None), playerClass.Paladin(None), playerClass.MonsterHunter(None)]


async def get_shop_message() -> discord.Message:
    messages = await util.boss_shop_channel.history(limit=1, oldest_first=True).flatten()
    return messages[0]


async def update_shop():
    shop_message = await get_shop_message()

    classes_sorted_by_cost = sorted(classes_that_can_be_purchased, key=lambda c: c.purchase_price)

    len_of_longest_player_name = max(len(p.name) for p in gameManager.global_game_manager.players)
    len_of_longest_player_gold = max(len(str(p.gold)) + math.floor(len(str(p.gold)) / 3) for p in gameManager.global_game_manager.players)
    len_of_longest_player_class_name = max(len(pc.name) for pc in classes_sorted_by_cost)
    len_of_longest_player_class_cost = max(len(str(pc.purchase_price)) for pc in classes_sorted_by_cost)
    len_of_longest_w_name = max(len(wep.get_name()) for wep in current_shop_weapons)
    len_of_longest_w_type = max(len(wep.get_weapon_type_str()) for wep in current_shop_weapons)
    longest_w_attack_digit = max(len(str(wep.attack_power)) for wep in current_shop_weapons)
    longest_w_spell_power_digit = max(len(str(wep.spell_power)) for wep in current_shop_weapons)
    longest_w_gold_price_digit = max(len(str(wep.cost)) for wep in current_shop_weapons)
    longest_w_crit_chance_digit = max(len(str(wep.attack_crit_chance)) for wep in current_shop_weapons)
    longest_w_spell_crit_chance_digit = max(len(str(wep.spell_crit_chance)) for wep in current_shop_weapons)

    shop_text = "Players' gold"
    shop_text += "\n```python\n"
    shop_text += "\n".join(p.shop_str(len_of_longest_player_name, len_of_longest_player_gold) for p in gameManager.global_game_manager.players)
    shop_text += "\n```"
    shop_text += "\nType 'buy <name>' to place an weapon into your inventory"
    shop_text += "\n```python\n"
    shop_text += f"\n{'Weapon name'.ljust(len_of_longest_w_name)}\n"
    shop_text += "\n".join(f"{w.shop_str(len_of_longest_w_name, len_of_longest_w_type, longest_w_attack_digit, longest_w_gold_price_digit, longest_w_crit_chance_digit, longest_w_spell_power_digit, longest_w_spell_crit_chance_digit)}" for idx, w in enumerate(weapons_sorted_by_cost))
    shop_text += "\n```"
    shop_text += "\nType 'info <class name>' to get information about the class"
    shop_text += "\nType 'buy <class name>' to buy an class"
    shop_text += "\n```python\n"
    shop_text += f"\n{'Name'.ljust(len_of_longest_player_class_name)} {'Gold cost'.ljust(len_of_longest_player_class_cost)}  Requirements\n"
    shop_text += "\n".join(pc.shop_str(len_of_longest_player_class_name, len_of_longest_player_class_cost) for pc in classes_sorted_by_cost)
    shop_text += "\n```"
    shop_text += "\n"
    shop_text += "\nType 'sell <loot name>' to sell an item from your inventory"
    shop_text += "\nType 'autosell <loot name>' to add/remove an item from your auto-sell list"
    await shop_message.edit(content=shop_text)


def buy_weapon(loot_name: str, player_message: discord.Message) -> str:
    loot_name = loot_name.lower()
    try:
        item_to_purchase: lootItem = next(w for w in weapons_sorted_by_cost if w.loot_name.lower() == loot_name)
    except StopIteration:
        raise statics.LootNotFoundException(f"Could not find item '{loot_name}'")
    purchasing_player = next(p for p in gameManager.global_game_manager.players if p.player_id == str(player_message.author.id))
    if item_to_purchase.loot_id in purchasing_player.loot:
        reply_message = f"You already own '{item_to_purchase.loot_name}'"
    elif item_to_purchase.cost > purchasing_player.gold:
        reply_message = f"You can't afford that item. Current gold: {purchasing_player.gold:,}, item cost: {item_to_purchase.cost:,}"
    else:
        purchasing_player.gold -= item_to_purchase.cost
        purchasing_player.add_loot(item_to_purchase.loot_id)
        gameManager.global_game_manager.persist_changes()
        reply_message = f"You bough the {item_to_purchase.loot_name} for {item_to_purchase.cost:,} gold"
    return reply_message


def buy_class(class_name: str, player_message: discord.Message) -> str:
    # check class exists
    try:
        class_to_purchase = next(i for i in classes_that_can_be_purchased if i.name.lower() == class_name.lower())
    except StopIteration:
        raise statics.PlayerClassNotFoundException(f"Could not find class '{class_name}'")
    purchasing_player = next(p for p in gameManager.global_game_manager.players if p.player_id == str(player_message.author.id))
    # check player haven't already unlocked class
    assert class_to_purchase.player_class_id not in [pc.player_class_id for pc in purchasing_player.player_class_list], "You have already acquired that class"
    # check player can afford class
    assert purchasing_player.gold >= class_to_purchase.purchase_price, f"You can't afford '{class_to_purchase.name}'. Current gold: {purchasing_player.gold:,}, item cost: {class_to_purchase.purchase_price:,}"
    # check player have fulfilled class requirements
    class_to_purchase.have_met_requirements(purchasing_player.player_class_list)

    # make the actual purchase
    purchasing_player.gold -= class_to_purchase.purchase_price
    purchasing_player.player_class_list.append(class_to_purchase.__class__(purchasing_player))
    gameManager.global_game_manager.persist_changes()
    buy_message = f"You bough the {class_to_purchase.name} for {class_to_purchase.purchase_price:,} gold"
    return buy_message


def buy_item(thing_to_buy_id: str, player_message: discord.Message) -> str:
    # reply_message = buy_weapon(int(item_id), player_message)
    try:
        reply_message = buy_class(thing_to_buy_id, player_message)
    except AssertionError as ae:
        reply_message = f"Failed to acquire class: {ae}"
    except statics.PlayerClassNotFoundException as pc_not_found:
        try:
            reply_message = buy_weapon(str(thing_to_buy_id), player_message)
        except statics.LootNotFoundException as loot_not_found:
            reply_message = f"{pc_not_found}\n{loot_not_found}"
    return reply_message


def sell_item(loot_name: str, player_message: discord.Message) -> str:
    try:
        selling_player = next(p for p in gameManager.global_game_manager.players if p.player_id == str(player_message.author.id))
        weapon_to_sell = statics.find_loot_by_name(loot_name)
        assert weapon_to_sell.loot_id in selling_player.loot, f"You don't own '{weapon_to_sell.get_name()}'"
        mes = selling_player.sell_loot(weapon_to_sell)
    except (StopIteration, AssertionError) as err:
        mes = str(err)
    return mes


def auto_sell_item(loot_name: str, player_message: discord.Message) -> str:
    try:
        auto_selling_player = next(p for p in gameManager.global_game_manager.players if p.player_id == str(player_message.author.id))
        weapon_to_update_auto_sell_status = statics.find_loot_by_name(loot_name)
        if weapon_to_update_auto_sell_status.loot_id != weapon.WeaponList.fists.loot_id:
            if auto_selling_player.loot_is_in_auto_sell_list(weapon_to_update_auto_sell_status.loot_id):
                auto_selling_player.auto_sell_list.remove(weapon_to_update_auto_sell_status.loot_id)
                mes = f"Removed '{weapon_to_update_auto_sell_status.loot_name}' from auto-sell list"
            else:
                auto_selling_player.auto_sell_list.append(weapon_to_update_auto_sell_status.loot_id)
                mes = f"Added '{weapon_to_update_auto_sell_status.loot_name}' to auto-sell list"
        else:
            mes = "Loot could not be added/removed from auto-sell list"
    except (StopIteration, AssertionError) as err:
        mes = str(err)
    return mes


async def handle_message(message: discord.Message):
    if not message.author.bot:
        gameManager.global_game_manager.add_player_if_missing(message.author)
        message_text = str(message.content).lower()
        if message_text.startswith("buy "):
            reply_message = buy_item(message_text.replace("buy ", '').strip(), message)
            await message.reply(reply_message)
            await update_shop()
        elif message_text.startswith("info "):
            reply_message = playerClass.display_info(message_text.replace("info ", '').strip())
            await message.reply(reply_message)
        elif message_text.startswith("sell "):
            reply_message = sell_item(message_text.replace("sell ", '').strip(), message)
            await message.reply(reply_message)
        elif message_text.startswith("autosell "):
            reply_message = auto_sell_item(message_text.replace("autosell ", '').strip(), message)
            await message.reply(reply_message)
        gameManager.global_game_manager.persist_changes()
        await util.clean_up_channel(message)
