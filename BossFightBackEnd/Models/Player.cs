using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.CustemExceptions;
using BossFight.Models.Loot;
using Newtonsoft.Json;

namespace BossFight.Models
{
    public class Player : Target, IPersist<Player>
    {
        [JsonIgnore]
        private static Random _random = new Random();
        
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(Player);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerId);

        // Persisted on Player table
        [PersistPropertyAttribute]
        public int PlayerId { get; set; }

        [PersistPropertyAttribute]
        public int Gold { get; set; }

        [PersistPropertyAttribute]
        public int WeaponId { get; set; }

        [PersistPropertyAttribute]
        public int Mana { get; set; }

        [PersistPropertyAttribute]
        public int CurentPlayerClassId { get; set; }
        
        // From other tables
        public PlayerPlayerClass PlayerPlayerClass { get; set; }
        public IEnumerable<PlayerPlayerClass> UnlockedPlayerPlayerClassList { get; set; }
        public List<int> LootList { get; set; }  //TODO change to real loot list (combinded list -> see PlayerWeapon.cs)
        public Weapon Weapon { get; set; }
        public List<int> AutoSellList { get; set; }
        public int Level { get => PlayerPlayerClass.Level; }
        public int BonusMagicDmg { get; set; }
        public int BonusMagicDmgDuration { get; set; }
        public IEnumerable<PlayerWeapon> PlayerWeaponList { get; set; }

        public Player() { }

        public Player FindOne(int id)
        {
            return (Player)_findOne(id);
        }

        public IEnumerable<Player> FindAll(int? id = null)
        {
            return _findAll(id).Cast<Player>();
        }

        public override IEnumerable<PersistableBase> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader)
        {
            var result = new List<PersistableBase>();

            while (reader.Read())
            {
                var player = new Player();
                player.PlayerId = reader.GetInt(nameof(PlayerId));
                player.Gold = reader.GetInt(nameof(Gold));
                player.Name = reader.GetString(nameof(Name));
                player.Hp = reader.GetInt(nameof(Hp));
                player.Mana = reader.GetInt(nameof(Mana));
                player.CurentPlayerClassId = reader.GetInt(nameof(CurentPlayerClassId));
                player.WeaponId = reader.GetInt(nameof(WeaponId));
                
                player.PlayerPlayerClass = new PlayerPlayerClass{Active = true}.FindOne(player.PlayerId);
                player.PlayerPlayerClass.Player = player;
                player.UnlockedPlayerPlayerClassList = new PlayerPlayerClass{}.FindAll(player.PlayerId);
                player.Weapon = new Weapon().FindOne(player.WeaponId);

                player.PlayerWeaponList = new PlayerWeapon{ PlayerId =  player.PlayerId}.FindAll();
                foreach(var x in player.PlayerWeaponList) { x.Player = player; }

                result.Add(player);
            }

            return result;
        }

        public void SubtractHealth(int pDamageToReceive)
        {
            Hp -= pDamageToReceive;

            if (Hp < -3)
                Hp = -3;
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

        public void GainXp(int pGainedXp, int? pMonsterLevel = null)
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
        }

        public void EquipWeapon(int pWeaponId)
        {
            WeaponId = pWeaponId;
            Persist(this.PlayerId);
        }

        public bool HasEnoughManaForAbility(Ability pAbility)
        {
            return Mana >= pAbility.ManaCost;
        }

        public int GetAttackBonus()
        {
            return (int)Math.Floor((double)Level / 2) + BonusMagicDmg + PlayerPlayerClass.AttackPowerBonus;
        }

        public int GetSpellBonus()
        {
            return (int)Math.Floor((double)Level / 2) + PlayerPlayerClass.SpellPowerBonus;
        }

        public int GetAttackCritChance()
        {
            var critChance = Weapon.AttackCritChance;
            critChance += PlayerPlayerClass.CritChance;
            return critChance;
        }

        public int GetSpellCritChance()
        {
            var critChance = Weapon.SpellCritChance;
            critChance += PlayerPlayerClass.CritChance;
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

        public AttackMessage AttackMonsterWithWeapon(MonsterTemplate pTargetMonster, Weapon pPlayerWeapon, bool pRetaliate = true)
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
            return $"You sold '{ pLootToSell.LootName }' for { sellPrice } gold";
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

        // public void RegenHealth(int PTimesToRegen = 1)
        // {
        //     int hpToRegen;
        //     if (IsKnockedOut())
        //     {
        //         hpToRegen = 1 * PTimesToRegen;
        //     }
        //     else
        //         hpToRegen = PlayerPlayerClass.HpRegenRate * PTimesToRegen;

        //     var newHp = Hp + hpToRegen;
        //     if (newHp > GetMaxHp())
        //     {
        //         Hp = GetMaxHp();
        //     }
        //     else
        //         Hp = newHp;
        // }

        // public void RegenMana(int pTimesToRegen = 1)
        // {
        //     var manaToRegen = PlayerPlayerClass.ManaRegenRate * pTimesToRegen;
        //     var newMana = Mana + manaToRegen;
        //     if (newMana > GetMaxMana())
        //     {
        //         Mana = GetMaxMana();
        //     }
        //     else
        //         Mana = newMana;
        // }
    }
}
