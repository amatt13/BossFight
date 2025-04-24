using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public interface IEffectHolder
    {
        /// <summary>
        /// Returns true if the effect was added to the ITarget
        /// </summary>
        bool AddEffect(Effect pEffect, bool pReplaceEffect);
        void RemoveEffect(Effect pEffect);
        bool HasEffect(Effect pEffect, out Effect foundEffect);
        void RemoveExpiredEffects();
    }
}
