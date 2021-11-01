using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.CustemExceptions;
using BossFight.Models.Loot;

namespace BossFight.Models
{
    public class Player : Target
    {
        private static Random _random = new Random();

        public int PlayerId { get; set; }
        public int Gold { get; set; }
        public List<int> LootList { get; set; }
        public int WeaponId { get; set; }
        //public int CurrentPlayerClassId { get; set; }
        public int Mana { get; set; }
        public int BonusMagicDmg { get; set; }
        public int BonusMagicDmgDuration { get; set; }
        public List<PlayerClass> PlayerClassList { get; set; }
        public PlayerClass PlayerClass { get; set; }
        public Weapon Weapon { get; set; }
        public List<int> AutoSellList { get; set; }

        public Player(string pName, int pPlayerId, int pGold = 0, List<int> pLootList = null, int pWeaponId = 1, int pHP = 10, int pPlayerClassId = 0, int pMana = 10, List<PlayerClass> pPlayerClassList = null)
            : base(pName: pName)
        {
            PlayerId = pPlayerId;
            Gold = pGold;
            LootList = pLootList ?? new List<int>();
            WeaponId = pWeaponId;
            //CurrentPlayerClassId = pPlayerClassId;
            Mana = pMana;
            BonusMagicDmg = 0;
            BonusMagicDmgDuration = 0;
            PlayerClassList = pPlayerClassList ?? new List<PlayerClass>();
            //TODO DETERMINE CURRENT PLAYERCLASS ON LOAD
            Level = PlayerClass.Level;
            //Weapon = FindWeaponByWeaponId(pWeaponId);
        }

        // public static Player FetchFromDB(int pPlayerId)
        // {
        //     var reader = DBSingleton.ExecuteQuery(@"SELECT * FROM player WHERE player.player_id = {pPlayerId}");
        //     Player player = null;
        //     if (reader.HasRows)
        //     {
        //         while (reader.Read())
        //         {
        //             Console.WriteLine(reader.GetString(0));
        //         }

        //         p.name = playerDict["name"];
        //         p.playerId = playerDict["playerId"].ToString();
        //         p.gold = Convert.ToInt32(playerDict["gold"]);
        //         p.loot = playerDict["loot"];
        //         p.weaponId = playerDict["weaponId"];
        //         p.hp = Convert.ToInt32(playerDict["hp"]);
        //         p.currentPlayerClassId = Convert.ToInt32(playerDict["currentPlayerClassId"]);
        //         p.mana = Convert.ToInt32(playerDict["mana"]);
        //         p.bonusMagicDmg = Convert.ToInt32(playerDict["bonusMagicDmg"]);
        //         p.bonusMagicDmgDuration = Convert.ToInt32(playerDict["bonusMagicDmgDuration"]);
        //         p.PlayerClassList = new List<object>();
        //         p.autoSellList = playerDict["autoSellList"];
        //         if (playerDict.keys().Contains("PlayerClassList"))
        //         {
        //             var playerClassDictionaries = playerDict["PlayerClassList"];
        //             foreach (var playerClassDict in playerClassDictionaries)
        //             {
        //                 try
        //                 {
        //                     p.PlayerClassList.append(cls.FromDictPlayerClassHelper(playerClassDict, p));
        //                 }
        //                 catch (ValueError)
        //                 {
        //                     quit(ve.ToString());
        //                 }
        //             }
        //         }
        //         p.playerClass = next(from pc in p.PlayerClassList
        //                              where pc.classId == p.currentPlayerClassId
        //                              select pc);
        //         p.level = p.playerClass.level;
        //         p.weapon = GenralHelperFunctions.findWeaponByWeaponId(p.weaponId);
        //     }
        //     return player;
        // }

