import math
from datetime import timedelta, datetime
from typing import Callable

from source.games.boss_fight.weapon import WeaponList, Weapon


# return a negative value if above required amount
def xp_needed_to_next_level(player_class):
    # xp_needed = (lvl*(1+lvl))^1.5
    xp_needed = math.floor(math.pow(player_class.level*(1+player_class.level), 1.5))
    xp_needed -= player_class.xp
    return xp_needed


def calculate_experience_from_damage_dealt_to_monster(damage_dealt: int, monster) -> int:
    xp = 1
    xp += math.floor(damage_dealt * 1.10)
    xp += math.floor(monster.level / 3)
    if monster.boss_monster:
        xp += math.ceil(xp * 0.2)
    return xp


def find_weapon_by_weapon_id(weapon_id) -> Weapon:
    weapon = WeaponList.fists
    try:
        if isinstance(weapon_id, int):
            weapon = next(w for w in WeaponList.ALL_WEAPONS if w.loot_id == weapon_id)
    except StopIteration:
        print(f"Could not find weapon with id: {weapon_id}")
    return weapon


def find_loot_by_name(loot_name: str) -> Weapon:
    loot_name = loot_name.lower()
    weapon = WeaponList.fists
    try:
        if isinstance(loot_name, str):
            weapon = next(w for w in WeaponList.ALL_WEAPONS if w.loot_name.lower() == loot_name)
    except StopIteration:
        print(f"Could not find weapon with name: {loot_name}")
    return weapon


def calc_xp_penalty(xp: int, p_lvl: int, m_lvl: int):
    if m_lvl is None:
        m_lvl = p_lvl

    if m_lvl > p_lvl + 2:
        xp = xp * (p_lvl / m_lvl)
    elif m_lvl < p_lvl:
        levels_below = p_lvl - m_lvl
        if levels_below == 6:
            xp = xp * 0.81
        elif levels_below == 7:
            xp = xp * 0.62
        elif levels_below == 8:
            xp = xp * 0.43
        elif levels_below == 9:
            xp = xp * 0.24
        elif levels_below >= 10:
            xp = xp * 0.05
    return math.ceil(xp)


def update_players_health_and_mana_helper(players: [], timestamp: str, minutes_between_ticks: int, regen_func: Callable[[object, int], None]) -> str:
    previous_timestamp = datetime.fromisoformat(timestamp)
    now = datetime.now()
    minutes_diff = math.floor((now - previous_timestamp).total_seconds() / 60)
    ticks = math.floor(minutes_diff / minutes_between_ticks)  # 1 tick every minutes_between_ticks minutes
    for p in players:
        regen_func(p, ticks)
    minutes_to_remove = minutes_diff % minutes_between_ticks
    now = now - timedelta(minutes=minutes_to_remove)
    return now.strftime("%Y-%m-%d:%H:%M:00")


def regen_player_health(p, times_to_regen=1):
    p.regen_health(times_to_regen)


def regen_player_mana(p, times_to_regen=1):
    p.regen_mana(times_to_regen)


class WTFException(Exception):
    pass


class PlayerClassNotFoundException(Exception):
    pass


class LootNotFoundException(Exception):
    pass
