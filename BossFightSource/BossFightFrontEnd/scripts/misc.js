function NewMonster(new_monster_dict) {
	const monsterWasKilledMessage = new_monster_dict["monsterWasKilledMessage"];
	const monsterDamageInfo = new_monster_dict["monsterDamageInfo"];
	LogToGeneralLog(monsterWasKilledMessage);
	LogToGeneralLog(monsterDamageInfo);

	const newMonsterInstance_dict = new_monster_dict["newMonsterInstance"];
	const monster = Monster.createFromDict(newMonsterInstance_dict);
	UpdateUiActiveMonster(monster, true)
}
