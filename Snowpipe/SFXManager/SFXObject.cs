using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectS
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXObject : PoolingObject
    {
        [SerializeField]
        protected AudioSource m_audio;
        public AudioSource AudioSource { get { return m_audio; } }
        public Action<SFXObject> m_onEndCallback = null;
        public string AudioClipName
        {
            get
            {
                if (m_audio == null || m_audio.clip == null)
                    return null;

                return m_audio.clip.name;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_audio = this.gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
            m_audio.spatialBlend = 1f;
        }

        protected override void LateUpdate()
        {
            if (!m_audio.isPlaying)
                ReturnToPoolForce();
            base.LateUpdate();
        }

        public override void ReturnToPoolForce()
        {
            if (m_audio != null)
            {
                m_audio.Stop();
                m_audio.clip = null;
            }

            base.ReturnToPoolForce();
            if (m_onEndCallback != null)
            {
                var call = m_onEndCallback;
                m_onEndCallback = null;
                call.Invoke(this);
            }
        }

        public override bool CheckWaitForEnd()
        {
            // 회수 요청이 오면 뒤도 안보고 풀로 보내버린다.
            return true;
        }

        public void PlaySFX(AudioClip clip, SFXType eSFXType, bool bLoop, float fPlayTimeOffset, Action<SFXObject> endCallback)
        {
            m_audio.clip = clip;
            m_audio.loop = bLoop;
            m_audio.spatialBlend = eSFXType == SFXType._2D ? 0f : 1f;
            m_onEndCallback = endCallback;

            if (fPlayTimeOffset == 0f)
            {
                m_audio.time = 0;
                m_audio.Play();
            }
            else if (fPlayTimeOffset < 0)
            {
                m_audio.PlayDelayed(fPlayTimeOffset);
            }
            else if (fPlayTimeOffset > 0)
            {
                fPlayTimeOffset = Mathf.Clamp(fPlayTimeOffset, 0f, clip.length);
                m_audio.time = fPlayTimeOffset;
                m_audio.UnPause();
            }

        }
    }
}
