
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

class MonsterTemplate {
	constructor(tier, name, boss_monster, monster_type_list) {
		this.tier = tier;
		this.name = name;
		this.boss_monster = boss_monster;
		this.monster_type_list = monster_type_list;
	}

	static CreateFromDict(monster_template_dict) {
		return new MonsterTemplate(monster_template_dict["Tier"], monster_template_dict["Name"], monster_template_dict["BossMonster"], monster_template_dict["MonsterTypeStringList"]);
	}
}

class Monster {
	constructor(hp, max_hp, level, monster_name, is_boss_monster, monster_instance_id, attack_strength, monster_template) {
		this.hp = hp;
		this.max_hp = max_hp;
		this.level = level;
		this.monster_name = monster_name;
		this.is_boss_monster = is_boss_monster;
		this.monster_instance_id = monster_instance_id;
		this.attack_strength = attack_strength;
		this.monster_template = monster_template;
	}

	static CreateFromDict(monster_dict) {
		const monster_template_dict = monster_dict["MonsterTemplate"];
		const monster_template = MonsterTemplate.CreateFromDict(monster_template_dict);
		return new Monster(monster_dict["Hp"], monster_dict["MaxHp"], monster_dict["Level"], monster_dict["Name"], monster_dict["IsBossMonster"], monster_dict["MonsterInstanceId"], monster_dict["AttackStrength"], monster_template);
	}
}

class Player {
	constructor(player_id, name, hp, mana, gold, weapon, player_player_class, player_weapon_list, user_name, preffered_body_type) {
		this.player_id = player_id
		this.name = name;
		this.hp = hp;
		this.mana = mana;
		this.gold = gold;
		this.weapon = weapon
		this.player_player_class = player_player_class;
		this.player_weapon_list = player_weapon_list
		this.user_name = user_name;
		this.preffered_body_type = preffered_body_type
	}

	static CreateFromDict(playerDict_dict) {
		const weapon_dict = playerDict_dict["Weapon"];
		const weapon = Weapon.CreateFromDict(weapon_dict);

		const player_player_class_dict = playerDict_dict["PlayerPlayerClass"];
		const player_player_class = PlayerPlayerClass.CreateFromDict(player_player_class_dict);

		const preffered_body_type_dict = playerDict_dict["PrefferedBodyType"];
		const preffered_body_type = BodyType.CreateFromDict(preffered_body_type_dict);

		let player_weapon_list = [];
		const player_weapon_dict = playerDict_dict["PlayerWeaponList"];
		player_weapon_dict.forEach(pw => {
			player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
		});
		const player = new Player(playerDict_dict["PlayerId"], playerDict_dict["Name"], playerDict_dict["Hp"], playerDict_dict["Mana"], playerDict_dict["Gold"], weapon, player_player_class, player_weapon_list, playerDict_dict["UserName"],
								  preffered_body_type);
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

	static CreateFromDict(chat_message_dict) {
		return new ChatMessage(chat_message_dict["ChatMessageId"], chat_message_dict["PlayerName"], chat_message_dict["MessageContent"], Date.parse(chat_message_dict["Timestamp"]));
	}
}

class PlayerClassRequirement {
	constructor(PlayerClassId, RequiredPlayerClassId, LevelRequirement, RequiredPlayerClassName) {
		this.player_class_id = PlayerClassId;
		this.required_player_class_id = RequiredPlayerClassId;
		this.level_requirement = LevelRequirement;
		this.required_player_class_name = RequiredPlayerClassName;
	}

	static CreateFromDict(player_class_requirement_dict) {
		return new PlayerClassRequirement(player_class_requirement_dict["PlayerClassId"], player_class_requirement_dict["RequiredPlayerClassId"], player_class_requirement_dict["LevelRequirement"], player_class_requirement_dict["RequiredPlayerClassName"]);
	}
}

class PlayerClass {
	constructor(Abilities, AttackPowerBonus, BaseHealth, BaseMana, CritChance, HpRegenRate, HpScale, ManaRegenRate, ManaScale, Name, PlayerClassId, PlayerClassRequirementList, ProficientWeaponTypesList, PurchasePrice, SpellPowerBonus, Description) {
		this.abilities = Abilities;
		this.attack_power_bonus = AttackPowerBonus;
		this.base_health = BaseHealth;
		this.base_mana = BaseMana;
		this.crit_chance = CritChance;
		this.hp_regen_rate = HpRegenRate;
		this.hp_scale = HpScale;
		this.mana_regen_rate = ManaRegenRate;
		this.mana_scale = ManaScale;
		this.name = Name;
		this.player_class_id = PlayerClassId;
		this.player_class_requirement_list = PlayerClassRequirementList;
		this.proficient_weapon_types_list = ProficientWeaponTypesList;  // TODO (currently just a dict)
		this.purchase_price = PurchasePrice;
		this.spell_power_bonus = SpellPowerBonus;
		this.description = Description;
	}

