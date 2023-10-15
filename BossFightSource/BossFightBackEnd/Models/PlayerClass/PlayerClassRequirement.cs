using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class PlayerClassRequirement
    {
        public int LevelRequirement {get; set;}

        [JsonIgnore]
        public PlayerClass RequiredPlayerClass { get; set; }

        public string RequiredPlayerClassName { get => RequiredPlayerClass.Name; }

        public PlayerClassRequirement () { }

        public PlayerClassRequirement (PlayerClass pRequiredPlayerClass, int pLevelRequirement) {
            RequiredPlayerClass = pRequiredPlayerClass;
            LevelRequirement = pLevelRequirement;
        }

        public override string ToString()
        {
            return $"{RequiredPlayerClassName}:{RequiredPlayerClassName} - lvl {LevelRequirement}";
        }
    }
}
