using System;
using System.Collections.Generic;
using BossFight.Extentions;
using BossFight.BossFightEnums;
using System.Linq;

namespace BossFight.Models
{
    public interface IProcsOnDamageTaken
    {
        void OnDamageReceived(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeReceived);
    }

    public interface IProcsOnDamageDealt
    {
        void OnDamageDealt(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeDealt);
    }

    public interface IProcsBeforeAttack
    {
        void OnBeforeAttack(ITarget pEffectHolder, ITarget pEffectCaster, PlayerAttackSummary pPlayerAttackSummary);
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
            EffectType = EffectType.DIVINE_SHIELD;
            if (Apply(pCaster, pCaster))
                Persist();
        }

        public override void OnDamageReceived(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeReceived)
        {
            if (pEffectHolder.IsAlive())
            {
                pDamageToBeReceived = 0;
                Charges -= 1;
                Persist();
            }
            else
            {
                Charges = 0;
                Persist();
            }
        }
    }

    public class IntimidateEffect : Effect
    {
        public override string Name {get; set;} = "Intimidate";
        public IntimidateEffect()
        : base()
        { }

        public IntimidateEffect(ITarget pDebuffTarget, ITarget pCaster)
        : base()
        {
            Charges = 4;
            if (Apply(pDebuffTarget, pCaster))
                Persist();
        }

        public override void OnDamageDealt(ITarget pEffectHolder, ITarget pDamageDealer, ref int pDamageToBeDealt)
        {
            double newDamage = pDamageToBeDealt * 0.75;
            pDamageToBeDealt = (int)Math.Ceiling(newDamage);
            Charges -= 1;
            Persist();
        }
    }

    public class DamageOverTimeEffect : Effect, IProcsBeforeAttack
    {
        public override string Name {get; set;} = "Damage over time";
        public int Damage {get; private set;}
        public DamageOverTimeEffect()
        : base()
        { }

        public DamageOverTimeEffect(ITarget pDebuffTarget, ITarget pCaster, int pDamagePerTick)
        : base()
        {
            Charges = 2;
            Damage = pDamagePerTick;
            Fields["Damage"] = Damage;
            EffectType = EffectType.DAMAGE_OVER_TIME;
            if (Apply(pDebuffTarget, pCaster))
                Persist();
        }

        public override void OnBeforeAttack(ITarget pEffectHolder, ITarget pEffectCaster, PlayerAttackSummary pPlayerAttackSummary)
        {
            if (pEffectHolder.IsAlive())
            {
                pEffectHolder.SubtractHealth(Damage, pEffectCaster);
                Charges -= 1;
                Persist();
                pPlayerAttackSummary.AddMonsterRetaliateMessage($"Fire continues to burn {pEffectHolder.Name} for {Damage} damage");
            }
            else
            {
                Charges = 0;
                Persist();
            }
        }
    }
}