        // public virtual object toDict()
        // {
        //     var playerDict = new Dictionary<object, object>
        //     {
        //     };
        //     var PlayerClassList = new List<object>();
        //     foreach (var playerClass in PlayerClassList)
        //     {
        //         PlayerClassList.append(playerClass.toDict());
        //     }
        //     playerDict["PlayerClassList"] = PlayerClassList;
        //     playerDict["gold"] = Gold;
        //     playerDict["hp"] = HP;
        //     playerDict["loot"] = LootList;
        //     playerDict["mana"] = Mana;
        //     playerDict["name"] = Name;
        //     playerDict["currentPlayerClassId"] = PlayerClass.classId;
        //     playerDict["playerId"] = PlayerId;
        //     playerDict["weaponId"] = Weapon.lootId;
        //     playerDict["bonusMagicDmg"] = BonusMagicDmg;
        //     playerDict["bonusMagicDmgDuration"] = BonusMagicDmgDuration;
        //     playerDict["autoSellList"] = autoSellList;
        //     return playerDict;
        // }

        // public static PlayerClass fromDictPlayerClassHelper(object playerClassDict, Player p)
        // {
        //     PlayerClass result;
        //     var playerClassName = str(playerClassDict["name"]).ToLower();
        //     if (playerClassName == "cleric")
        //         result = Cleric.FromDB(playerClassDict, p);
        //     else if (playerClassName == "executioner")
        //         result = Executioner.FromDB(playerClassDict, p);
        //     else if (playerClassName == "ranger")
        //         result = Ranger.FromDB(playerClassDict, p);
        //     else if (playerClassName == "hexer")
        //         result = Hexer.FromDB(playerClassDict, p);
        //     else if (playerClassName == "mage")
        //         result = Mage.FromDB(playerClassDict, p);
        //     else if (playerClassName == "barbarian")
        //         result = Barbarian.FromDB(playerClassDict, p);
        //     else if (playerClassName == "paladin")
        //         result = Paladin.FromDB(playerClassDict, p);
        //     else if (playerClassName == "monster hunter")
        //         result = MonsterHunter.FromDB(playerClassDict, p);
        //     else
        //     {
        //         var errorMsg = "Could not convert find class with name " + playerClassName;
        //         Console.WriteLine(errorMsg);
        //         throw new MyException(errorMsg);
        //     }
        //     return result;
        // }

        public override string ToString()
        {
            return Name;
        }

        public virtual object shopStr(int pLengthOfLongestPlayerName, int PengthOfLongestPlayerTotalGold)
        {
            var goldStr = String.Format("{0:n0}", Gold);
            goldStr = $"{ goldStr.Replace(',', '.') }".PadLeft(PengthOfLongestPlayerTotalGold);
            return $"{ Name.PadLeft(pLengthOfLongestPlayerName, '.') } { goldStr } gold";
        }

        public override int GetMaxHp()
        {
            return PlayerClass.MaxHp;
        }

        public int GetMaxMana()
        {
            return PlayerClass.MaxMana;
        }

        public int GetLevel()
        {
            return PlayerClass.Level;
        }

        public bool IsKnockedOut()
        {
            return IsDead();
        }

        public PlayerClass GainXp(int pGainedXp, int? pMonsterLevel = null)
        {
            pGainedXp = GenralHelperFunctions.CalcXpPenalty(pGainedXp, GetLevel(), pMonsterLevel);
            PlayerClass.XP += pGainedXp;
            var xpNeededToNextLevel = GenralHelperFunctions.XpNeededToNextLevel(PlayerClass);
            if (xpNeededToNextLevel <= 0)
            {
                PlayerClass.LevelUp();
                if (xpNeededToNextLevel < 0)
                {
                    GainXp(-xpNeededToNextLevel, pMonsterLevel);
                }
            }
            return PlayerClass;
        }

        public bool HasEnoughManaForAbility(Ability pAbility)
        {
            return Mana >= pAbility.ManaCost;
        }

        public int GetAttackBonus()
        {
            return (int)Math.Floor((double)Level / 2) + BonusMagicDmg + PlayerClass.GetAttackPowerBonus();
        }

        public int GetSpellBonus()
        {
            return (int)Math.Floor((double)Level / 2) + PlayerClass.GetSpellPowerBonus();
        }

        public int GetAttackCritChance()
        {
            var critChance = Weapon.AttackCritChance;
            critChance += PlayerClass.PlayerClassCritChance;
            return critChance;
        }

        public int GetSpellCritChance()
        {
            var critChance = Weapon.SpellCritChance;
            critChance += PlayerClass.PlayerClassCritChance;
            return critChance;
        }

