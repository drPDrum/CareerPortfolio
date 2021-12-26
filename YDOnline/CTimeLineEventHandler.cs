using System;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class CTimeLineEventHandler : MonoBehaviour
{
    public enum ESoundType
    {
        None = -1,
        FadeInOut = 0,
        Effect,
        Ambient,
        AmbientLoop,
        BGM,
    }

    private SoundManager        m_cSoundMgr = null;
    private PlayableDirector    m_Director = null;    
    private Action              m_acPause = null;
    private Action              m_acEnd = null;

    public PlayableDirector Director
    {
        get
        {
            if(m_Director == null)
                m_Director = transform.GetComponent<PlayableDirector>();
            return m_Director;
        }
    }

    private void Start()
    {
        if(m_cSoundMgr == null)
            m_cSoundMgr = SoundManager.Instance;

        m_cSoundMgr.LoadAssetAll();
        m_cSoundMgr.SetReverbZone(true);
    }

    public void SetEvent(Action acVoid, bool bPauseEvent)
    {
        if(bPauseEvent)
        {
            m_acPause = null;
            m_acPause = acVoid;
        }
        else
        {
            m_acEnd += acVoid;
        }
    }
        
    public bool IsPaused { get; private set; }

    public void Pause()
    {
        if(IsPaused)
            return;
        
        if(m_acPause != null)
            m_acPause.Invoke();

        IsPaused = true;

        Director.Pause();        
    }

    public void Play()
    {
        if(!IsPaused)
            return;

        IsPaused = false;

        Director.Play();        
    }

    public void End()
    {
        Director.Pause();
        m_cSoundMgr.SetReverbZone(false);
        if(m_acEnd != null)
            m_acEnd.Invoke();

        if(m_cSoundMgr != null)
            m_cSoundMgr.StopAll();
    }

    public void PlaySound(ESoundType eType, string strSoundName, float fFadeIn, float fVolume, float fPitch, AudioReverbPreset ePreset)
    {
#if UNITY_EDITOR
        if(!Application.isPlaying)
            return;
#endif        
        if(m_cSoundMgr == null || eType == ESoundType.None)
            return;

        switch(eType)
        {
        case ESoundType.FadeInOut:
            m_cSoundMgr.FadeIn(strSoundName, fFadeIn, fVolume, fPitch, ePreset);
            break;
        case ESoundType.Effect:
            m_cSoundMgr.Play(strSoundName, fVolume, fPitch, ePreset);
            break;
        case ESoundType.Ambient:
            m_cSoundMgr.PlayAmbient(strSoundName, false, fVolume, fPitch, ePreset);
            break;
        case ESoundType.AmbientLoop:
            m_cSoundMgr.PlayAmbient(strSoundName, true, fVolume, fPitch, ePreset);
            break;
        case ESoundType.BGM:
            m_cSoundMgr.PlayBGM(strSoundName, fVolume, fPitch, ePreset);
            break;
        }
    }

    public void StopSound(ESoundType eType, string strSoundName, float fFadeOut)
    {
#if UNITY_EDITOR
        if(!Application.isPlaying)
            return;
#endif        
        if(m_cSoundMgr == null || eType == ESoundType.None || eType == ESoundType.Effect)
            return;

        switch(eType)
        {
        case ESoundType.FadeInOut:
            m_cSoundMgr.FadeOut(strSoundName, fFadeOut);
            break;
        case ESoundType.Ambient:
        case ESoundType.AmbientLoop:
            m_cSoundMgr.StopAmbient();
            break;
        case ESoundType.BGM:
            m_cSoundMgr.StopBGM();
            break;
        }
    }
}
