using System;

namespace BossFight.Models
{
    public class PlayerClassRequirement
    {
        public Type PlayerClassType  {get; set;}
        public int LevelRequirement {get; set;}
        public string ClassName {get; set;}

        public PlayerClassRequirement(Type pClassType, int pLevelRequirement, string pClassName)
        {
            PlayerClassType = pClassType;
            LevelRequirement = pLevelRequirement;
            ClassName = pClassName;
        }

        public override string ToString()
        {
            return $"{ClassName}:{PlayerClassType} - lvl {LevelRequirement}";
        }
    }
}
