using System;
using System.Linq;
using BossFight.Models.DB;
using MySqlConnector;

namespace BossFight.Models
{
    public abstract class Target : PersistableBase
    {
        public int HP { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public override string TableName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string IdColumn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        // public Target(AppDb db) : base(db)
        // {}

        public Target(int pHP = 1, string pName = "No name", int pLevel = 1)
        {
            HP = pHP;
            Name = pName;
            Level = pLevel;
        }

        public virtual int GetMaxHp()
        {
            throw new NotImplementedException("get_max_hp() not implemented for class");
        }

        public virtual bool IsDead()
        {
            return HP <= 0;
        }

        public virtual bool IsAlive()
        {
            return !IsDead();
        }

        public virtual bool IsAtFullHealth()
        {
            return HP >= GetMaxHp();
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