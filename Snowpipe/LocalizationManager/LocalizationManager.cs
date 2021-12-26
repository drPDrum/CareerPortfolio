using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.IO;
using System.Linq;
using DataFileEnum;
using DataLoadLib;
using DataLoadLib.Global;
using Newtonsoft.Json;

namespace ProjectS
{
    public enum ELanguage
    {
        None = -1,
        Common = 0,
        Korean,
        English,
        Japanese,
    }

    public partial class LocalizationManager : ManagerBase
    {
        public static ELanguage Language_Text { get; set; } = ELanguage.None;

        private Dictionary<string, CLocalizationData> m_dicLocalizationData = new Dictionary<string, CLocalizationData>();
        private Dictionary<string, string> _dic_Localization = new Dictionary<string, string>();

#if UNITY_EDITOR && USE_LOG
        private Dictionary<string, List<string>> m_dicSameKeys = new Dictionary<string, List<string>>();
#endif

#if UNITY_EDITOR
        public static bool IsLoadedData { get; private set; } = false;
#endif

        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            Language_Text = Managers.LocalData.GetLanguage();
            Debug.LogFormat("<color=yellow>Language : {0}</color>", Language_Text);
            // Load System Text

            var arrFiles = Resources.LoadAll<TextAsset>("System_Localization");
            if (arrFiles != null)
            {
                for (int i = 0 ; i < arrFiles.Length ; ++i)
                {
                    if (arrFiles[i] == null)
                        continue;

                    LoadLocalizationData(arrFiles[i], false);

#if USE_LOG
                    Debug.LogFormat("<color=cyan>System Localization File {0} Loaded.</color>", arrFiles[i].name);
#endif
                }
            }

            yield break;
        }

        public void LoadAllData()
        {
#if USE_FULL_BUILD
            var arrFiles = Resources.LoadAll<TextAsset>("Localization");
            if(arrFiles != null)
            {
                for(int i = 0 ; i < arrFiles.Length ; ++i)
                {
                    if (arrFiles[i] == null)
                        continue;

                    LoadLocalizationData(arrFiles[i]);
                }
            }

#elif UNITY_EDITOR
            string strMainPath = "Assets/_DownloadableAssets/";
            string strDataPath = "Localization/";

            var dir = strMainPath + strDataPath;
            var files = Directory.GetFiles(dir, "*.bytes", SearchOption.TopDirectoryOnly);

            for (int i = 0 ; i < files.Length ; i++)
            {
                string resName = files[i].Replace(dir, string.Empty);
                resName = resName.Replace(".bytes", string.Empty);

                TextAsset ta = Managers.Asset.LoadAsset<TextAsset>(strDataPath + resName, true);

                //Debug.Log("Res Name : " + resName);
                if(ta is null)
                    Debug.LogError($"TextAsset {resName} is NULL.");
                else
                    LoadLocalizationData(ta);
            }
#else
            var dataValue = Managers.Asset.AssetInfo.Where(x => x.Value == "Localization");
            foreach (var das in dataValue)
            {
                string resName = das.Key;

               resName = resName.Replace("Localization/", string.Empty);

               TextAsset ta = Managers.Asset.LoadAsset<TextAsset>(das.Key, true);

                if (ta is null)
                    Debug.LogError($"TextAsset {resName} is NULL.");
                else
                    LoadLocalizationData(ta);
            }
#endif


#if UNITY_EDITOR && USE_LOG
            System.Text.StringBuilder stb = new System.Text.StringBuilder();
            foreach (var pair in m_dicSameKeys)
            {
                if (pair.Value.Count <= 1)
                    continue;

                stb.Append(pair.Key);
                stb.Append(" :: ");
                for (int i = 0 ; i < pair.Value.Count ; ++i)
                {
                    stb.Append(pair.Value[i]);
                    if (i != pair.Value.Count - 1)
                        stb.Append(" / ");
                }

                stb.AppendLine();
            }

            if (stb.Length > 0)
            {
                Debug.LogError("Duplicated Localization Keys");
                Debug.LogError(stb.ToString());
            }
#endif

#if UNITY_EDITOR
            IsLoadedData = true;
#endif
        }

