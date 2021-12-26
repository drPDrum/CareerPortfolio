using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProjectS
{
    public partial class LocalDataManager
    {
        #region Resolution
        public Managers.EResolution GetResolution()        
        {
            string strCode = EKey.Resolution.ToString();

            return (Managers.EResolution)(PlayerPrefs.GetInt(strCode, 0));
        }

        public void SetResolution(Managers.EResolution eResolution)
        {
            string strCode = EKey.Resolution.ToString();

            PlayerPrefs.SetInt(strCode, (int)eResolution);
            Save();
        }


        #endregion Resolution

        #region FrameRate
        public int GetFrameRate()
        {
            string strCode = EKey.FrameRate.ToString();

            // 2021.04.28 PD님 요청으로 기본 60 -> 30 변경
            return PlayerPrefs.GetInt(strCode, 30);
        }

        public void SetFrameRate(int nFrameRate)
        {
            string strCode = EKey.FrameRate.ToString();

            PlayerPrefs.SetInt(strCode, nFrameRate);
            Save();
        }
        #endregion FrameRate

        #region ImageQuality

        public Managers.EGameQuality GetGameQuality()
        {
            string strCode = EKey.GameQuility.ToString();

            return (Managers.EGameQuality)PlayerPrefs.GetInt(strCode, (int)Managers.EGameQuality.High);
        }

        public void SetGameQuality(Managers.EGameQuality eQuality)
        {
            string strCode = EKey.GameQuility.ToString();

            PlayerPrefs.SetInt(strCode, (int)eQuality);
            Save();
        }

        #endregion ImageQuality        

        #region SFX
        public bool GetUseSFX()
        {
            string strCode = EKey.SFX.ToString();

            return !PlayerPrefs.HasKey(strCode);
        }

        public void SetUseSFX(bool bActive)
        {
            string strCode = EKey.SFX.ToString();

            if (bActive)
            {
                if (PlayerPrefs.HasKey(strCode))
                {
                    PlayerPrefs.DeleteKey(strCode);
                    Save();
                }
            }
            else
            {
                if (!PlayerPrefs.HasKey(strCode))
                {
                    PlayerPrefs.SetInt(strCode, 1);
                    Save();
                }
            }
        }

        public float GetSFXVolume()
        {
            string strCode = EKey.SFX_Volume.ToString();

            return PlayerPrefs.GetFloat(strCode, 1f);
        }

        public void SetSFXVolume(float fVolume)
        {
            string strCode = EKey.SFX_Volume.ToString();

            fVolume = Mathf.Clamp01(fVolume);

            if (GetSFXVolume() == fVolume)
                return;

            if (PlayerPrefs.HasKey(strCode))
                PlayerPrefs.DeleteKey(strCode);

            PlayerPrefs.SetFloat(strCode, fVolume);
            Save();
        }
        #endregion SFX

        #region BGM
        public bool GetUseBGM()
        {
            string strCode = EKey.BGM.ToString();

            return !PlayerPrefs.HasKey(strCode);
        }

        public void SetUseBGM(bool bActive)
        {
            string strCode = EKey.BGM.ToString();

            if (bActive)
            {
                if (PlayerPrefs.HasKey(strCode))
                {
                    PlayerPrefs.DeleteKey(strCode);
                    Save();
                }
            }
            else
            {
                if (!PlayerPrefs.HasKey(strCode))
                {
                    PlayerPrefs.SetInt(strCode, 1);
                    Save();
                }
            }
        }

        public float GetBGMVolume()
        {
            string strCode = EKey.BGM_Volume.ToString();

            return PlayerPrefs.GetFloat(strCode, 0.67f);
        }

        public void SetBGMVolume(float fVolume)
        {
            string strCode = EKey.BGM_Volume.ToString();

            fVolume = Mathf.Clamp01(fVolume);

            if (GetBGMVolume() == fVolume)
                return;

            if (PlayerPrefs.HasKey(strCode))
                PlayerPrefs.DeleteKey(strCode);

            PlayerPrefs.SetFloat(strCode, fVolume);
            Save();
        }

        #endregion BGM

        #region Language
        public ELanguage GetLanguage()
        {
            string strCode = EKey.Language.ToString();
            var eLanguage = ELanguage.English;

            if (!PlayerPrefs.HasKey(strCode))
                return eLanguage;

            return (ELanguage)PlayerPrefs.GetInt(strCode, 1); 
        }

        public void SetLanguage(ELanguage eLanguage)
        {
            string strCode = EKey.Language.ToString();

            if (PlayerPrefs.HasKey(strCode))
                PlayerPrefs.DeleteKey(strCode);

            PlayerPrefs.SetInt(strCode, (int)eLanguage);
            // 즉시 바꿔줄 필요가 있음, 바로 매니저가 초기화 되기 때문에...
            PlayerPrefs.Save();
        }

        #endregion Language

        public bool GetUseAutoSkillState()
        {
            string strCode = EKey.UseAutoSkill.ToString();
            return PlayerPrefs.GetInt(strCode, 0) == 1;
        }

        public void SetUseAutoSkillState(bool state)
        {
            string strCode = EKey.UseAutoSkill.ToString();
            if (PlayerPrefs.HasKey(strCode))
                PlayerPrefs.DeleteKey(strCode);

            PlayerPrefs.SetInt(strCode, state ? 1 : 0);
            PlayerPrefs.Save();
        }

        public float GetGameSpeed()
        {
            string strCode = EKey.GameSpeed.ToString();
            return PlayerPrefs.GetFloat(strCode, 1f);
        }

        public void SetGameSpeed(float value)
        {
            string strCode = EKey.GameSpeed.ToString();
            if (PlayerPrefs.HasKey(strCode))
                PlayerPrefs.DeleteKey(strCode);

            PlayerPrefs.SetFloat(strCode, value);
            PlayerPrefs.Save();
        }
    }
}