	static CreateFromDict(player_class_dict) {
		let player_class_requirement_list = [];
		const player_class_requirement_dict = player_class_dict["PlayerClassRequirementList"];
		player_class_requirement_dict.forEach(pcr => {
			player_class_requirement_list.push(new PlayerClassRequirement(pcr["PlayerClassId"], pcr["RequiredPlayerClassId"], pcr["LevelRequirement"], pcr["RequiredPlayerClassName"]))
		});
		let abilities_list = [];
		const unlocked_abilities_dict = player_class_dict["UnlockedAbilities"];
		unlocked_abilities_dict.forEach(ability_dict => {
			abilities_list.push(Ability.CreateFromDict(ability_dict));
		});

		return new PlayerClass(abilities_list, player_class_dict["AttackPowerBonus"], player_class_dict["BaseHealth"], player_class_dict["BaseMana"], player_class_dict["CritChance"], player_class_dict["HpRegenRate"],
			player_class_dict["HpScale"], player_class_dict["ManaRegenRate"], player_class_dict["ManaScale"], player_class_dict["Name"], player_class_dict["PlayerClassId"], player_class_requirement_list,
			player_class_dict["ProficientWeaponTypesList"], player_class_dict["PurchasePrice"], player_class_dict["SpellPowerBonus"], player_class_dict["Description"])
	}
}

class BodyType {
	constructor(body_type_id, name) {
		this.body_type_id = body_type_id;
		this.Name = name;  //TODO remove me
		this.name = name;
	}

	static CreateFromDict(body_type_dict) {
		return new BodyType(body_type_dict["BodyTypeId"], body_type_dict["Name"]);
	}
}

class PlayerAttackSummary {
	constructor(player_instance, MonsterInstance, PlayerTotalDamage, PlayerCrit, MonsterCrit, PlayerExtraDamageFromBuffs, PlayerXpEarned, MonsterAffectedByDots, MonsterRetaliateMessage, PlayerKilledMonster, MonsterTotalDamage) {
		this.player = player_instance;
		this.monster = MonsterInstance;
		this.player_total_damage = PlayerTotalDamage;
		this.player_crit = PlayerCrit;
		this.monster_crit = MonsterCrit;
		this.player_extra_damage_from_buffs = PlayerExtraDamageFromBuffs;
		this.player_xp_earned = PlayerXpEarned;
		this.monster_affected_by_dots = MonsterAffectedByDots;  //TODO delete me?
		this.monster_retaliate_message = MonsterRetaliateMessage;
		this.player_killed_monster = PlayerKilledMonster;
		this.monster_total_damage = MonsterTotalDamage;
	}

	static CreateFromDict(player_attack_summary_dict) {
		const player_dict = player_attack_summary_dict["Player"];
		const monster_dict = player_attack_summary_dict["Monster"];
		const player = Player.CreateFromDict(player_dict);
		const monster = Monster.CreateFromDict(monster_dict);

		return new PlayerAttackSummary(player, monster, player_attack_summary_dict["PlayerTotalDamage"], player_attack_summary_dict["PlayerCrit"], player_attack_summary_dict["MonsterCrit"],
			player_attack_summary_dict["PlayerExtraDamageFromBuffs"], player_attack_summary_dict["PlayerXpEarned"], player_attack_summary_dict["MonsterAffectedByDots"],
			player_attack_summary_dict["MonsterRetaliateMessage"], player_attack_summary_dict["PlayerKilledMonster"], player_attack_summary_dict["MonsterTotalDamage"]);
	}
}

class Ability {
	constructor(name, description, only_target_monster, mana_cost, affects_all_players, ability_cast_key) {
		this.name = name;
		this.ability_cast_key = ability_cast_key;
		this.image_source = name.toLowerCase().replace(" ", "");
		this.description = description;
		this.only_target_monster = only_target_monster;
		this.mana_cost = mana_cost;
		this.affects_all_players = affects_all_players;
	}

	static CreateFromDict(ability_dict) {
		return new Ability(ability_dict["Name"], ability_dict["Description"], ability_dict["OnlyTargetMonster"], ability_dict["ManaCost"], ability_dict["AffectsAllPlayers"], ability_dict["AbilityCastKey"]);
	}
}

class PlayerInformation {
	constructor(player_name, player_level, player_id, player_class_name, current_hp, max_hp, current_mana, max_mana, preffered_body_type) {
		this.name = player_name;
		this.level = player_level;
		this.player_id = player_id;
		this.player_class_name = player_class_name;
		this.current_hp = current_hp;
		this.max_hp = max_hp;
		this.current_mana = current_mana;
		this.max_mana = max_mana;
		this.preffered_body_type = preffered_body_type;
		this.image_source = getPlayerClassSprite(player_class_name, preffered_body_type.Name);
	}

	static CreateFromDict(info_dict) {
		const body_type = BodyType.CreateFromDict(info_dict["PrefferedBodyType"])
		return new PlayerInformation(info_dict["Name"], info_dict["Level"], info_dict["PlayerId"], info_dict["PlayerClassName"], info_dict["CurrentHp"], info_dict["MaxHp"], info_dict["CurrentMana"], info_dict["MaxMana"], body_type);
	}
}

class MonsterTierVoteChoice
{
	static DECREASE_DIFFICULTY = -1;
	static UNCHANGED = 0;
	static INCREASE_DIFFICULTY = 1;
}
