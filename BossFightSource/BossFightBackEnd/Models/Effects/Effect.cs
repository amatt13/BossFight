using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using MySqlConnector;
using BossFight.Extentions;
using Microsoft.Extensions.Logging;
using BossFight.BossFightBackEnd.BossFightLogger;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public class Effect: PersistableBase<Effect>
    {
        protected readonly ILogger<Effect> _logger;

        [JsonIgnore]
        public override string TableName
        {
            get
            {
                return EffectBelongsToPlayer()
                ? "PlayerEffect"
                : EffectBelongsToMonster()
                    ? "MonsterEffect"
                    : throw new Exception("Invalid effect. Does not belong to anyone!");
            }
            set => throw new Exception("What are we trying to do here?");
        }
        [JsonIgnore]
        public override string IdColumn { get; set; } = nameof(EffectId);

        // Persisted on Player table
        [PersistProperty(true)]
        public int? EffectId { get; set; }

        [PersistProperty]
        public EffectType EffectType { get; set; }

        // Duration: for how many turns the effect will be actived before it gets deleted
        [PersistProperty]
        public int? Duration { get; set; }

        // Charges: how many times the effect will be actived before it gets deleted
        [PersistProperty]
        public int? Charges {get; set;}

        [PersistProperty]
        public int? EffectHolderPlayerId {get; set;}

        [PersistProperty]
        public int? EffectHolderMonsterId {get; set;}

        [PersistProperty]
        public int? EffectCasterPlayerId {get; set;}

        [PersistProperty]
        public int? EffectCasterMonsterId {get; set;}

        [PersistProperty]
        public Dictionary<string, object> Fields {get; set;} = new Dictionary<string, object>();

        public virtual string Name {get; set;}

        // From other tables
        // The effect can be applied to players or monsters.
        [JsonIgnore]
        private Player _effectHolderPlayer;

        [JsonIgnore]
        public Player EffectHolderPlayer
        {
            get
            {
                _effectHolderPlayer ??= new Player().FindOne(EffectHolderPlayerId);
                return _effectHolderPlayer;
            }
            protected set
            {
                _effectHolderPlayer = value;
            }
        }

        [JsonIgnore]
        private MonsterInstance _effectHolderMonster;

        [JsonIgnore]
        public MonsterInstance EffectHolderMonster
        {
            get
            {
                _effectHolderMonster ??= new MonsterInstance().FindOne(EffectHolderMonsterId);
                return _effectHolderMonster;
            }
            protected set
            {
                _effectHolderMonster = value;
            }
        }

        // The effect can be applied by a player or monster.
        [JsonIgnore]
        private Player _effectCasterPlayer;

        [JsonIgnore]
        public Player EffectCasterPlayer
        {
            get
            {
                _effectCasterPlayer ??= new Player().FindOne(EffectCasterPlayerId);
                return _effectCasterPlayer;
            }
            protected set
            {
                _effectCasterPlayer = value;
            }
        }

        [JsonIgnore]
        private MonsterInstance _effectCasterMonster;

        [JsonIgnore]
        public MonsterInstance EffectCasterMonster
        {
            get
            {
                _effectCasterMonster ??= new MonsterInstance().FindOne(EffectCasterMonsterId);
                return _effectCasterMonster;
            }
            protected set
            {
                _effectCasterMonster = value;
            }
        }

        // Calculated fields/properties
        [JsonIgnore]
        public Dictionary<string, string> SearchFields { get; set; }

        [JsonIgnore]
        public ITarget EffectHolder
        {
            get
            {
                if (EffectBelongsToPlayer())
                {
                    return EffectHolderPlayer;
                }
                else if (EffectBelongsToMonster())
                {
                    return EffectHolderMonster;
                }
                else
                {
                    return null;
                }
            }
        }

        public Effect()
        {
            ILoggerProvider fileLoggerProvider = new BossFightLoggerProvider("logs/Effect.txt");
            ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddProvider(fileLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            _logger = _loggerFactory.CreateLogger<Effect>();
        }

        protected Effect(Effect pBaseEffect)
        {
            if (pBaseEffect != null)
            {
                EffectId = pBaseEffect.EffectId;
                EffectType = pBaseEffect.EffectType;
                Duration = pBaseEffect.Duration;
                Charges = pBaseEffect.Charges;
                EffectHolderPlayerId = pBaseEffect.EffectHolderPlayerId;
                EffectHolderMonsterId = pBaseEffect.EffectHolderMonsterId;
                EffectCasterPlayerId = pBaseEffect.EffectCasterPlayerId;
                EffectCasterMonsterId = pBaseEffect.EffectCasterMonsterId;
                //Name = pBaseEffect.Name;  // Should be set by the subclass
                EffectHolderPlayer = pBaseEffect.EffectHolderPlayer;
                EffectHolderMonster = pBaseEffect.EffectHolderMonster;
                EffectCasterPlayer = pBaseEffect.EffectCasterPlayer;
                EffectCasterMonster = pBaseEffect.EffectCasterMonster;
                Fields = pBaseEffect.Fields;
            }
        }

        private static class EffectFactory<T>
        where T: Effect, new()
        {

            /// <summary>
            /// Use this function when recreating a effect from the DB
            /// </summary>
            public static T CreateNewEffectAndCopyProperties(Effect pBaseEffect)
            {
                var newInstance = new T
                {
                    EffectId = pBaseEffect.EffectId,
                    EffectType = pBaseEffect.EffectType,
                    Duration = pBaseEffect.Duration,
                    Charges = pBaseEffect.Charges,
                    EffectHolderPlayerId = pBaseEffect.EffectHolderPlayerId,
                    EffectHolderMonsterId = pBaseEffect.EffectHolderMonsterId,
                    EffectCasterPlayerId = pBaseEffect.EffectCasterPlayerId,
                    EffectCasterMonsterId = pBaseEffect.EffectCasterMonsterId,
                    //Name = pBaseEffect.Name;  // Should be set by the subclass
                    EffectHolderPlayer = pBaseEffect._effectHolderPlayer,
                    EffectHolderMonster = pBaseEffect._effectHolderMonster,
                    EffectCasterPlayer = pBaseEffect._effectCasterPlayer,
                    EffectCasterMonster = pBaseEffect._effectCasterMonster,
                    Fields = pBaseEffect.Fields
                };
                foreach(var key in newInstance.Fields.Keys)
                {
                    PropertyInfo propertyInfo = newInstance.GetType().GetProperty(key);
                    var value = newInstance.Fields[key];
                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(newInstance, int.Parse(value.ToString()));
                    }
                    else
                    {
                        throw new Exception($"Unsuported Fields type! Fields = {newInstance.Fields}, Target type {propertyInfo.PropertyType}, value {value}");
                    }
                }
                return newInstance;
            }
        }

        public Effect BuildEffect()
        {
            return EffectType switch
            {
                EffectType.DIVINE_SHIELD => EffectFactory<DivineShieldEffect>.CreateNewEffectAndCopyProperties(this),
                EffectType.INTIMIDATE => EffectFactory<IntimidateEffect>.CreateNewEffectAndCopyProperties(this),
                EffectType.DAMAGE_OVER_TIME => EffectFactory<DamageOverTimeEffect>.CreateNewEffectAndCopyProperties(this),
                _ => throw new Exception($"Invalid EffectType for Effect building. EffectType : '{EffectType}' on {TableName}"),
            };
        }

        protected virtual bool Apply(ITarget pTarget, ITarget pCaster, bool pReplaceEffect = true)
        {
            _logger.LogTrace("Applying effect to {pTarget}", pTarget);
            SetEffectHolderITarget(pTarget);
            SetEffectCasterITarget(pCaster);
            return pTarget.AddEffect(this, pReplaceEffect);
        }

        public virtual void Remove(ITarget pTarget)
        {
            _logger.LogTrace("Removing effect from {pTarget}", pTarget);
            pTarget.RemoveEffect(this);
        }

        public bool EffectBelongsToPlayer()
        {
            return EffectHolderPlayerId.HasValue;
        }

        public bool EffectBelongsToMonster()
        {
            return EffectHolderMonsterId.HasValue;
        }

        protected void SetEffectHolderPlayer(Player pPlayer)
        {
            EffectHolderPlayer = pPlayer;
            EffectHolderPlayerId = pPlayer.PlayerId;
        }

        protected void SetEffectHolderMonster(MonsterInstance pMonster)
        {
            EffectHolderMonster = pMonster;
            EffectHolderMonsterId = pMonster.MonsterInstanceId;
        }

        protected void SetEffectHolderITarget(ITarget pTarget)
        {
            if (pTarget is Player player)
            {
                SetEffectHolderPlayer(player);
            }
            else if (pTarget is MonsterInstance monster)
            {
                SetEffectHolderMonster(monster);
            }
            else
                _logger.LogError("SetEffectHolderITarget could not find matching function for {pTarget}", pTarget);
        }

        protected void SetEffectCasterPlayer(Player pPlayer)
        {
            EffectCasterPlayer = pPlayer;
            EffectCasterPlayerId = pPlayer.PlayerId;
        }

        protected void SetEffectCasterMonster(MonsterInstance pMonster)
        {
            EffectCasterMonster = pMonster;
            EffectCasterMonsterId = pMonster.MonsterInstanceId;
        }

        protected void SetEffectCasterITarget(ITarget pTarget)
        {
            if (pTarget is Player player)
            {
                SetEffectCasterPlayer(player);
            }
            else if (pTarget is MonsterInstance monster)
            {
                SetEffectCasterMonster(monster);
            }
            else
                _logger.LogError("SetEffectCasterITarget could not find matching function for {pTarget}", pTarget);
        }

        public ITarget GetCaster()
        {
            return EffectCasterPlayerId != null
            ? EffectCasterPlayer
            : EffectCasterMonster;
        }

        public virtual void OnDamageReceived(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeReceived) { }
        public virtual void OnDamageDealt(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeDealt) { }
        public virtual void OnBeforeAttack(ITarget pEffectHolder, ITarget pEffectCaster, PlayerAttackSummary pPlayerAttackSummary) { }

        public override string ToString()
        {
            return Name;
        }

        #region PersistableBase implementation

        public override IEnumerable<Effect> BuildObjectFromReader(MySqlDataReader reader, MySqlConnection pConnection)
        {
            var result = new List<Effect>();

            while (reader.Read())
            {
                var effect = new Effect()
                {
                    EffectType = (EffectType)reader.GetInt(nameof(EffectType)),
                    EffectId = reader.GetInt(nameof(EffectId)),
                    Duration = reader.GetIntNullable(nameof(Duration)),
                    Charges = reader.GetIntNullable(nameof(Charges)),
                    EffectHolderPlayerId = reader.GetIntNullable(nameof(EffectHolderPlayerId)),
                    EffectHolderMonsterId = reader.GetIntNullable(nameof(EffectHolderMonsterId)),
                    EffectCasterPlayerId = reader.GetIntNullable(nameof(EffectCasterPlayerId)),
                    EffectCasterMonsterId = reader.GetIntNullable(nameof(EffectCasterMonsterId)),
                    Fields = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(nameof(Fields)))
                };
                result.Add(effect);
            }
            reader.Close();

            foreach (var effect in result)
            {
                if (effect.EffectCasterMonsterId.HasValue)
                    effect.EffectCasterMonster = new MonsterInstance().FindOneForParent(effect.EffectCasterMonsterId, pConnection);
            }

            return result;
        }

        public override string AdditionalSearchCriteria(PersistableBase<Effect> pSearchObject, bool pStartWithAnd = true)
        {
            var e = pSearchObject as Effect;
            var additionalSearchCriteriaText = String.Empty;
            if (e.EffectHolderPlayerId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(EffectHolderPlayerId) } = { e.EffectHolderPlayerId }\n";

            if (e.EffectHolderMonsterId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(EffectHolderMonsterId) } = { e.EffectHolderMonsterId }\n";

            if (e.EffectCasterPlayerId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(EffectCasterPlayerId) } = { e.EffectCasterPlayerId }\n";

            if (e.EffectCasterMonsterId.HasValue)
                additionalSearchCriteriaText += $" AND { nameof(EffectCasterMonsterId) } = { e.EffectCasterMonsterId }\n";

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation
    }
}
