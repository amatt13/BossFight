using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using BossFight.Models.Loot;
using MySqlConnector;
using System.Text.Json.Serialization;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Player : PersistableBase<Player>, ITarget
    {
        [JsonIgnore]
        private static readonly Random _random = new();

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
        public string UserName { get; set; }

        [PersistProperty]
        [JsonIgnore]  // please dont send me to the front end
        public string Password { get; set; }

        [PersistProperty]
        [JsonIgnore]
        public int PreferredBodyTypeId { get; set; }

        [PersistProperty]
        public int Hp { get; set; }

        [PersistProperty]
        public int Mana { get; set; }

        [PersistProperty]
        public string Name { get; set; } = "DEFAULT EFFECT";

        public List<MonsterType> MonsterTypeList { get; } = new List<MonsterType>{MonsterType.PLAYER};


        // From other tables
        public PlayerPlayerClass PlayerPlayerClass { get; set; }
        [JsonIgnore]
        public IEnumerable<PlayerPlayerClass> UnlockedPlayerPlayerClassList { get; set; }
        public List<int?> LootList { get; set; }  //TODO change to real loot list (combinded list -> see PlayerWeapon.cs)
        public Weapon Weapon { get; set; }
        public List<int?> AutoSellList { get; set; }
        public int Level { get => PlayerPlayerClass.Level; }
        public int BonusMagicDmg { get; set; }
        public IEnumerable<PlayerWeapon> PlayerWeaponList { get; set; }
        public BodyType PrefferedBodyType { get; set; }
        public IEnumerable<Effect> ActiveEffects { get; set; }

        [JsonIgnore]
        private EffectManager _effectManager {get; set;}

        public Player() { }

        public override IEnumerable<Player> BuildObjectFromReader(MySqlConnector.MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<Player>();

            while (!reader.IsClosed && reader.Read())
            {
                var player = new Player
                {
                    PlayerId = reader.GetInt(nameof(PlayerId)),
                    Gold = reader.GetInt(nameof(Gold)),
                    Name = reader.GetString(nameof(Name)),
                    Hp = reader.GetInt(nameof(Hp)),
                    Mana = reader.GetInt(nameof(Mana)),
                    WeaponId = reader.GetInt(nameof(WeaponId)),
                    UserName = reader.GetString(nameof(UserName)),
                    Password = reader.GetString(nameof(Password)),
                    PreferredBodyTypeId = reader.GetInt(nameof(PreferredBodyTypeId))
                };

                result.Add(player);
            }
            reader.Close();

            foreach (var player in result)
            {
                player.PlayerPlayerClass = new PlayerPlayerClass{Active = true, PlayerId = player.PlayerId}.FindAllForParent(null, pConnection).First();
                player.PlayerPlayerClass.Player = player;
                player.PlayerPlayerClass.PlayerClass.RecalculateUnlockedAbilities(player.Level);
                player.UnlockedPlayerPlayerClassList = new PlayerPlayerClass{PlayerId = player.PlayerId}.FindAllForParent(null, pConnection);
                player.Weapon = (Weapon)new Weapon().FindOneForParent(player.WeaponId, pConnection);
                player.PrefferedBodyType = new BodyType{}.FindOneForParent(player.PreferredBodyTypeId, pConnection);


                var effects = new Effect{EffectHolderPlayerId = player.PlayerId}.FindAllForParent(null, pConnection);
                var builtEffects = new List<Effect>();
                foreach (var effect in effects)
                {
                    Effect newEffect = effect.BuildEffect();
                    builtEffects.Add(newEffect);
                }
                player.ActiveEffects = builtEffects;
                player._effectManager = new EffectManager(player.ActiveEffects);

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
            if (pDamageToReceive > 0)
            {
                Hp -= pDamageToReceive;

                if (Hp < -3)
                    Hp = -3;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual object ShopStr(int pLengthOfLongestPlayerName, int PengthOfLongestPlayerTotalGold)
        {
            var goldStr = String.Format("{0:n0}", Gold);
            goldStr = $"{ goldStr.Replace(',', '.') }".PadLeft(PengthOfLongestPlayerTotalGold);
            return $"{ Name.PadLeft(pLengthOfLongestPlayerName, '.') } { goldStr } gold";
        }

        public int GetMaxHp()
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

        public bool IsDead()
        {
            return Hp <= 0;
        }

        public bool IsAlive()
        {
            return !IsDead();
        }

        public bool IsAtFullHealth()
        {
            return Hp >= GetMaxHp();
        }

        public string PossessiveName()
        {
            if (Name.Last() == 's')
            {
                return Name + "'";
            }
            else
            {
                return Name + "'s";
            }
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

        public string SellLoot(iLootItem pLootToSell)
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

        public bool AddEffect(Effect pEffect)
        {
            return _effectManager.AddEffect(pEffect);
        }

        public void RemoveEffect(EffectType pEffectType)
        {
            _effectManager.RemoveEffect(pEffectType);
        }

        public bool HasEffect(EffectType pEffectType)
        {
            return _effectManager.HasEffect(pEffectType);
        }

        public void RemoveExpiredEffects()
        {
           _effectManager.RemoveExpiredEffects();
        }
    }
}
