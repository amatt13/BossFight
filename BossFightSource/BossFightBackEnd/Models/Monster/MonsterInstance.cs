using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;
using BossFight.Extentions;
using MySqlConnector;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class MonsterInstance : PersistableBase<MonsterInstance>, ITarget
    {
        private static readonly Random _random = new Random();
        private const int TIER_DIVIDER = 5;
        private const float MONSTER_CRIT_MODIFIER = 1.5f;

        [JsonIgnore]
        public override string TableName { get; set; } = nameof(MonsterInstance);
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(MonsterInstanceId);

        // Persisted on MonsterActive table
        [PersistProperty(true)]
        public int? MonsterInstanceId { get; set;  }

        [PersistProperty]
        public int MonsterTemplateId { get; set; }

        [PersistProperty]
        public bool? Active { get; set; }

        [PersistProperty]
        public int Level { get; set; }

        [PersistProperty]
        public int BlindDuration { get; set; }

        [PersistProperty]
        public int StunDuration { get; set; }

        [PersistProperty]
        public int LowerAttackDuration { get; set; }

        [PersistProperty]
        public float LowerAttackPercentage { get; set; }

        [PersistProperty]
        public int EasierToCritDuration { get; set; }

        [PersistProperty]
        public int EasierToCritPercentage { get; set; }

        private int? _maxHp;
        [PersistProperty]
        public int MaxHp 
        { 
            get
            {
                _maxHp ??= CalcMaxHealth();
                    
                return _maxHp.Value;
            }
            set { _maxHp = value; }
        }

        [PersistProperty]
        public int Hp { get; set; }

        [PersistProperty]
        public int Mana { get; set; }
        public string Name 
        { 
            get { return MonsterTemplate.Name; }
            set { MonsterTemplate.Name = value; }
        }

        // From other tables
        [JsonIgnore]
        public Dictionary<int, DamageTrackerEntry> DamageOverTimeTracker { get; set; }  // key is player_id

        public IEnumerable<MonsterDamageTracker> MonsterDamageTrackerList { get; set; }
        
        [JsonIgnore]
        public Dictionary<int, int> FrenzyStackTracker { get; set; }  // key is player_id, value is frenzy stack lvl/size

        [JsonIgnore]
        public MonsterTemplate MonsterTemplate { get; set; }

        // Calculated (not persisted) fields/properties

        public bool IsBossMonster { get { return MonsterTemplate.BossMonster.GetValueOrDefault(false); } }
        
        public double AttackStrength { get => Level / 2; }

        public MonsterInstance () { }

        public MonsterInstance (MonsterTemplate pMonsterTemplate) 
        {
            MonsterTemplate = pMonsterTemplate;
            MonsterTemplateId = pMonsterTemplate.MonsterTemplateId.Value;
        }

        #region PersistableBase implementation

        public override void BeforePersist()
        {
            base.BeforePersist();
            if (_maxHp == null)
                MaxHp = CalcMaxHealth();
        }

        public override IEnumerable<MonsterInstance> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<MonsterInstance>();

            while (!reader.IsClosed && reader.Read())
            {   
                var monsterInstance = new MonsterInstance();
                monsterInstance.MonsterInstanceId = reader.GetInt(nameof(MonsterInstanceId));
                monsterInstance.MonsterTemplateId = reader.GetInt(nameof(MonsterTemplateId));
                monsterInstance.Hp = reader.GetInt(nameof(Hp));
                monsterInstance.Mana = reader.GetInt(nameof(Mana));
                monsterInstance.MaxHp = reader.GetInt(nameof(MaxHp));
                monsterInstance.Active = reader.GetBoolean(nameof(Active));
                monsterInstance.Level = reader.GetInt(nameof(Level));
                monsterInstance.BlindDuration = reader.GetInt(nameof(BlindDuration));
                monsterInstance.StunDuration = reader.GetInt(nameof(StunDuration));
                monsterInstance.LowerAttackDuration = reader.GetInt(nameof(LowerAttackDuration));
                monsterInstance.LowerAttackPercentage = reader.GetFloat(nameof(LowerAttackPercentage));
                monsterInstance.EasierToCritDuration = reader.GetInt(nameof(EasierToCritDuration));
                monsterInstance.EasierToCritPercentage = reader.GetInt(nameof(EasierToCritPercentage));
                result.Add(monsterInstance);               
            }
            reader.Close();

            foreach (var monsterInstance in result)
            {
                monsterInstance.MonsterTemplate = new MonsterTemplate().FindOneForParent(monsterInstance.MonsterTemplateId, pConnection);

                monsterInstance.MonsterDamageTrackerList = new MonsterDamageTracker{MonsterInstanceId = monsterInstance.MonsterInstanceId}.FindAllForParent(null, pConnection);
                monsterInstance.MonsterDamageTrackerList.ForEach(mdt => mdt.MonsterInstance = monsterInstance);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<MonsterInstance> pSearchObject, bool pStartWithAnd = true)
        {
            var mi = pSearchObject as MonsterInstance;
            var additionalSearchCriteriaText = String.Empty;
            if (mi.Active.HasValue)
                additionalSearchCriteriaText += $" AND Active = { (mi.Active.Value ? "TRUE" : "FALSE") }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation

        public override string ToString()
        {
            return this.MonsterTemplate.Name;
        }

        public bool HasMonsterType(List<MonsterType> pMonsterTypeList)
        {
            return pMonsterTypeList.Any(mt => HasMonsterType(mt));
        }

        public bool HasMonsterType(MonsterType monsterType)
        {
            return MonsterTemplate.MonsterTypeList.Contains(monsterType);
        }

        public void SetLevelAndHealth(int pLevel)
        {
            Level = pLevel;
            CalcHealth();
        }

        public int GetMaxHp()
        {
            return MaxHp;
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

        public void CalcHealth()
        {
            var hp = 10d;
            foreach (var i in Enumerable.Range(0, Level))
            {
                if (IsBossMonster)
                {
                    hp += hp * 0.44;
                }
                else
                    hp += hp * 0.30;
            }
            hp = Math.Floor(hp);
            if (!IsBossMonster)
            {
                var variance = (int) Math.Floor(hp / 100 * 7);
                hp += _random.Next(-variance, variance + 1);
            }
            _maxHp = (int)hp;
            Hp = (int)hp;
        }

        //TODO persist across restarts?
        public int CalcMaxHealth()
        {
            var maxHp = 10d;
            foreach (var i in Enumerable.Range(0, Level))
            {
                if (IsBossMonster)
                {
                    maxHp += maxHp * 0.44;
                }
                else
                    maxHp += maxHp * 0.30;
            }
            maxHp = Math.Floor(maxHp);
            if (!IsBossMonster)
            {
                var variance = (int) Math.Floor(maxHp / 100 * 7);
                maxHp += _random.Next(-variance, variance + 1);
            }
            return (int)maxHp;
        }

        public void DebuffBlind(int pDuration)
        {
            if (BlindDuration < pDuration)
                throw new MyException("Cant blind monster. A stronger blind is already in effect");
            BlindDuration = pDuration;
        }

        public void DebuffStun(int pDuration)
        {
            if (StunDuration < pDuration)
                throw new MyException("Cant stun monster. A stronger stun is already in effect");
            StunDuration = pDuration;
        }

        public void DebuffLowerDmg(int pDuration, float pLowerAttackPercentage)
        {
            if (LowerAttackDuration < pDuration || LowerAttackPercentage < pLowerAttackPercentage)
                throw new MyException("Cant lower attack of monster. A stronger weaken is already in effect");
            LowerAttackDuration = pDuration;
            LowerAttackPercentage = pLowerAttackPercentage;
        }

        public void DebuffDmgOverTime(int pDuration, int pDamage, Player pPlayer)
        {
            foreach (var trackerPlayerId in DamageOverTimeTracker.Keys)
            {
                if (trackerPlayerId != pPlayer.PlayerId)
                    throw new MyException("Each players is limited to a single damage over time effect");
            }
            //DamageOverTimeTracker[pPlayer.PlayerId] = new DamageTrackerEntry(pPlayer.PlayerId, pPlayer.Name, pDuration, pDamage);
        }

        public int DebuffApplyFrenzyStack(int pFrenzyStacksToApply, Player pPlayer)
        {
            // if (FrenzyStackTracker.Keys.Contains(pPlayer.PlayerId))
            // {
            //     FrenzyStackTracker[pPlayer.PlayerId] += pFrenzyStacksToApply;
            // }
            // else
            //     FrenzyStackTracker[pPlayer.PlayerId] = pFrenzyStacksToApply;
            
            // // return current stack size for player
            // return FrenzyStackTracker[pPlayer.PlayerId];
            return 0;
        }

        public void DebuffEasierToCrit(int pDuration, int pExtraCritChance)
        {
            if (EasierToCritDuration < pDuration || EasierToCritPercentage < pExtraCritChance)
                throw new MyException("Cant increase critical chance on monster. A stronger effect is already applied");
            EasierToCritDuration = pDuration;
            EasierToCritPercentage = pExtraCritChance;
        }

        public void SubtractHealth(int pDamage, Player pAttackingPlayer)
        {
            var monsterDamageTrackerItem = MonsterDamageTrackerList.FirstOrDefault(mdt => mdt.PlayerId == pAttackingPlayer.PlayerId);
            if (monsterDamageTrackerItem != null)
            {
                monsterDamageTrackerItem.DamageReceivedFromPlayer += pDamage;
                monsterDamageTrackerItem.Persist();
            }
            else
            {
                monsterDamageTrackerItem = new MonsterDamageTracker(pAttackingPlayer, this, pDamage);
                monsterDamageTrackerItem.Persist();
                MonsterDamageTrackerList.Append(monsterDamageTrackerItem);
            }
                
            Hp -= pDamage;
        }

        public bool MonsterAttackIsCrit()
        {
            var critChance = IsBossMonster ? 15 : 3;
            var roll = _random.Next(1, 101);
            return roll <= critChance;
        }

        public bool HasActiveBlindDebuff()
        {
            return BlindDuration > 0;
        }

        public bool IsBlinded()
        {
            bool affectedByBlind = false;            
            if (HasActiveBlindDebuff())
            {
                BlindDuration -= 1;
                affectedByBlind = _random.Next(1, 101) > 33;
            }
            return affectedByBlind;
        }

        public bool HasActiveStunDebuff()
        {
            return StunDuration > 0;
        }

        public bool IsStunned()
        {
            bool isStunned = false;
            if (HasActiveStunDebuff())
            {
                StunDuration -= 1;
                isStunned = true;
            }
            return isStunned;
        }

        public bool HasLowerAttackDefuff()
        {
            return LowerAttackDuration > 0;
        }

        public int LowerAttackBecauseOfLowerAttackDebuff(int monsterDamage)
        {
            var newDamage = monsterDamage;
            if (HasLowerAttackDefuff())
            {
                newDamage -= (int)(newDamage * LowerAttackPercentage);
                LowerAttackDuration -= 1;
            }
            return newDamage;
        }

        public int CalculateMonsterDamage(out bool pIsCrit)
        {
            double damageDealt = AttackStrength;

            // variance
            double variance = (Level / 5) - 1;
            if (variance > 0.0)
                damageDealt += _random.Next((int)Math.Floor(-variance), (int)Math.Ceiling(variance) + 1);
                
            // round up to 1
            if (damageDealt <= 0.0)
                damageDealt = 1;

            // bonus boss damage
            if (IsBossMonster)
                damageDealt *= 1.25;
                
            // crit bonus
            pIsCrit = MonsterAttackIsCrit();
            if (pIsCrit)
                damageDealt *= MONSTER_CRIT_MODIFIER;
                

            return (int)Math.Ceiling(damageDealt);
        }

        public bool AttackOnMonsterIsCrit(int pCritChance)
        {
            if (EasierToCritDuration > 0)
            {
                pCritChance += EasierToCritPercentage;
                EasierToCritDuration -= 1;
            }

            var roll = _random.Next(0, 101);
            return roll <= pCritChance;
        }
    }
}
