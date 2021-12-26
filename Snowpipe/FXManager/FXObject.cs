using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectS
{
    public class FXObject : PoolingObject
    {
        [SerializeField]
        protected ParticleSystem        m_cParticle = null;
        [SerializeField]
        protected Animation             m_anim = null;
        [SerializeField]
        protected string                m_strAnimStart = null;
        [SerializeField]
        protected string                m_strAnimLoop = null;
        [SerializeField]
        protected string                m_strAnimEnd = null;

        [SerializeField]
        protected SpriteAnimation       m_spriteAnim = null;

        [Header("Return시 가장 먼저 없애야 할 것들")]
        [SerializeField]
        protected GameObject[]          m_arrObjOthers = null;

        protected ParticleSystem[]      m_arrChildParticles = null;

        protected Jun_TweenRuntime[]    m_arrTweens;

        public bool IsLoop { get; private set; } = false;

        protected override void Awake()
        {
            base.Awake();

            // 파티클 세팅
            if (m_cParticle == null)
            {
                m_cParticle = this.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
                if (m_cParticle == null)
                    m_cParticle = this.GetComponentInChildren(typeof(ParticleSystem)) as ParticleSystem;
            }

            // 애니메이션 세팅
            if (m_anim == null)
                m_anim = GetComponentInChildren(typeof(Animation), true) as Animation;

            if (m_anim != null && string.IsNullOrWhiteSpace(m_strAnimStart) && string.IsNullOrWhiteSpace(m_strAnimLoop))
            {
                if (m_anim.clip != null)
                {
                    if (m_anim.clip.isLooping)
                        m_strAnimLoop = m_anim.clip.name;
                    else
                        m_strAnimStart = m_anim.clip.name;
                }
            }

            // Tweener 세팅
            m_arrTweens = this.GetComponentsInChildren<Jun_TweenRuntime>(true);
            for (int i = 0; i < m_arrTweens.Length; ++i)
            {
                var tween = m_arrTweens[i];
                IsLoop |= tween.playType == Jun_TweenRuntime.PlayType.Loop;
                IsLoop |= tween.playType == Jun_TweenRuntime.PlayType.PingPong;
            }

            // 스프라이트 애니메이션 세팅
            if (m_spriteAnim == null)
                m_spriteAnim = GetComponent(typeof(SpriteAnimation)) as SpriteAnimation;

            // Loop 체크
            m_arrChildParticles = this.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < m_arrChildParticles.Length; ++i)
                IsLoop |= m_arrChildParticles[i].main.loop;

            if (!string.IsNullOrEmpty(m_strAnimLoop))
                IsLoop = true;

            if (m_spriteAnim != null)
            {
                IsLoop |= m_spriteAnim.wrapMode == SpriteAnimation.WrapMode.Loop;
                IsLoop |= m_spriteAnim.wrapMode == SpriteAnimation.WrapMode.PingPong;
            }

            if (!IsLoop)
            {
                if (m_cParticle == null 
                    && m_arrChildParticles.Length == 0 
                    && m_anim == null
                    && m_spriteAnim == null
                    && (m_arrTweens == null && m_arrTweens.Length == 0))
                {
                    IsLoop = true;
                }
            }
        }

        protected override void LateUpdate()
        {
            if (!bWaitForEnd)
            {
                if (!IsLoop)
                {
                    // 파티클이 살아 있다면
                    if (m_cParticle != null && m_cParticle.IsAlive(true))
                        return;

                    // 애니메이션이 살아 있다면?
                    if (m_anim != null && m_anim.isPlaying)
                        return;

                    // Sprite 애니메이션이 살아 있다면?
                    if (m_spriteAnim != null && m_spriteAnim.isPlaying)
                        return;

                    if (m_arrTweens != null)
                    {
                        for (int i = 0; i < m_arrTweens.Length; ++i)
                        {
                            if (m_arrTweens[i].isPlaying)
                                return;
                        }
                    }

                    ReturnToPool();
                }
                else if (IsLoop)
                {
                    // 시작 애니메이션이 살아 있다면?
                    if (m_anim != null && m_anim.clip != null && m_anim.clip.name == m_strAnimStart)
                    {
                        m_anim.clip = null;
                        m_anim.CrossFade(m_strAnimLoop, 0.15f);
                    }
                }
            }

            base.LateUpdate();
        }

        public override void ReturnToPoolForce()
        {
            if (m_cParticle != null)
                m_cParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (m_anim != null)
            {
                m_anim.clip = null;
                m_anim.Stop();
            }

            if (m_spriteAnim != null)
                m_spriteAnim.Stop();

            base.ReturnToPoolForce();
        }

        public override void ReturnToPool()
        {
            if (m_cParticle != null)
                m_cParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (m_anim != null)
            {
                if (string.IsNullOrEmpty(m_strAnimEnd))
                    m_anim.Stop();
                else
                    m_anim.CrossFade(m_strAnimEnd, 0.15f);
            }

            if (m_spriteAnim != null)
                m_spriteAnim.Stop();

            if (m_arrObjOthers != null)
            {
                for (int i = 0; i < m_arrObjOthers.Length; ++i)
                {
                    if (m_arrObjOthers[i] == null)
                        continue;

                    m_arrObjOthers[i].SetActive(false);
                }
            }

            base.ReturnToPool();
        }

        public override bool CheckWaitForEnd()
        {
            // 파티클이 살아 있다면
            if (m_cParticle != null && m_cParticle.IsAlive(true))
                return false;

            // 애니메이션이 살아 있다면?
            if (m_anim != null && m_anim.isPlaying)
                return false;

            // Sprite 애니메이션이 살아 있다면?
            if (m_spriteAnim != null && m_spriteAnim.isPlaying)
                return false;

            if (m_arrTweens != null)
            {
                for (int i = 0; i < m_arrTweens.Length; ++i)
                {
                    if (m_arrTweens[i].isPlaying)
                        return false;
                }    
            }

            return true;
        }

        public void PlayFX(float speedScale, float playTimeOffset)
        {
            if (m_cParticle != null)
            {
                m_cParticle.Play(true);
                m_cParticle.time = playTimeOffset;

                var main = m_cParticle.main;
                main.simulationSpeed = speedScale;
            }

            // 애니메이션 실행
            if (m_anim != null)
            {
                if(!string.IsNullOrEmpty(m_strAnimStart))
                {
                    m_anim.clip = m_anim.GetClip(m_strAnimStart);
                    m_anim.Play(m_strAnimStart);
                    m_anim[m_strAnimStart].time = playTimeOffset;
                    m_anim[m_strAnimStart].speed = speedScale;
                }
                else if (!string.IsNullOrEmpty(m_strAnimLoop))
                {
                    m_anim.clip = m_anim.GetClip(m_strAnimLoop);
                    m_anim.Play(m_strAnimLoop);
                    m_anim[m_strAnimLoop].time = playTimeOffset;
                    m_anim[m_strAnimLoop].speed = speedScale;
                }
                else if (m_anim.clip != null)
                {
                    m_anim.clip = null;
                }
            }

            // 트위너 실행
            if (m_arrTweens != null)
            {
                for (int i = 0; i < m_arrTweens.Length; ++i)
                {
                    var tween = m_arrTweens[i];
                    tween.Play();
                }
            }

            // 스프라이트 애니메이션 실행
            if (m_spriteAnim != null)
            {
                m_spriteAnim.Play();
                m_spriteAnim.time = playTimeOffset;
                m_spriteAnim.normalizedSpeed = speedScale;
            }

            if (m_arrObjOthers != null)
            {
                for (int i = 0; i < m_arrObjOthers.Length; ++i)
                {
                    if (m_arrObjOthers[i] == null)
                        continue;

                    m_arrObjOthers[i].SetActive(true);
                }
            }
        }

        public void SetSpeedScale(float speedScale)
        {
            if (m_anim != null && m_anim.clip != null)
                m_anim[m_anim.clip.name].speed = speedScale;

            for (int i = 0; i < m_arrChildParticles.Length; ++i)
            {
                var main = m_arrChildParticles[i].main;
                main.simulationSpeed = speedScale;
            }

            if (m_spriteAnim != null)
                m_spriteAnim.normalizedSpeed = speedScale;
        }
    }
}