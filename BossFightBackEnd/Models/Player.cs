using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.CustemExceptions;
using BossFight.Models.Loot;

namespace BossFight.Models
{
    public class Player : Target, IPersist<Player>
    {
        private static Random _random = new Random();

        public override string TableName { get; set; } = "Player";
        public override string IdColumn { get; set; } = nameof(PlayerId);

        // Persisted on Player table
        public int PlayerId { get; set; }
        public int Gold { get; set; }
        public int WeaponId { get; set; }
        public int Mana { get; set; }
        public int CurentPlayerClassId { get; set; }
        
        // From other tables

        public PlayerClass PlayerClass { get; set; }
        public PlayerPlayerClass PlayerPlayerClass { get; set; }
        public List<PlayerClass> PlayerClassList { get; set; }
        public List<PlayerPlayerClass> UnlockedPlayerPlayerClassList { get; set; }
        public List<int> LootList { get; set; }
        public Weapon Weapon { get; set; }
        public List<int> AutoSellList { get; set; }

        public int BonusMagicDmg { get; set; }
        public int BonusMagicDmgDuration { get; set; }

        public Player() { }

        public Player FindOne(int id)
        {
            return (Player)_findOne(id);
        }

        public override PersistableBase BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            var player = new Player();

            while (reader.Read())
            {
                player.PlayerId = reader.GetInt32(nameof(PlayerId));
                player.Gold = reader.GetInt32(nameof(Gold));
                player.Name = reader.GetString(nameof(Name));
                player.Hp = reader.GetInt32(nameof(Hp));
                player.Mana = reader.GetInt32(nameof(Mana));
                player.CurentPlayerClassId = reader.GetInt(nameof(CurentPlayerClassId));
                
                player.PlayerClass = new PlayerClass().FindOne(player.CurentPlayerClassId);
                player.PlayerPlayerClass = new PlayerPlayerClass().FindOne(player.CurentPlayerClassId);
                var weaponId = reader.GetIntNullable("WeaponId");
                if (weaponId.HasValue)
                    player.Weapon = new Weapon().FindOne(weaponId.Value);
            }

            return player;
        }

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
            return PlayerPlayerClass.MaxHp;
        }

        public int GetMaxMana()
        {
            return PlayerPlayerClass.MaxMana;
        }

        public int GetLevel()
        {
            return PlayerPlayerClass.Level;
        }

        public bool IsKnockedOut()
        {
            return IsDead();
        }

        public PlayerClass GainXp(int pGainedXp, int? pMonsterLevel = null)
        {
            pGainedXp = GenralHelperFunctions.CalcXpPenalty(pGainedXp, GetLevel(), pMonsterLevel);
            PlayerPlayerClass.XP += pGainedXp;
            var xpNeededToNextLevel = GenralHelperFunctions.XpNeededToNextLevel(PlayerPlayerClass);
            if (xpNeededToNextLevel <= 0)
            {
                PlayerPlayerClass.LevelUp();
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
            return (int)Math.Floor((double)Level / 2) + BonusMagicDmg + PlayerClass.AttackPowerBonus;
        }

        public int GetSpellBonus()
        {
            return (int)Math.Floor((double)Level / 2) + PlayerClass.SpellPowerBonus;
        }

        public int GetAttackCritChance()
        {
            var critChance = Weapon.AttackCritChance;
            critChance += PlayerClass.CritChance;
            return critChance;
        }

        public int GetSpellCritChance()
        {
            var critChance = Weapon.SpellCritChance;
            critChance += PlayerClass.CritChance;
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

        // Throws an exception if the weapon is not found
        // public void ChangeWeapon(int pNewWeaponId)
        // {
        //     var itemToEquip = LootList.First(i => i == Convert.ToInt32(pNewWeaponId));
        //     LootList.Remove(itemToEquip);
        //     AddLoot(WeaponId);
        //     WeaponId = itemToEquip;
        //     //Weapon = GenralHelperFunctions.findWeaponByWeaponId(pNewWeaponId);
        // }

        // public void ChangeClass(string pNewPlayerClassName)
        // {
        //     var newPlayerClassName = pNewPlayerClassName.ToLower();
        //     PlayerClass newClass;
        //     if (newPlayerClassName != PlayerClass.Name.ToLower())
        //     {
        //         try
        //         {
        //             newClass = PlayerClassList.First(pc => pc.Name.ToLower() == newPlayerClassName);
        //             if (newClass != null)
        //             {
        //                 PlayerClass = newClass;
        //                 double currentHealthPercentage = Math.Floor((double)Hp / GetMaxHp());
        //                 if (currentHealthPercentage > 1.0d)
        //                     currentHealthPercentage = 1.0d;
        //                 //CurrentPlayerClassId = newClass.classId;
        //                 Hp = (int)Math.Floor(GetMaxHp() * currentHealthPercentage);
        //                 if (Hp < -3)
        //                     Hp = -3;
        //                 Mana = 0;
        //             }
        //         }
        //         catch (InvalidOperationException)
        //         {
        //             PlayerClassList.OrderBy(x => x.Name);
        //             var unlockedClasses = String.Join(", ", from pc in PlayerClassList select $"{ pc.Name }");
        //             throw new MyException($"Class with name '{ pNewPlayerClassName }' is not unlocked. Your unlocked classes is: { unlockedClasses }");
        //         }
        //     }
        // }

        public string UnlockedClassesInfo()
        {
            var info = "Unlocked classes\n";
            info += String.Join("\n", from pc in UnlockedPlayerPlayerClassList select $"{ pc.PlayerClassName } level { pc.Level }\nHp { pc.MaxHp } Mana { pc.MaxMana }");
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
            Hp -= pDamage;
            var damageText = $"You received { pDamage } damage from { pMonsterName }";
            if (IsKnockedOut())
            {
                if (Hp < -3)
                    Hp = -3;
                damageText += ", and is knocked out";
            }
            if (pNewLine)
                damageText += "\n";

            return damageText;
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
            Hp = GetMaxHp();
            Mana = GetMaxMana();
        }

        public void RegenHealth(int PTimesToRegen = 1)
        {
            int hpToRegen;
            if (IsKnockedOut())
            {
                hpToRegen = 1 * PTimesToRegen;
            }
            else
                hpToRegen = PlayerClass.HpRegenRate * PTimesToRegen;

            var newHp = Hp + hpToRegen;
            if (newHp > GetMaxHp())
            {
                Hp = GetMaxHp();
            }
            else
                Hp = newHp;
        }

        public void RegenMana(int pTimesToRegen = 1)
        {
            var manaToRegen = PlayerClass.ManaRegenRate * pTimesToRegen;
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