        public bool PlayerAttackIsCrit(int pBonusCritChance = 0)
        {
            var critChance = GetAttackCritChance();
            critChance += pBonusCritChance;
            var roll = _random.Next(0, 101);
            return roll <= critChance;
        }

        public bool PlayerSpellIsCrit(int pBonusCritChance = 0)
        {
            var critChance = GetSpellCritChance();
            critChance += pBonusCritChance;
            var roll = _random.Next(0, 101);
            return roll <= critChance;
        }

        public bool IsProficientWithWeapon(Weapon pWeapon)
        {
            return PlayerClass.ProficientWeaponTypesList.Contains(pWeapon.WeaponType);
        }

        // Throws an exception if the weapon is not found
        public void ChangeWeapon(int pNewWeaponId)
        {
            var itemToEquip = LootList.First(i => i == Convert.ToInt32(pNewWeaponId));
            LootList.Remove(itemToEquip);
            AddLoot(WeaponId);
            WeaponId = itemToEquip;
            //Weapon = GenralHelperFunctions.findWeaponByWeaponId(pNewWeaponId);
        }

        public void ChangeClass(string pNewPlayerClassName)
        {
            var newPlayerClassName = pNewPlayerClassName.ToLower();
            PlayerClass newClass;
            if (newPlayerClassName != PlayerClass.Name.ToLower())
            {
                try
                {
                    newClass = PlayerClassList.First(pc => pc.Name.ToLower() == newPlayerClassName);
                    if (newClass != null)
                    {
                        PlayerClass = newClass;
                        double currentHealthPercentage = Math.Floor((double)HP / PlayerClass.MaxHp);
                        if (currentHealthPercentage > 1.0d)
                            currentHealthPercentage = 1.0d;
                        //CurrentPlayerClassId = newClass.classId;
                        HP = (int)Math.Floor(GetMaxHp() * currentHealthPercentage);
                        if (HP < -3)
                            HP = -3;
                        Mana = 0;
                    }
                }
                catch (InvalidOperationException)
                {
                    PlayerClassList.OrderBy(x => x.Name);
                    var unlockedClasses = String.Join(", ", from pc in PlayerClassList select $"{ pc.Name }");
                    throw new MyException($"Class with name '{ pNewPlayerClassName }' is not unlocked. Your unlocked classes is: { unlockedClasses }");
                }
            }
        }

        public string UnlockedClassesInfo()
        {
            var info = "Unlocked classes\n";
            info += String.Join("\n", from pc in PlayerClassList select $"{ pc.Name } level { pc.Level }\nHp { pc.MaxHp } Mana { pc.MaxMana }");
            return info;
        }

        public bool BuffBonusDamageIfStronger(int bonusDamage, int duration)
        {
            var buffApplied = false;
            if (bonusDamage > BonusMagicDmg || bonusDamage == BonusMagicDmg && duration > BonusMagicDmgDuration)
            {
                BonusMagicDmgDuration = duration;
                BonusMagicDmg = bonusDamage;
                buffApplied = true;
            }
            return buffApplied;
        }

        public bool SubtractBonusMagicDmgDuration(int n)
        {
            var durationSubtracted = false;
            if (BonusMagicDmgDuration > 0)
            {
                BonusMagicDmgDuration -= n;
                durationSubtracted = true;
                if (BonusMagicDmgDuration <= 0)
                {
                    BonusMagicDmgDuration = 0;
                    BonusMagicDmg = 0;
                }
            }
            return durationSubtracted;
        }

