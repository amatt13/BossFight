using System;
using System.Collections.Generic;
using System.Linq;
using BossFight.CustemExceptions;
using Microsoft.AspNetCore.Components.Web;

namespace BossFight.Models
{
    public class Ability
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ITarget Caster { get; set; }
        public string UseAbilityText { get; set; }
        public bool OnlyTargetMonster { get; set; }
        public int ManaCost { get; set; }
        public string MagicWord { get; set; }
        public bool AffectsAllPlayers { get; set; }
        public string AffectsAllPlayersStr { get; set; }

        public Ability(string pName, string pDescription, int pManaCost, string pMagicWord)
        {
            Name = pName;
            Description = pDescription;
            Caster = null;
            UseAbilityText = "";
            OnlyTargetMonster = false;
            ManaCost = pManaCost;
            MagicWord = pMagicWord.ToLower();
            AffectsAllPlayers = false;
            AffectsAllPlayersStr = "";
        }

        public override string ToString()
        {
            var onlyTargetMonsterString = "";
            if (OnlyTargetMonster)
                onlyTargetMonsterString = "*";
                
            return $"{ ManaCost } mana - **{ MagicWord }**/**{ Name }**{ onlyTargetMonsterString } -> { Description }";
        }

        public virtual string UseAbility(Player pCaster, ITarget pTarget, bool pDontUseCasterEffect = false)
        {
            UseAbilityText = "";
            Caster = pCaster;
            if (OnlyTargetMonster && pTarget is Player)
                throw new MyException("Can only target monsters");
                
            if (!pDontUseCasterEffect)
                CasterEffect();
                
            if (pTarget != null)
                TargetEffect(pTarget);
                
            SubtractManaCostFromCaster();
            AddManaText();
            return AffectsAllPlayersStr + UseAbilityText;
        }

        // The effect that will be executed on the caster
        public virtual void CasterEffect()
        { }

        // The effect that will be executed on the target
        public virtual void TargetEffect(ITarget pTarget)
        { }

        public virtual void AffectsAllPlayersEffect(List<Player> allPlayers)
        { }

        public void SubtractManaCostFromCaster()
        {
            if (Caster.Mana < ManaCost)
                throw new WTFException($"caster mana: { Caster.Mana} mana cost: { ManaCost }");
                
            Caster.Mana -= ManaCost;
        }

        public void AddManaText()
        {
            if (UseAbilityText.Any() && !UseAbilityText.EndsWith('\n'))
                UseAbilityText += "\n";
                
            UseAbilityText += $"**Mana:** You have { Caster.Mana } mana left";
        }

        public string BoldNameWithColon()
        {
            return $"**{Name}:**";
        }
    }
}
