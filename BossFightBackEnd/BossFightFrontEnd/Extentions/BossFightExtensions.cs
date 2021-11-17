using System;

namespace BossFight.Extentions
{
    public static class BossFightExtensions
    {
        public static string ToDbString(this object pObject)
        {
            string result = null;
            if (pObject is String)
            {
                result = (pObject as string).ToDbString();
            }
            else if (pObject is bool?)
            {
                result = (pObject as bool?).ToDbString();
            }
            else if (pObject is bool)
            {
                result = ((bool)pObject).ToDbString();
            }
            else
                result = pObject.ToString();

            return result;
        }

        public static string ToDbString(this String pString)
        {
            return pString != null ? $"'{ pString.Replace("'", @"\'") }'" : null;
        }

        public static string ToDbString(this bool? pBool)
        {
            string result = null;
            if (pBool.HasValue)
                result = pBool.Value ? "True" : "False";

            return result;
        }

        public static string ToDbString(this bool pBool)
        {
            return pBool ? "True" : "False";
        }
    }
}