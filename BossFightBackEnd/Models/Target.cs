using System;
using System.Linq;

namespace BossFight.Models
{
    public abstract class Target : PersistableBase
    {       
        [PersistPropertyAttribute]
        public int Hp { get; set; }

        public virtual string Name { get; set; }

        public Target(int pHP = 1, string pName = "No name")
        {
            Hp = pHP;
            Name = pName;
        }

        public virtual int GetMaxHp()
        {
            throw new NotImplementedException("get_max_hp() not implemented for class");
        }

        public virtual bool IsDead()
        {
            return Hp <= 0;
        }

        public virtual bool IsAlive()
        {
            return !IsDead();
        }

        public virtual bool IsAtFullHealth()
        {
            return Hp >= GetMaxHp();
        }

        public string PossessiveName()
        {
            if (Name.Last() == 's')
            {
                return Name + "'";
            }
            else
            {
                return Name + "'s";
            }
        }
    }
}