using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectS
{
    public enum SFXType
    {
        _2D,
        _3D
    }

    public class SFXManager : ManagerBase
    {
        private PoolBase m_cPoolSFX;

        private bool m_bMute = false;
        public bool Mute
        {
            get
            {
                return m_bMute;
            }
            set
            {
                if (m_cPoolSFX == null)
                    return;

                if (m_bMute == value)
                    return;

                m_bMute = value;

                var list_SfxObj = m_cPoolSFX.GetAllItem();
                for (int i = 0; i < list_SfxObj.Count; ++i)
                {
                    var sfxObj = list_SfxObj[i] as SFXObject;
                    if (sfxObj == null)
                        continue;

                    sfxObj.AudioSource.mute = !value;
                }
            }
        }

        private float m_fVolume = 1f;

        public float Volume
        {
            get
            {
                return m_fVolume;
            }
            set
            {
                if (m_fVolume == value)
                    return;

                m_fVolume = value;
            }
        }

        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            // 임시 프리펩 생성
            var prefSFX = Managers.Asset.LoadAsset<GameObject>("SfxObject", false);

            if (prefSFX == null)
            {
                prefSFX = new GameObject("SfxObject");
                prefSFX.GetOrAddComponent<SFXObject>();
                m_cPoolSFX.Init(prefSFX, 32);
                GameObject.Destroy(prefSFX);
            }
            else
            {
                prefSFX.GetOrAddComponent<SFXObject>();
                m_cPoolSFX.Init(prefSFX, 32);
            }

            Mute = !Managers.LocalData.GetUseSFX();
            Volume = Managers.LocalData.GetSFXVolume();
            yield break;
        }

        public void Awake()
        {
            m_cPoolSFX = this.gameObject.AddComponent(typeof(PoolBase)) as PoolBase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vLocalPos">로컬 좌표, tr_Parent == null이면 월드 좌표</param>
        /// <param name="qLocalRot">로컬 회전, tr_Parent == null이면 월드 회전</param>
        /// <param name="vLocalScale">로컬 스케일, tr_Parent == null이면 월드 스케일</param>
        /// <param name="tr_Parent">부모 Transform</param>
        /// <param name="clip">재생할 AudioClip</param>
        /// <param name="isLoof">반복 재생 여부</param>
        /// <param name="playTimeOffset">빨리감기 Time</param>
        /// <returns></returns>
        public SFXObject PlaySFX(
            AudioClip clip,
            Vector3 vLocalPos,
            Quaternion qLocalRot, 
            Vector3 vLocalScale, 
            Transform tr_Parent, 
            SFXType sfxType,
            bool isLoof,
            float playTimeOffset = 0f)
        {
            if (clip == null)
                return null;

            var sfx = m_cPoolSFX.Pop(PoolBase.PopOptionForNotEnough.Force) as SFXObject;

            sfx.GenerateObject(vLocalPos, qLocalRot, vLocalScale, tr_Parent);
            sfx.PlaySFX(clip, sfxType, isLoof, playTimeOffset, null);
            sfx.AudioSource.volume = this.Volume;
            return sfx;
        }

        public SFXObject PlaySFX(
            AudioClip clip,
            Vector3 vLocalPos,
            Quaternion qLocalRot,
            Vector3 vLocalScale,
            Transform transParent,
            SFXType eSFXType,
            Action<SFXObject> endCallback,
            float fPlayTimeOffset = 0f)
        {
            if (clip == null)
            {
                endCallback?.Invoke(null);
                return null;
            }

            var sfx = m_cPoolSFX.Pop(PoolBase.PopOptionForNotEnough.Force) as SFXObject;

            sfx.GenerateObject(vLocalPos, qLocalRot, vLocalScale, transParent);
            sfx.PlaySFX(clip, eSFXType, false, fPlayTimeOffset, endCallback);
            sfx.AudioSource.volume = this.Volume;
            return sfx;
        }

        public SFXObject PlaySFX(
            AudioClip clip,
            Transform transParent,
            SFXType eSFXType,
            bool bLoop,
            float fPlayTimeOffset = 0f)
        {
            return PlaySFX(clip, Vector3.zero, Quaternion.identity, Vector3.one,  transParent,eSFXType, bLoop, fPlayTimeOffset);
        }

        public SFXObject PlaySFX(
            AudioClip clip,
            Transform transParent,
            SFXType eSFXType,
            Action<SFXObject> endCallback,
            float fPlayTimeOffset = 0f)
        {
            return PlaySFX(clip, Vector3.zero, Quaternion.identity, Vector3.one, transParent, eSFXType, endCallback, fPlayTimeOffset);
        }

        public SFXObject PlaySFX(AudioClip clip, SFXType eSFXType)
        {
            return PlaySFX(clip, Vector3.zero, Quaternion.identity, Vector3.one, this.transform, eSFXType, false, 0.0f);
        }

        public SFXObject PlaySFX(AudioClip clip, SFXType eSFXType, Action<SFXObject> endCallback)
        {
            return PlaySFX(clip, Vector3.zero, Quaternion.identity, Vector3.one, this.transform, eSFXType, endCallback, 0.0f);
        }


        public void RetrieveAllItems()
        {
#if DEV
            Debug.Log("SFXManager->RetrieveAllItems");
#endif
            m_cPoolSFX.RetrieveAllItems();
        }
    }
}
