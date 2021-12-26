using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Otome;
using DataLoadLib.Global;

namespace DataFileEnum
{
    public class CDataBase
    {
        public string TypeName { get; protected set; }

        protected const int REMOVE_COUNT_FOR_ASSETNAME = 14;
        protected static string FMT_FILE_NAME = "{0}_{1}";

        protected string GetZoneFileName(string strAssetName)
        {
            var eZoneType = Managers.Net?.CurrServerInfo?.zoneType ?? Proto.Bf.Type.ZoneType.ZoneKr;

            if(eZoneType == Proto.Bf.Type.ZoneType.None)
            {
#if DevClient
                Debug.LogFormat("<color=blue>Load Data : {0} </color>- ZoneType None", strAssetName);
#endif
                return strAssetName;
            }
            else
            {
#if DevClient
                Debug.LogFormat("<color=blue>Load Data : {0} </color>- {1}", string.Format(FMT_FILE_NAME, strAssetName, eZoneType), strAssetName, eZoneType);
#endif
                return string.Format(FMT_FILE_NAME, strAssetName, eZoneType);
            }
        }
    }

    public class CTableData<T> : CDataBase where T : CDataFileBase
    {
        private SortedDictionary<int, T> m_dicTableData = new SortedDictionary<int, T>();
        private Type m_cType = null;

        public CTableData()
        {
            m_cType = typeof(T);
            TypeName = m_cType.FullName.Remove(0, REMOVE_COUNT_FOR_ASSETNAME);
        }

        public SortedDictionary<int, T> GetDicData()
        {
            if(m_cType == null)
                return null;

            if(m_dicTableData == null || m_dicTableData.Count == 0)
                LoadDataFile();

            return m_dicTableData;
        }

        public T GetData(int nID)
        {
            if(m_cType == null)
                return null;

            if(m_dicTableData == null || m_dicTableData.Count == 0)
                LoadDataFile();

            if(m_dicTableData.ContainsKey(nID))
                return m_dicTableData[nID];

            return null;
        }

        private void LoadDataFile()
        {
            if(m_cType == null)
            {
                Debug.LogError("Type ERROR");
                return;
            }

            TextAsset asset = Managers.Resource.LoadAsset<TextAsset>(FrameworkConst.BundleName_GameData, GetZoneFileName(TypeName));
            if(asset == null)
            {
                Debug.LogError("Asset ERROR");
                return;
            }

            var listData = GameDataManager.GetTableData(asset);

            if(listData == null)
            {
                Debug.LogErrorFormat("{0} data NULL", TypeName);
                return;
            }

            if(m_dicTableData == null)
                m_dicTableData = new SortedDictionary<int, T>();
            else
                m_dicTableData.Clear();
            
            if(m_cType != null)
            {
                for(int i = 0 ; i < listData.Count ; ++i)
                {
                    TableInfo tableInfo = new TableInfo();
                    tableInfo.SetValue(listData[i]);

                    T cData = Activator.CreateInstance(m_cType, tableInfo) as T;
                    if(cData.IsZoneEventAvailable())
                        m_dicTableData.Add(listData[i][0].nValue, cData);
#if DevClient
                    else
                    {
                        Debug.LogFormat("<color=yellow>Not available SeasonNo</color><color=blue>TypeName : {0} ID : {1} SeasonNO : {2} CurZone SeasonNO : {3}</color>",
                            TypeName, cData.ID, Managers.Net?.CurrServerInfo?.curSeasonNo ?? -1, cData.GetZoneSeasonNO());
                    }
#endif
                }
            }
        }
    }

    public class CGroupDataBase : CDataBase
    {        
        protected string GetGroupFileName(int nGroupID)
        {
            string strFileName = string.Format(FMT_FILE_NAME, TypeName, nGroupID);

            return GetZoneFileName(strFileName);
        }
    }

    public class CGroupData<T> : CGroupDataBase where T : CDataFileBase
    {
        private Dictionary<int, List<T>> m_dicGroupData = new Dictionary<int, List<T>>();
        private Type m_cType = null;
        
        public CGroupData()
        {
            m_cType = typeof(T);
            TypeName = m_cType.FullName.Remove(0, REMOVE_COUNT_FOR_ASSETNAME);
        }

        public List<T> GetData(int nGroupID)
        {
            if(m_cType == null)
                return null;

            if(m_dicGroupData.ContainsKey(nGroupID) && m_dicGroupData[nGroupID] != null)
                return m_dicGroupData[nGroupID];

            LoadDataFile(nGroupID);
            if(m_dicGroupData.ContainsKey(nGroupID))
                return m_dicGroupData[nGroupID];

            return null;
        }

        private void LoadDataFile(int nGroupID)
        {
            if(m_cType == null)
            {
                Debug.LogError("Type ERROR");
                return;
            }

            TextAsset asset = Managers.Resource.LoadAsset<TextAsset>(FrameworkConst.BundleName_GameData,
                GetGroupFileName(nGroupID));

            if(asset == null)
            {
                Debug.LogError("Asset ERROR : " + TypeName);
                return;
            }

            var listData = GameDataManager.GetTableData(asset);

            if(listData == null)
            {
                Debug.LogErrorFormat("{0} data GroupID {1} NULL", "AbilityNodeData", nGroupID);
                return;
            }

            if(m_dicGroupData.ContainsKey(nGroupID))
                m_dicGroupData[nGroupID].Clear();
            else
                m_dicGroupData.Add(nGroupID, new List<T>());

            for(int i = 0 ; i < listData.Count ; ++i)
            {
                TableInfo tableInfo = new TableInfo();
                tableInfo.SetValue(listData[i]);

                T cData = Activator.CreateInstance(m_cType, tableInfo) as T;
                if(cData.IsZoneEventAvailable())
                    m_dicGroupData[nGroupID].Add(cData);
#if DevClient
                else
                {
                    Debug.LogFormat("<color=blue>Not available Zone Load Data</color> TypeName-{0} ID-{1} EventValue-{2} ZoneValue-{3}",
                        TypeName, cData.ID, Managers.Net?.CurrServerInfo?.curSeasonNo ?? -1, cData.GetZoneSeasonNO());
                }
#endif
            }
        }
    }
}