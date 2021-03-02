using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SFA.DAS.RoatpOversight.Web.Extensions
{
    public static class ITempDataDictionaryExtensions
    {
        public static void AddValue(this ITempDataDictionary tempData, string key, object value)
        {
            tempData[key] = value;
        }

        public static T GetValue<T>(this ITempDataDictionary tempData, string key)
        {
            if (!tempData.ContainsKey(key))
            {
                return default(T);
            }

            var value = tempData[key];
            return (T)value;
        }
    }
}
