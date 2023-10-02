using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MySqlConnector;
using BossFight.Extentions;
using System.Linq;
using Microsoft.Extensions.Logging;
using BossFight.BossFightBackEnd.BossFightLogger;

namespace BossFight.Models
{
    public enum EffectType
    {
        DivineShield = 1,
        Intimidate
    }

    public interface IProcsOnDamageTaken
    {
        void OnDamageReceived(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeReceived);
    }

    public interface IProcsOnDamageDealt
    {
        void OnDamageDealt(ITarget pBuffHolder, ITarget pDamageDealer, ref int pDamageToBeDealt);
    }

    public class EffectManager: IEffectHolder
    {
        private IEnumerable<Effect> _effectList;

        public EffectManager(IEnumerable<Effect> pEffectList)
        {
            _effectList = pEffectList;
        }

        public bool AddEffect(Effect pEffect)
        {
            var effectAdded = false;
            if (!HasEffect(pEffect.EffectType))
            {
                _effectList = _effectList.Append(pEffect);
                effectAdded = true;
            }

            return effectAdded;
        }

        public void RemoveEffect(EffectType pEffectType)
        {
            _effectList = _effectList.Where(effect => effect.EffectType != pEffectType);
        }

        public bool HasEffect(EffectType pEffectType)
        {
            return _effectList.NullSafeAny(e => e.EffectType == pEffectType);
        }

        public void RemoveExpiredEffects()
        {
            var toBeKeptEffects = new List<Effect>();
            foreach (var effect in _effectList)
            {
                if (effect.Duration <= 0 || effect.Charges <= 0)
                {
                    effect.Delete(effect.EffectId.Value);
                }
                else
                {
                    toBeKeptEffects.Add(effect);
                }
            }

            _effectList = toBeKeptEffects;
        }
    }

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

        [PersistProperty]
        public int? Duration { get; set; }

        [PersistProperty]
        public int? Charges {get; set;}

        [PersistProperty]
        public int? EffectHolderPlayerId {get; set;}

        [PersistProperty]
        public int? EffectHolderMonsterId {get; set;}

        public virtual string Name {get; set;}

        // From other tables
        // The effect can be applied to players or monsters.
        public Player EffectHolderPlayer { get; protected set; }
        public MonsterInstance EffectHolderMonster { get; protected set; }

        public Effect()
        {
            ILoggerProvider fileLoggerProvider = new BossFightLoggerProvider("Effect.txt");
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
                //Name = pBaseEffect.Name;  // Should be set by the subclass
                EffectHolderPlayer = pBaseEffect.EffectHolderPlayer;
                EffectHolderMonster = pBaseEffect.EffectHolderMonster;
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
                    //Name = pBaseEffect.Name;  // Should be set by the subclass
                    EffectHolderPlayer = pBaseEffect.EffectHolderPlayer,
                    EffectHolderMonster = pBaseEffect.EffectHolderMonster
                };
                return newInstance;
            }
        }

        public Effect BuildEffect()
        {
            return EffectType switch
            {
                EffectType.DivineShield => EffectFactory<DivineShieldEffect>.CreateNewEffectAndCopyProperties(this),
                EffectType.Intimidate => EffectFactory<IntimidateEffect>.CreateNewEffectAndCopyProperties(this),
                _ => throw new Exception("Invalid EffectType for Effect building"),
            };
        }

        protected virtual bool Apply(ITarget pTarget)
        {
            _logger.LogTrace("Applying effect to {pTarget}", pTarget);
            SetEffectHolderITarget(pTarget);
            return pTarget.AddEffect(this);
        }

        public virtual void Remove(ITarget pTarget)
        {
            _logger.LogTrace("Removing effect from {pTarget}", pTarget);
            pTarget.RemoveEffect(EffectType);
        }

        public bool EffectBelongsToPlayer()
        {
            return EffectHolderPlayer?.PlayerId != null || EffectHolderPlayerId.HasValue;
        }

        public bool EffectBelongsToMonster()
        {
            return EffectHolderMonster?.MonsterInstanceId != null || EffectHolderMonsterId.HasValue;
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

        public virtual void OnDamageReceived(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeReceived) { }
        public virtual void OnDamageDealt(ITarget pBuffHolder, ITarget pDamageDealer, ref int pDamageToBeDealt) { }

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
                    EffectType = (EffectType)reader.GetInt(nameof(Models.EffectType)),
                    EffectId = reader.GetInt(nameof(EffectId)),
                    Duration = reader.GetIntNullable(nameof(Duration)),
                    Charges = reader.GetIntNullable(nameof(Charges)),
                    EffectHolderPlayerId = reader.GetIntNullable(nameof(EffectHolderPlayerId)),
                    EffectHolderMonsterId = reader.GetIntNullable(nameof(EffectHolderMonsterId))
                };
                result.Add(effect);
            }
            reader.Close();

            /*
            foreach (var effect in result)
            {

            }
            */

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

            return TrimAdditionalSearchCriteriaText(additionalSearchCriteriaText, pStartWithAnd);
        }

        #endregion PersistableBase implementation
    }

    public class DivineShieldEffect : Effect, IProcsOnDamageTaken
    {
        public override string Name {get; set;} = "Divine Shield";
        public DivineShieldEffect()
        : base()
        { }

        public DivineShieldEffect(ITarget pCaster)
        : base()
        {
            Charges = 2;
            EffectType = EffectType.DivineShield;
            if (Apply(pCaster))
                Persist();
        }

        public override void OnDamageReceived(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeReceived)
        {
            pDamageToBeReceived = 0;
            Charges -= 1;
            Persist();
        }
    }

    public class IntimidateEffect : Effect
    {
        public override string Name {get; set;} = "Intimidate";
        public IntimidateEffect()
        : base()
        { }

        public IntimidateEffect(ITarget pDebuffTarget)
        : base()
        {
            Charges = 4;
            if (Apply(pDebuffTarget))
                Persist();
        }

        public override void OnDamageDealt(ITarget pBuffHolder, ITarget pDamageDealer, ref int pDamageToBeDealt)
        {
            double newDamage = pDamageToBeDealt * 0.75;
            pDamageToBeDealt = (int)Math.Ceiling(newDamage);
            Charges -= 1;
            Persist();
        }
    }
}
