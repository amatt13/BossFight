using System.Linq;

namespace BossFight.Models
{
    public interface ITarget
    {       
        int Hp { get; set; }
        int Mana { get; set; }
        string Name { get; set; }
        int Level { get; }

        abstract int GetMaxHp();
        bool IsDead();
        bool IsAlive();
        bool IsAtFullHealth();
        string PossessiveName();
    }
}