        public AttackMessage AttackMonsterWithWeapon(Monster pTargetMonster, Weapon pPlayerWeapon, bool pRetaliate = true)
        {
            var bonusCritChance = 0;
            if (pTargetMonster.EasierToCritDuration > 0)
            {
                bonusCritChance = pTargetMonster.EasierToCritPercentage;
                pTargetMonster.EasierToCritPercentage -= 1;
            }
            var isCrit = PlayerAttackIsCrit(bonusCritChance);
            var damageDealt = pTargetMonster.ReceiveDamageFromPlayer(this, pPlayerWeapon, isCrit);
            var Tup1 = pTargetMonster.ReceiveDamageFromDamageOverTimeEffects();
            int totalDamageOverTime = Tup1.Item1;
            List<string> playersThatDealtDamageOverTime = Tup1.Item2;
            var xpEarned = GenralHelperFunctions.CalculateExperienceFromDamageDealtToMonster(damageDealt, pTargetMonster);
            GainXp(xpEarned, pTargetMonster.Level);
            var attackMessage = new AttackMessage(this, pTargetMonster);
            attackMessage.PlayerCrit = isCrit;
            attackMessage.WeaponAttackMessage = $"{ pPlayerWeapon.AttackMessage } and dealt **{ damageDealt } **damage!";
            attackMessage.PlayerXpEarned = xpEarned;
            if (pTargetMonster.IsAlive() && pRetaliate)
            {
                attackMessage.MonsterRetaliateMessage = pTargetMonster.DealDamageToPlayer(this);
            }
            if (SubtractBonusMagicDmgDuration(1))
            {
                attackMessage.PlayerExtraDamageFromBuffs = true;
            }
            if (playersThatDealtDamageOverTime.Any())
            {
                attackMessage.MonsterAffectedByDots = $"Monster took an additional { totalDamageOverTime } damage from various spell effects by { String.Join(", ", from p in playersThatDealtDamageOverTime select p) }";
            }
            return attackMessage;
        }

        public string ReceiveDamageFromMonster(int pDamage, string pMonsterName, bool pNewLine = true)
        {
            HP -= pDamage;
            var damageText = $"You received { pDamage } damage from { pMonsterName }";
            if (IsKnockedOut())
            {
                if (HP < -3)
                    HP = -3;
                damageText += ", and is knocked out";
            }
            if (pNewLine)
                damageText += "\n";

            return damageText;
        }

        public void AddAllMissingStartingClasses()
        {
            var cleric = new Cleric(this);
            var executioner = new Executioner(this);
            var ranger = new Ranger(this);
            foreach (var pc in new List<PlayerClass> {
                    cleric,
                    executioner,
                    ranger
                })
            {
                // try
                // {
                //     next(from playerClass in PlayerClassList
                //          where playerClass.classId == pc.classId
                //          select playerClass);
                // }
                // catch (StopIteration)
                // {
                //     PlayerClassList.append(pc);
                // }
            }
        }

        public void AddLoot(int pLootToAdd)
        {
            if (LootIsInAutoSellList(pLootToAdd))
            {
                //var wp = GenralHelperFunctions.findWeaponByWeaponId(lootId);
                //Gold += wp.GetSellPrice();
            }
            else
            {
                LootList.Add(pLootToAdd);
                LootList.Sort();
            }
        }

        public void AddLoot(LootItem pLootToAdd)
        {
            int lootId = pLootToAdd.LootId;
            AddLoot(lootId);
        }

        public string sellLoot(LootItem pLootToSell)
        {
            LootList.Remove(pLootToSell.LootId);
            var sellPrice = pLootToSell.GetSellPrice();
            return $"You sold '{ pLootToSell.GetName()}' for { sellPrice } gold";
        }

        public bool LootIsInAutoSellList(int pLootId)
        {
            return AutoSellList.Contains(pLootId);
        }

        public void RestoreAllHealthAndMana()
        {
            HP = PlayerClass.MaxHp;
            Mana = PlayerClass.MaxMana;
        }

        public void RegenHealth(int PTimesToRegen = 1)
        {
            int hpToRegen;
            if (IsKnockedOut())
            {
                hpToRegen = 1 * PTimesToRegen;
            }
            else
                hpToRegen = PlayerClass.GetHealthRegenRate() * PTimesToRegen;

            var newHp = HP + hpToRegen;
            if (newHp > GetMaxHp())
            {
                HP = GetMaxHp();
            }
            else
                HP = newHp;
        }

        public void RegenMana(int pTimesToRegen = 1)
        {
            var manaToRegen = PlayerClass.GetManaRegenRate() * pTimesToRegen;
            var newMana = Mana + manaToRegen;
            if (newMana > GetMaxMana())
            {
                Mana = GetMaxMana();
            }
            else
                Mana = newMana;
        }
    }
}
