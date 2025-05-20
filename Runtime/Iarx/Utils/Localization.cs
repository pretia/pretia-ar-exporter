using UnityEngine;

namespace Pretia
{
    public class Localization
    {
        public static string GetLocString()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Japanese:
                    return "ja";
                default:
                    return "en";
            }
        }

    }
}