        public string this[string strKey]
        {
            get
            {
                if (string.IsNullOrEmpty(strKey))
                    return null;

                var lzData = m_dicLocalizationData?.GetOrNull(strKey);
                if (lzData == null)
                {
#if USE_LOG
                    Debug.LogError($"LocalDataManager->NotFoundKey :: {strKey}");
#endif
                    return strKey;
                }

#if USE_LOG
                var result = lzData.GetLanguage(Language_Text);
                if (string.IsNullOrWhiteSpace(result))
                    Debug.LogError($"LocalDataManager->KeyFoundButNotSet :: {strKey} / {Language_Text}");

                return result;
#else
                return lzData.GetLanguage(Language_Text);
#endif
            }
        }

        public string this[string strKey, params object[] values]
        {
            get
            {
                if (string.IsNullOrEmpty(strKey))
                    return null;

                try
                {
                    var lzData = m_dicLocalizationData?.GetOrNull(strKey);
                    if (lzData == null)
                    {
#if USE_LOG
                        Debug.LogError($"LocalDataManager->NotFoundKey :: {strKey}");
#endif
                        return strKey;
                    }


#if USE_LOG
                    var result = lzData.GetLanguage(Language_Text);
                    if (string.IsNullOrWhiteSpace(result))
                        Debug.LogError($"LocalDataManager->KeyFoundButNotSet :: {strKey} / {Language_Text}");

                    return string.Format(result, values);
#else
                    return string.Format(lzData.GetLanguage(Language_Text), values);
#endif
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("The count of prameters is wrong!!!!! [" + strKey + "][" + ex.ToString() + "]");
                    return strKey;
                }
            }
        }

        public bool GetText(string strKey, out string result)
        {
            if (m_dicLocalizationData == null || string.IsNullOrWhiteSpace(strKey))
            {
                result = null;
                return false;
            }

            var lzData = m_dicLocalizationData.GetOrNull(strKey);
            if (lzData == null)
            {
                result = null;
                return false;
            }

            result = lzData.GetLanguage(Language_Text);
#if USE_LOG
            if (string.IsNullOrWhiteSpace(result))
                Debug.LogError($"LocalDataManager->KeyFoundButNotSet :: {strKey} / {Language_Text}");
#endif

            return true;
        }

        public string GetLanguageText(ELanguage eLanguage, string strKey)
        {
            if (eLanguage == ELanguage.None || string.IsNullOrEmpty(strKey))
                return null;

            var lzData = m_dicLocalizationData?.GetOrNull(strKey);
            if (lzData == null)
            {
#if USE_LOG
                Debug.LogError($"LocalDataManager->NotFoundKey :: {strKey}");
#endif
                return strKey;
            }

#if USE_LOG
            var result = lzData.GetLanguage(eLanguage);
            if (string.IsNullOrWhiteSpace(result))
                Debug.LogError($"LocalDataManager->KeyFoundButNotSet :: {strKey} / {eLanguage}");

            return result;
#else
            return lzData.GetLanguage(eLanguage);
#endif
        }

        public static ELanguage ConvertSystemToLocalizationLanguageCode(SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case SystemLanguage.Korean: return ELanguage.Korean;
                case SystemLanguage.English: return ELanguage.English;
                case SystemLanguage.Japanese: return ELanguage.Japanese;
            }

            return ELanguage.None;
        }

        public void LoadLocalizationData(TextAsset asset, bool bEncFile = true)
        {
            var listData = GameDataManager.GetTableData(asset, bEncFile);
            if (listData == null)
                return;

            for (int i = 0 ; i < listData.Count ; ++i)
            {
                TableInfo tableInfo = new TableInfo();
                tableInfo.SetValue(listData[i]);
                CLocalizationData cData = new CLocalizationData(tableInfo);

                m_dicLocalizationData.AddOrRefresh(cData.ID, cData);

#if UNITY_EDITOR && USE_LOG
                var list_FileName = m_dicSameKeys.GetOrCreate(cData.ID);
                list_FileName.Add(asset.name);
#endif
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Utils/Change Language/Korean")]
        public static void ChangeLocalizationText_Korea()
        {
            LocalDataManager.SetIntLocalData(LocalDataManager.EKey.Language, (int)ELanguage.Korean);
            Debug.Log("Language Changed : " + ELanguage.Korean);
        }

        [UnityEditor.MenuItem("Utils/Change Language/English")]
        public static void ChangeLocalizationText_English()
        {
            LocalDataManager.SetIntLocalData(LocalDataManager.EKey.Language, (int)ELanguage.English);
            Debug.Log("Language Changed : " + ELanguage.English);
        }
#endif
    }
}