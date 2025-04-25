using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Player : PersistableBase<Player>, ITarget
    {
        [JsonIgnore]
        public override string TableName { get; set; } = nameof(Player);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(PlayerId);

        // Persisted on Player table
        [PersistProperty(true)]
        public int? PlayerId { get; set; }

        public int Id {
            get
            {
                return PlayerId.Value;
            }
        }

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
        public string Name { get; set; }

        public List<MonsterType> MonsterTypeList { get; } = new List<MonsterType>{MonsterType.PLAYER};


        // From other tables
        public PlayerPlayerClass PlayerPlayerClass { get; set; }
        [JsonIgnore]
        public IEnumerable<PlayerPlayerClass> UnlockedPlayerPlayerClassList { get; set; }
        public List<int?> LootList { get; set; }  //TODO change to real loot list (combinded list -> see PlayerWeapon.cs)
        public Weapon Weapon { get; set; }
        public List<int?> AutoSellList { get; set; }
        public int Level { get => Stats.GetLevel(); }
        public int BonusMagicDmg { get; set; }
        public IEnumerable<PlayerWeapon> PlayerWeaponList { get; set; }
        public BodyType PrefferedBodyType { get; set; }
        public IEnumerable<Effect> ActiveEffects { get; set; }

        // Behaviors
        [JsonIgnore]
        private EffectManager _effectManager {get; set;}

        [JsonIgnore]
        public PlayerCombat Combat => new(this);

        [JsonIgnore]
        public PlayerStats Stats => new(this);


        public Player() { }

        #region PersistableBase implementation
        public override void BeforePersist()
        {
            base.BeforePersist();
            UpdateBossFightConnectionWithPlayer();
        }

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
                player.UnlockedPlayerPlayerClassList.ForEach(pc => pc.PlayerClass.RecalculateUnlockedAbilities(player.Level));
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
        #endregion PersistableBase implementation

        #region ITarget methods implementation
        public bool IsDead()
        {
            return Combat.IsDead();
        }

        public bool IsAlive()
        {
            return !IsDead();
        }

        public bool IsAtFullHealth()
        {
            return Combat.IsAtFullHealth();
        }

        public void SubtractHealth(int pDamage, ITarget pAttacker)
        {
            Combat.SubtractHealth(pDamage, pAttacker);
        }

        public int GetMaxHp()
        {
            return Stats.GetMaxHp();
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

        #endregion

        public void UpdateBossFightConnectionWithPlayer()
        {
            var bossFightConnection = WebSocketConnections.GetInstance().GetConnection(this);
            if (bossFightConnection != null)
            {
                bossFightConnection.Player = this;
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

        public void GainXp(int pGainedXp, int? pMonsterLevel = null)
        {
            pGainedXp = ExperienceCalculator.CalcXpPenalty(pGainedXp, Stats.GetLevel(), pMonsterLevel);
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

        public void AddLoot(ILootItem pLootToAdd)
        {
            var lootId = pLootToAdd.LootId;
            AddLoot(lootId);
        }

        public string SellLoot(ILootItem pLootToSell)
        {
            LootList.Remove(pLootToSell.LootId);
            var sellPrice = pLootToSell.GetSellPrice();
            return $"You sold '{ pLootToSell.LootName }' for { sellPrice } gold";
        }

        public bool LootIsInAutoSellList(int? pLootId)
        {
            return AutoSellList.Contains(pLootId);
        }

        public bool AddEffect(Effect pEffect, bool pReplaceEffect)
        {
            return _effectManager.AddEffect(pEffect, pReplaceEffect);
        }

        public void RemoveEffect(Effect pEffect)
        {
            _effectManager.RemoveEffect(pEffect);
        }

        public bool HasEffect(Effect pEffect, out Effect pFoundEffect)
        {
            return _effectManager.HasEffect(pEffect, out pFoundEffect);
        }

        public void RemoveExpiredEffects()
        {
           _effectManager.RemoveExpiredEffects();
        }
    }
}
