using System;

namespace BossFight.Extentions
{
    public static class BossFightExtensions
    {
        public static object ToDbString(this object pObject)
        {
            object result;
            if (pObject is null)
            {
                result = "NULL";
            }
            else if (pObject is String)
            {
                result = (pObject as string).ToDbString();
            }
            else if (pObject is bool? )
            {
                result = (pObject as bool?).ToDbString();
            }
            else if (pObject is bool boolValue)
            {
                result = boolValue.ToDbString();
            }
            else if (pObject.GetType().IsEnum)
            {
                result = ((int)pObject).ToDbString();
            }
            else
                result = pObject.ToString();

            return result;
        }

        public static string ToDbString(this String pString)
        {
            return pString != null ? $"\"{ pString.Replace("'", @"\'") }\"" : null;
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

        public static bool HasText(this string pString)
        {
            return !String.IsNullOrWhiteSpace(pString);
        }
    }
}
