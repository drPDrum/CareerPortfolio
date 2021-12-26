using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using DataFileEnum;

namespace ProjectS
{
    public partial class LocalDataManager : ManagerBase
    {
        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            yield break;
        }

        /// <summary>
        /// Enum Value가 변경되면 키 값이 변경.
        /// </summary>
        public enum EKey
        {
            AccountID,

            Resolution,
            FrameRate,
            GameQuility,
            SFX,
            SFX_Volume,
            BGM,
            BGM_Volume,
            Language,

            UUID,

            TUTORIAL_STEP,
            
            PatchList,
            Deck,
            UseAutoSkill,
            GameSpeed,

            MAX,
        }

        private const string FMT_KEY_LOCAL = "{0}_{1}";
        private const string FMT_KEY_VARIANT = "{0}_{1}_{2}";

        private bool m_bWaitForSave = false;
        private bool m_bNonSecure = false;
        private bool m_bSecure = false;
        

        private void Save(bool bSecure = false)
        {
            if (bSecure)
                m_bSecure = true;
            else
                m_bNonSecure = true;

            if (m_bWaitForSave)
                return;

            m_bWaitForSave = true;
            StartCoroutine(RoutineSave());
        }

        private IEnumerator RoutineSave()
        {
            yield return new WaitForEndOfFrame();

            m_bWaitForSave = false;

            if (m_bNonSecure)
            {
                m_bNonSecure = false;
                PlayerPrefs.Save();
            }
            if (m_bSecure)
            {
                m_bSecure = false;
                Utils.SecurePlayerPrefs.Save();
            }
        }

        private void LocalData_Save(string key, object saveObject)
        {
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);

            if (saveObject == null)
                return;

            try
            {
                MemoryStream memory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(memory, saveObject);

                PlayerPrefs.SetString(key, Convert.ToBase64String(memory.ToArray()));
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private T LocalData_Load<T>(string key) where T : class
        {
            if (!PlayerPrefs.HasKey(key))
                return null;

            try
            {
                string deserializeValue = PlayerPrefs.GetString(key);

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memory = new MemoryStream(Convert.FromBase64String(deserializeValue));

                return (T)formatter.Deserialize(memory);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                PlayerPrefs.DeleteKey(key);
                Save();
                return null;
            }
        }
        
        public void ClearPlayerPref()
        {
            PlayerPrefs.DeleteAll();
            Save();
        }
    }
}