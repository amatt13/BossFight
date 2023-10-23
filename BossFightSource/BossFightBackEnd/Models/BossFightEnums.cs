using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BossFight.Extentions;

namespace BossFight.BossFightEnums
{
    // NB: remember to update the SQL table as well when making changes here
    public enum MonsterType
    {
        PLAYER = 0,  // Should only be used by player characters
        HUMANOID = 1,
        UNDEAD = 2,
        BEAST = 3,
        DRAGON = 4,
        DEMON = 5,
        MAGIC_CREATURE = 6
    }

    public enum MonsterTierVoteChoice
    {
        DECREASE_DIFFICULTY = -1,
        UNCHANGED = 0,
        INCREASE_DIFFICULTY = 1
    }

    public enum PlayerClassEnum
    {
        CLERIC = 1,
        HIGHWAYMAN = 2,
        RANGER = 3,
        HEXER = 4,
        MAGE = 5,
        BARBARIAN = 6,
        MONSTER_HUNTER = 7,
        PALADIN = 8
    }

    public static class EnumTextFormatter
    {
        private static readonly TextInfo _textInfo = new CultureInfo("en-UK", false).TextInfo;

        /// <summary>
        /// Print ther enum value in title case with the '_' repalced by ' '
        /// </summary>
        public static string EnumPrinter(Enum pEnumValue)
        {
            return _textInfo.ToTitleCase(pEnumValue.ToString().Replace("_", " ").ToLower());
        }

        /// <summary>
        /// Print every value in the Enum IEnumerable
        /// </summary>
        public static string EnumPrinter<T>(IEnumerable<T> pEnumValues, string pDelimiter = ", ")
        where T: Enum
        {
            var strings = new List<String>();
            pEnumValues.ForEach(value => strings.Add(EnumPrinter(value)));
            return String.Join(pDelimiter, strings);
        }

        /// <summary>
        /// Print every value in the Enum array
        /// </summary>
        public static string EnumPrinter(Array pEnumValues, string pDelimiter = ", ")
        {
            var strings = new List<String>();
            pEnumValues.Cast<Enum>().ForEach(value => strings.Add(EnumPrinter(value)));
            return String.Join(pDelimiter, strings);
        }

        /// <summary>
        /// Print every value in the Enum type
        /// </summary>
        public static string EnumPrinter(Type pEnumType, string pDelimiter = ", ")
        {
            var enumValues = Enum.GetValues(pEnumType);
            var newValues = enumValues.Cast<Enum>().ToList();
            return EnumPrinter(newValues, pDelimiter);
        }
    }
}
