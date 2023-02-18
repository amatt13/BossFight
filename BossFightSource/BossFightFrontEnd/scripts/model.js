
class Weapon {
	constructor(WeaponType, AttackMessage, BossWeapon, WeaponLvl, AttackPower, AttackCritChance, SpellPower, SpellCritChance, LootId, LootName, LootDropChance, Cost) {
		this.weapon_type = WeaponType
		this.attack_message = AttackMessage
		this.boss_weapon = BossWeapon
		this.weapon_lvl = WeaponLvl
		this.attack_power = AttackPower
		this.attack_crit_chance = AttackCritChance
		this.spell_power = SpellPower
		this.spell_crit_chance = SpellCritChance
		this.loot_id = LootId
		this.loot_name = LootName
		this.loot_drop_chance = LootDropChance
		this.cost = Cost
	}

	static CreateFromDict(weapon_dict) {
		const weapon = new Weapon(weapon_dict["WeaponType"], weapon_dict["AttackMessage"], weapon_dict["BossWeapon"], weapon_dict["WeaponLvl"], weapon_dict["AttackPower"], weapon_dict["AttackCritChance"], weapon_dict["SpellPower"], 
			weapon_dict["SpellCritChance"], weapon_dict["LootId"], weapon_dict["LootName"], weapon_dict["LootDropChance"], weapon_dict["Cost"]);
		return weapon;
	}
}

class PlayerPlayerClass {
	constructor(xp, level, active, max_hp, max_mana, player_class_name, xp_to_next_level, player_class) {
		this.xp = xp;
		this.level = level;
		this.active = active;
		this.max_hp = max_hp;
		this.max_mana = max_mana;
		this.player_class_name = player_class_name;
		this.xp_to_next_level = xp_to_next_level;
		this.player_class = player_class;
	}

	static CreateFromDict(player_player_class_dict) {
		const player_class = PlayerClass.CreateFromDict(player_player_class_dict["PlayerClass"]);
		const player_player_class = new PlayerPlayerClass(player_player_class_dict["XP"], player_player_class_dict["Level"], player_player_class_dict["Active"], player_player_class_dict["MaxHp"], player_player_class_dict["MaxMana"], player_player_class_dict["PlayerClassName"], player_player_class_dict["XpNeededToNextLevel"], player_class);
		return player_player_class;
	}
}

class PlayerWeapon {
	constructor(weapon_id, name) {
		this.weapon_id = weapon_id;
		this.name = name;
	}
}

class Monster {
	constructor(hp, max_hp, level, monster_name, is_boss_monster, monster_instance_id, blind_duration, easier_to_crit_duration, easier_to_crit_Percentage, lower_attack_duration, lower_attack_percentage, stun_duration, attack_strength) {
		this.hp = hp;
		this.max_hp = max_hp;
		this.level = level;
		this.monster_name = monster_name;
		this.is_boss_monster = is_boss_monster;
		this.monster_instance_id = monster_instance_id;
		this.blind_duration = blind_duration;
		this.easier_to_crit_duration = easier_to_crit_duration;
		this.easier_to_crit_Percentage = easier_to_crit_Percentage;
		this.lower_attack_duration = lower_attack_duration;
		this.lower_attack_percentage = lower_attack_percentage;
		this.stun_duration = stun_duration;
		this.attack_strength = attack_strength;
	}
}

class Player {
	constructor(player_id, name, hp, mana, gold, weapon, player_player_class, player_weapon_list, user_name) {
		this.player_id = player_id
		this.name = name;
		this.hp = hp;
		this.mana = mana;
		this.gold = gold;
		this.player_class = null;
		this.weapon = weapon
		this.player_player_class = player_player_class;
		this.player_weapon_list = player_weapon_list
		this.user_name = user_name;
	}

	static CreateFromDict(playerDict_dict) {
		const weapon_dict = playerDict_dict["Weapon"];
		const weapon = Weapon.CreateFromDict(weapon_dict);

		const player_player_class_dict = playerDict_dict["PlayerPlayerClass"];
		const player_player_class = PlayerPlayerClass.CreateFromDict(player_player_class_dict);

		let player_weapon_list = [];
		const player_weapon_dict = playerDict_dict["PlayerWeaponList"];
		player_weapon_dict.forEach(pw => {
			player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
		});
		const player = new Player(playerDict_dict["PlayerId"], playerDict_dict["Name"], playerDict_dict["Hp"], playerDict_dict["Mana"], playerDict_dict["Gold"], weapon, player_player_class, player_weapon_list, playerDict_dict["UserName"]);
		return player;
	}

	IsAlive() {
		return this.hp != undefined && this.hp > 0;
	}
}

class ChatMessage {
	constructor(chat_message_id, player_name, message_content, timestamp) {
		this.chat_message_id = chat_message_id
		this.player_name = player_name
		this.message_content = message_content
		this.timestamp = timestamp
	}
}

class PlayerClass {
	constructor(Abilities, AttackPowerBonus, BaseHealth, BaseMana, CritChance, HpRegenRate, HpScale, ManaRegenRate, ManaScale, Name, PlayerClassId, PlayerClassRequirementList, ProficientWeaponTypesList, PurchasePrice, SpellPowerBonus, Description) {
		this.abilities = Abilities
		this.attack_power_bonus = AttackPowerBonus
		this.base_health = BaseHealth
		this.base_mana = BaseMana
		this.crit_chance = CritChance
		this.hp_regen_rate = HpRegenRate
		this.hp_scale = HpScale
		this.mana_regen_rate = ManaRegenRate
		this.mana_scale = ManaScale
		this.name = Name
		this.player_class_id = PlayerClassId
		this.player_class_requirement_list = PlayerClassRequirementList  // TODO (currently just a dict)
		this.proficient_weapon_types_list = ProficientWeaponTypesList  // TODO (currently just a dict)
		this.purchase_price = PurchasePrice
		this.spell_power_bonus = SpellPowerBonus
		this.description = Description
	}

	static CreateFromDict(player_class_dict) {
		return new PlayerClass(player_class_dict["Abilities"], player_class_dict["AttackPowerBonus"], player_class_dict["BaseHealth"], player_class_dict["BaseMana"], player_class_dict["CritChance"], player_class_dict["HpRegenRate"], 
			player_class_dict["HpScale"], player_class_dict["ManaRegenRate"], player_class_dict["ManaScale"], player_class_dict["Name"], player_class_dict["PlayerClassId"], player_class_dict["PlayerClassRequirementList"], 
			player_class_dict["ProficientWeaponTypesList"], player_class_dict["PurchasePrice"], player_class_dict["SpellPowerBonus"], player_class_dict["Description"])
	}
}


class MonsterTierVoteChoice
{
	static DECREASE_DIFFICULTY = -1
	static UNCHANGED = 0
	static INCREASE_DIFFICULTY = 1
}
