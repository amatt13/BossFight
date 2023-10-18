using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BossFight.BossFightEnums;

namespace BossFight.Models
{
    public abstract class PlayerClass
    {
        public PlayerClassEnum PlayerClassId { get; set; }
        public int PlayerClassIntId { get => (int)PlayerClassId; }
        public string Name { get; set; }
        public double HpScale { get; set; }
        public double ManaScale { get; set; }
        public int PurchasePrice { get; set; }
        public int CritChance { get; set; }
        public int HpRegenRate { get; set; }
        public int ManaRegenRate { get; set; }
        public int AttackPowerBonus { get; set; }
        public int SpellPowerBonus { get; set; }
        public int BaseHealth { get; set; }
        public int BaseMana { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        protected List<Ability> _unlockedAbilities {get; set;}
        public List<Ability> UnlockedAbilities {get { return _unlockedAbilities; }}

        // From other tables
        public List<PlayerClassWeaponProficiency> ProficientWeaponTypesList { get; set; } = new List<PlayerClassWeaponProficiency>();
        public List<PlayerClassRequirement> PlayerClassRequirementList { get; set; } = new List<PlayerClassRequirement>();


        public override string ToString()
        {
            return Name;
        }

        public virtual int CalculateMaxHp(int pClassLevel)
        {
            return (int)Math.Floor(HpScale * pClassLevel) + BaseHealth;
        }

        public virtual int CalculateMaxMana(int pClassLevel)
        {
            return (int)Math.Floor(ManaScale * pClassLevel) + BaseMana;
        }

        /// <summary>
        /// Should be called once to instantiate PlayerClassRequirementList
        /// </summary>
        protected static List<PlayerClassRequirement> BuildPlayerClassRequirementList()
        {
            return new List<PlayerClassRequirement>();
        }

        /// <summary>
        /// Every Player Class must implement this method by only returning the abilties that
        /// the player is eligible for with their current level.
        /// </summary>
        public abstract List<Ability> RecalculateUnlockedAbilities(int pPlayerLevel);
    }
}
