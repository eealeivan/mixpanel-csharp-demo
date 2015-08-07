using System.Collections.Generic;
using System.Diagnostics;

namespace Web
{
    internal class MixpanelDictionaryUtils
    {
        public static bool DictionaryCollectionsEqual(
            IList<IDictionary<string, object>> first, 
            IList<IDictionary<string, object>> second)
        {
            Debug.Assert(first != null); 
            Debug.Assert(second != null);

            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; i++)
            {
                if (GetDictinaryHash(first[i]) != GetDictinaryHash(second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetDictinaryHash(IDictionary<string, object> dic)
        {
            int hash = 0;

            unchecked
            {
                foreach (var key in dic.Keys)
                {
                    hash += key.GetHashCode();
                    var value = dic[key];
                    if (value == null)
                    {
                        continue;
                    }

                    var valueDic = value as IDictionary<string, object>;
                    if (valueDic != null)
                    {
                        hash += GetDictinaryHash(valueDic);
                    }

                    hash += value.GetHashCode();
                }
            }

            return hash;
        }
    }
}