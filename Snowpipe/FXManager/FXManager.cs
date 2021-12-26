using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectS
{
    public class FXManager : ManagerBase
    {
        public readonly Dictionary<string, int> m_dicFxRefCounts = new Dictionary<string, int>(); 
        public readonly Dictionary<string, PoolBase> m_dicFxPools = new Dictionary<string, PoolBase>();

        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            yield break;
        }

        public bool IsRegistedFX(string strFXPrefName)
        {
            if (string.IsNullOrEmpty(strFXPrefName))
                return false;

            return m_dicFxRefCounts.ContainsKey(strFXPrefName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefFX">주의! 모든 이펙트들은 서로 다른 이름을 가지고 있어야 합니다.</param>
        /// <param name="count"></param>
        public void RegistFX(GameObject prefFX, int nInitCount = 1)
        {
            RegistFX<FXObject>(prefFX, nInitCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefFX">주의! 모든 이펙트들은 서로 다른 이름을 가지고 있어야 합니다.</param>
        /// <param name="count"></param>
        public void RegistFX<T>(GameObject prefFX, int nInitCount = 1) where T : FXObject
        {
            if (prefFX == null)
                return;

            prefFX.GetOrAddComponent<T>();
            if (m_dicFxRefCounts.ContainsKey(prefFX.name))
            {
                ++m_dicFxRefCounts[prefFX.name];
            }
            else
            {
                m_dicFxRefCounts.Add(prefFX.name, nInitCount);

                PoolBase cPool = null;

                if (m_dicFxPools.TryGetValue(prefFX.name, out cPool))
                {
                    if (cPool == null)
                        m_dicFxPools.Remove(prefFX.name);
                }

                if (cPool == null)
                {
                    cPool = PoolBase.Create(string.Format("Pool_{0}", prefFX.name), this.transform);
                    m_dicFxPools.Add(prefFX.name, cPool);
                }

                cPool.Init(prefFX, nInitCount);
            }
        }

        public int GetRegistCount(string strFXPrefName)
        {
            int result = 0;
            if (m_dicFxRefCounts.TryGetValue(strFXPrefName, out result))
                return result;

            return 0;
        }

        public void RegistAdd(string strFXPrefName)
        {
            if (m_dicFxRefCounts.ContainsKey(strFXPrefName))
                ++m_dicFxRefCounts[strFXPrefName];
        }

        public void RemoveFX(string strFXPrefName)
        {
            if (string.IsNullOrEmpty(strFXPrefName))
                return;

            if (!m_dicFxRefCounts.ContainsKey(strFXPrefName))
                return;

            if (--m_dicFxRefCounts[strFXPrefName] > 0)
                return;

            m_dicFxRefCounts.Remove(strFXPrefName);

            PoolBase fxPool = null;
            if (!m_dicFxPools.TryGetValue(strFXPrefName, out fxPool))
                return;

            if (fxPool != null)
                GameObject.Destroy(fxPool.gameObject);

            m_dicFxPools.Remove(strFXPrefName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFXPrefName">프리펩 이름</param>
        /// <param name="vLocalPos">로컬 좌표, tr_Parent == null이면 월드 좌표</param>
        /// <param name="qLocalRot">로컬 회전, tr_Parent == null이면 월드 회전</param>
        /// <param name="vLocalScale">로컬 스케일, tr_Parent == null이면 월드 스케일</param>
        /// <param name="transParent">부모 Transform</param>
        /// <param name="fPlaySpeedScale">재생 속도</param>
        /// <param name="fPlayTimeOffset">빨리감기 Time</param>
        /// <returns></returns>
        public FXObject PlayFX(
            string strFXPrefName,
            Vector3 vLocalPos,
            Quaternion qLocalRot,
            Vector3 vLocalScale,
            Transform transParent,
            float fPlaySpeedScale,
            float fPlayTimeOffset,
            PoolBase.PopOptionForNotEnough ePopType = PoolBase.PopOptionForNotEnough.Instantiate)
        {
            if (string.IsNullOrEmpty(strFXPrefName))
                return null;

            PoolBase fxPool = null;
            m_dicFxPools.TryGetValue(strFXPrefName, out fxPool);

            if (fxPool == null)
                return null;

            var fx = fxPool.Pop(ePopType) as FXObject;

            fx.GenerateObject(vLocalPos,qLocalRot, vLocalScale, transParent);
            fx.PlayFX(fPlaySpeedScale,fPlayTimeOffset);

            return fx;
        }

        public FXObject PlayFX(
            string strFXPrefName,
            Transform transParent,
            float fPlaySpeedScale,
            float fPlayTimeOffset,
            PoolBase.PopOptionForNotEnough ePopType = PoolBase.PopOptionForNotEnough.Instantiate)
        {
            return PlayFX(strFXPrefName, Vector3.zero, Quaternion.identity, Vector3.one, transParent, fPlaySpeedScale, fPlayTimeOffset, ePopType);
        }


        public void RetrieveItems(string strFXPrefName)
        {
            var pool = m_dicFxPools.GetOrNull(strFXPrefName);
            if (pool == null)
                return;

            pool.RetrieveAllItems();
        }

        public void RetrieveAllItems()
        {
#if DevClient
            Debug.Log("FXManager->RetrieveAllItems");
#endif
            foreach(var pair in m_dicFxPools)
                pair.Value.RetrieveAllItems();
        }
    }
}