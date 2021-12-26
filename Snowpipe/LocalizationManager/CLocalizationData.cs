using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DataLoadLib.Global;
using System.Linq;
using ProjectS;

namespace DataFileEnum
{
    public class CLocalizationData
    {
        public TableInfo DataTable { get; private set; }
        public string ID { get; private set; }
        public string Korean { get; private set; }        // 한국어
        public string English { get; private set; }       // 영어
        public string Japanese { get; private set; }      // 일본어

        public CLocalizationData(TableInfo cInfo)
        {
            this.DataTable = cInfo;
            this.ID = cInfo.GetStrValue(0);

            SetInfo(cInfo);
        }

        private void SetInfo(TableInfo cInfo)
        {
            this.Korean = cInfo.GetStrValue((int)ELanguage.Korean);
            this.English = cInfo.GetStrValue((int)ELanguage.English);
            this.Japanese = cInfo.GetStrValue((int)ELanguage.Japanese);
        }

        public string GetLanguage(int nIndex)
        {
            return GetLanguage((ELanguage)nIndex);
        }

        public string GetLanguage(ELanguage eLanguage)
        {
            switch (eLanguage)
            {
                case ELanguage.Korean: return Korean;
                case ELanguage.English: return English;
                case ELanguage.Japanese: return Japanese;
            }

            return string.Empty;
        }

        public static string[] GetAllLocalKey()
        {
            return Enum.GetNames(typeof(ELanguage));
        }

        public static ELanguage GetDefaultLanguageCode_Text()
        {
#if UNITY_EDITOR
            return ELanguage.Korean;
#else
            return ELanguage.English;
#endif
        }
    }
}
