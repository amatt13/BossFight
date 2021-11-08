using System;
using System.Linq;
using BossFight.Models.DB;
using MySqlConnector;

namespace BossFight.Models
{
    public abstract class Target : PersistableBase
    {
        public int Hp { get; set; }
        public string Name { get; set; }
        public override string TableName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string IdColumn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


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

        public virtual object PossessiveName()
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