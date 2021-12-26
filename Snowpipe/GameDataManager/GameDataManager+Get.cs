using System.Collections.Generic;
using UnityEngine;
using DataFileEnum;
using System;

namespace ProjectS
{
    public partial class GameDataManager
    {
        public T GetData<T>(int nID) where T : CDataFileBase
        {
            var strType = typeof(T).FullName;
            if (string.IsNullOrEmpty(strType))
            {
                Debug.LogError("Type NULL ERROR");
                return null;
            }

            if (m_dicDataFiles.ContainsKey(strType))
            {
                var cData = m_dicDataFiles[strType] as CTableData<T>;
                try
                {
                    return cData.GetData(nID);
                }
                catch (Exception e)
                {
#if DevClient
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
#endif
                    return null;
                }
            }
            else
            {
                var cData = new CTableData<T>();
                m_dicDataFiles.Add(strType, cData);

                try
                {
                    return cData.GetData(nID);
                }
                catch (Exception e)
                {
#if DevClient
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
#endif
                    return null;
                }
            }
        }
        public SortedDictionary<int, T> GetDicData<T>() where T : CDataFileBase
        {
            var strType = typeof(T).FullName;
            if (string.IsNullOrEmpty(strType))
            {
#if DevClient
                Debug.LogError("Type Null ERROR");
#endif
                return null;
            }

            if (m_dicDataFiles.ContainsKey(strType))
            {
                var cData = m_dicDataFiles[strType] as CTableData<T>;
                try
                {
                    return cData.GetDicData();
                }
                catch (Exception e)
                {
#if DevClient
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
#endif
                    return null;
                }
            }
            else
            {
                var cData = new CTableData<T>();
                m_dicDataFiles.Add(strType, cData);

                try
                {
                    return cData.GetDicData();
                }
                catch (Exception e)
                {
#if DevClient
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
#endif
                    return null;
                }
            }
        }

        public List<T> GetGroupDataList<T>(int nGroupID) where T : CDataFileBase
        {
            if (nGroupID <= 0)
                return null;

            string strKey = typeof(T).FullName;

            if (m_dicGroupData.ContainsKey(strKey))
            {
                if (m_dicGroupData[strKey] is CGroupData<T>)
                {
                    var cData = m_dicGroupData[strKey] as CGroupData<T>;

                    return cData.GetData(nGroupID);
                }
                else
                {
                    Debug.LogError("Group Data Set ERROR : " + strKey);
                    return null;
                }
            }
            else
            {
                var cData = new CGroupData<T>();

                m_dicGroupData.Add(strKey, cData);

                return cData.GetData(nGroupID);
            }
        }
    }
}