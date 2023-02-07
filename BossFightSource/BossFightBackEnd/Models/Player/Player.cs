using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.CustemExceptions;
using BossFight.Extentions;
using BossFight.Models.Loot;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class Player : Target<Player>
    {
        [JsonIgnore]
        private static Random _random = new Random();
        
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(Player);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerId);

        // Persisted on Player table
        [PersistProperty(true)]
        public int? PlayerId { get; set; }

        [PersistProperty]
        public int Gold { get; set; }

        [PersistProperty]
        public int WeaponId { get; set; }

        [PersistProperty]
        public int Mana { get; set; }

        [PersistProperty]
        public int CurentPlayerClassId { get; set; }

        [PersistProperty]
        public string UserName { get; set; }

        [PersistProperty]
        [JsonIgnore]
        public string Password { get; set; }
        
        // From other tables
        public PlayerPlayerClass PlayerPlayerClass { get; set; }
        public IEnumerable<PlayerPlayerClass> UnlockedPlayerPlayerClassList { get; set; }
        public List<int?> LootList { get; set; }  //TODO change to real loot list (combinded list -> see PlayerWeapon.cs)
        public Weapon Weapon { get; set; }
        public List<int?> AutoSellList { get; set; }
        public int Level { get => PlayerPlayerClass.Level; }
        public int BonusMagicDmg { get; set; }
        public int BonusMagicDmgDuration { get; set; }
        public IEnumerable<PlayerWeapon> PlayerWeaponList { get; set; }

        public Player() { }

        public override IEnumerable<Player> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<Player>();

            while (!reader.IsClosed && reader.Read())
            {
                var player = new Player();
                player.PlayerId = reader.GetInt(nameof(PlayerId));
                player.Gold = reader.GetInt(nameof(Gold));
                player.Name = reader.GetString(nameof(Name));
                player.Hp = reader.GetInt(nameof(Hp));
                player.Mana = reader.GetInt(nameof(Mana));
                player.CurentPlayerClassId = reader.GetInt(nameof(CurentPlayerClassId));
                player.WeaponId = reader.GetInt(nameof(WeaponId));
                player.UserName = reader.GetString(nameof(UserName));
                player.Password = reader.GetString(nameof(Password));

                result.Add(player);
            }
            reader.Close();

            foreach (var player in result)
            {
                player.PlayerPlayerClass = new PlayerPlayerClass{ Active = true, PlayerId = player.PlayerId }.FindAllForParent(null, pConnection).First();
                player.PlayerPlayerClass.Player = player;
                player.UnlockedPlayerPlayerClassList = new PlayerPlayerClass{ PlayerId = player.PlayerId }.FindAllForParent(null, pConnection);
                player.Weapon = new Weapon().FindOneForParent(player.WeaponId, pConnection);

                player.PlayerWeaponList = new PlayerWeapon{ PlayerId =  player.PlayerId}.FindAllForParent(null, pConnection);
                foreach(var x in player.PlayerWeaponList) { x.Player = player; }
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<Player> pSearchObject, bool pStartWithAnd = true)
        {
            var p = pSearchObject as Player;
            var additionalSearchCriteriaText = String.Empty;
            if (p.UserName.HasText())
                additionalSearchCriteriaText += $" AND { nameof(UserName) } = { p.UserName.ToDbString() }\n";

            if (p.Password.HasText())
                additionalSearchCriteriaText += $" AND { nameof(Password) } = { p.Password.ToDbString() }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        public int CalckWeaponAttackDamage(MonsterInstance pTargetMonster, PlayerAttackSummary pPlayerAttackSummary)
        {
            var isCrit = pTargetMonster.AttackOnMonsterIsCrit(GetAttackCritChance());
            var dmg = Weapon.AttackPower + GetAttackBonus();
            if (BonusMagicDmgDuration > 0) 
                BonusMagicDmgDuration -= 1;

            if (isCrit)
            {
                dmg = (int)Math.Ceiling(1.25 * dmg);
                pPlayerAttackSummary.PlayerCrit = true;
            }

            pPlayerAttackSummary.PlayerTotalDamage = dmg;
            return dmg;
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
            pGainedXp = ExperienceCalculator.CalcXpPenalty(pGainedXp, GetLevel(), pMonsterLevel);
            PlayerPlayerClass.XP += pGainedXp;
            var xpNeededToNextLevel = ExperienceCalculator.XpNeededToNextLevel(PlayerPlayerClass);
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
            Persist();
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

        public bool PlayerSpellIsCrit(int pBonusCritChance = 0)
        {
            var critChance = GetSpellCritChance();
            critChance += pBonusCritChance;
            var roll = _random.Next(0, 101);
            return roll <= critChance;
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

        public void AddLoot(int? pLootToAdd)
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

        public void AddLoot(iLootItem pLootToAdd)
        {
            var lootId = pLootToAdd.LootId;
            AddLoot(lootId);
        }

        public string sellLoot(iLootItem pLootToSell)
        {
            LootList.Remove(pLootToSell.LootId);
            var sellPrice = pLootToSell.GetSellPrice();
            return $"You sold '{ pLootToSell.LootName }' for { sellPrice } gold";
        }

        public bool LootIsInAutoSellList(int? pLootId)
        {
            return AutoSellList.Contains(pLootId);
        }

        public void RestoreAllHealthAndMana()
        {
            Hp = GetMaxHp();
            Mana = GetMaxMana();
        }
    }
}
