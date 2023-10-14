namespace BossFight.Models
{
    public interface IEffectHolder
    {
        /// <summary>
        /// Returns true if the effect was added to the ITarget
        /// </summary>
        bool AddEffect(Effect pEffect);
        void RemoveEffect(EffectType pEffectType);
        bool HasEffect(EffectType pEffectType);
        void RemoveExpiredEffects();
    }
}
