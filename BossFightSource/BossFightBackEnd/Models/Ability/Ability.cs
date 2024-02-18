using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class Ability
    {
        public string Name { get; private set; }

        public string AbilityCastKey { get {return GetType().Name; }}

        public string Description { get; set; }

        [JsonIgnore]
        public ITarget Caster { get; set; }

        [JsonIgnore]
        public ITarget Target { get; set; }

        public bool OnlyTargetMonster { get; set; }

        public int ManaCost { get; set; }

        public bool AffectsAllPlayers { get; set; }

        public Ability(string pName, string pDescription, int pManaCost)
        {
            Name = pName;
            Description = pDescription;
            Caster = null;
            Target = null;
            OnlyTargetMonster = false;
            ManaCost = pManaCost;
            AffectsAllPlayers = false;
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual AbilityResult UseAbility(ITarget pCaster, ITarget pTarget)
        {
            var abilityResult = new AbilityResult();
            var errors = String.Empty;
            Caster = pCaster;
            Target = pTarget;

            if (CanCastAbility(ref errors)) {
                TargetEffect(pTarget, abilityResult);
                SubtractManaCostFromCaster();
                abilityResult.AbilityResultText = abilityResult.AbilityResultText.TrimEnd(new char[] { '\r', '\n' });
                abilityResult.CastSuccess = true;
            }
            else
            {
                errors += $"Could not use ability {Name}\n";
                abilityResult.CastSuccess = false;
            }

            errors = errors.TrimEnd(new char[] { '\r', '\n' });
            abilityResult.Error = errors;

            return abilityResult;
        }

        public virtual bool CanCastAbility(ref string pError)
        {
            var canCastAbility = true;
            if (ManaCost >= Caster.Mana)
            {
                pError += "You do not have enough mana\n";
                canCastAbility = false;
            }
            else if (Caster.IsDead())
            {
                pError += "You are knocked out\n";
                canCastAbility = false;
            }

            if (Caster is Player player)
            {
                var playerPlayerClass = player.PlayerPlayerClass;
                var playerClass = playerPlayerClass.PlayerClass.RecalculateUnlockedAbilities(playerPlayerClass.Level).Any(ability => ability.Name == Name);  // Just check for the name... should be good enough
            }

            return canCastAbility;
        }

        // The effect that will be executed on the target
        public virtual void TargetEffect(ITarget pTarget, AbilityResult pAbilityResult)
        { }

        public virtual void AffectsAllPlayersEffect(List<Player> allPlayers)
        { }

        public void SubtractManaCostFromCaster()
        {
            Caster.Mana -= ManaCost;
        }
    }
}
