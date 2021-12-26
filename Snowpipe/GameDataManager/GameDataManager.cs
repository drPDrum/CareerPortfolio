using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataFileEnum;
using DataLoadLib;
using DataLoadLib.Global;

namespace ProjectS
{
    public partial class GameDataManager : ManagerBase
    {
        #region Fields        
        private Dictionary<string, CDataBase> m_dicDataFiles = new Dictionary<string, CDataBase>();
        private Dictionary<string, CGroupDataBase> m_dicGroupData = new Dictionary<string, CGroupDataBase>();

        #endregion Fields

        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            if (m_dicDataFiles == null)
                m_dicDataFiles = new Dictionary<string, CDataBase>();
            else
                m_dicDataFiles.Clear();

            //LoadConstData();            
            yield break;
        }

        public static List<DataInfo[]> GetTableData(TextAsset asset, bool bEncfile = true)
        {
            if (asset == null)
                return null;

            try
            {
                using (Stream stream = new MemoryStream(asset.bytes))
                {
                    List<DataInfo[]> listData = null;
                    if (bEncfile)
                        DataLoadClass.DataLoadDecryptor(stream, out listData, CConst.EncKeyData);
                    else
                        DataLoadClass.DataLoadOriginal(stream, out listData);

                    return listData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("{0} LoadDataFile fail ex - {1}\nKey : [{2}]", asset.name, e.Message, CConst.EncKeyData);
            }

            return null;
        }



        public static void LoadConstData()
        {
            TextAsset asset = Managers.Asset.LoadAsset<TextAsset>(string.Format(CConst.PATH_DATA, "ConstData"), true);

            if (asset == null)
            {
                Debug.LogError("Asset ERROR : ConstData");
                return;
            }

            var listData = GameDataManager.GetTableData(asset);

            if (listData == null)
            {
                Debug.LogErrorFormat("Table Parsing ERROR : ConstData");
                return;
            }

            var arrInt = listData.Select((x) => x[2].nValue).ToArray();
            var arrStr = listData.Select((x) => x[3].strValue).ToArray();

            CConstData.SetInfo(arrInt, arrStr);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 그룹 데이터는 사용하지 말것.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SortedDictionary<int, T> GetDicDataInEditor<T>() where T : CDataFileBase
        {
            var cType = typeof(T);

            if (cType is null)
                return null;

            var strType = cType.FullName.Remove(0, CDataBase.REMOVE_COUNT_FOR_ASSETNAME);

            var strFileFullPath = string.Format(CConst.PATH_DATA, strType);
            var path = $"Assets/_DownloadableAssets/{strFileFullPath}.bytes";
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (asset is null)
                return null;

            var listData = GameDataManager.GetTableData(asset);
            if (listData is null)
            {
                Debug.LogErrorFormat("{0} data NULL", strType);
                return null;
            }

            var dicData = new SortedDictionary<int, T>();

            for (int i = 0 ; i < listData.Count ; ++i)
            {
                TableInfo tableInfo = new TableInfo();
                tableInfo.SetValue(listData[i]);

                T cData = Activator.CreateInstance(cType, tableInfo) as T;
                dicData.Add(listData[i][0].nValue, cData);
            }

            return dicData;
        }

        /// <summary>
        /// 그룹 데이터 전용.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetGroupDataListInEditor<T>(int nGroupID) where T : CDataFileBase
        {
            var cType = typeof(T);
            if (cType is null)
                return null;

            var strType = cType.FullName.Remove(0, CDataBase.REMOVE_COUNT_FOR_ASSETNAME);

            var strFileFullPath = string.Format(CConst.PATH_GROUPDATA, strType, nGroupID);
            var path = $"Assets/_DownloadableAssets/{strFileFullPath}.bytes";
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (asset is null)
                return null;

            var listData = GameDataManager.GetTableData(asset);

            if (listData is null)
            {
                Debug.LogErrorFormat("{0} data GroupID {1} NULL", strType, nGroupID);
                return null;
            }

            var listRet = new List<T>();

            for (int i = 0 ; i < listData.Count ; ++i)
            {
                TableInfo tableInfo = new TableInfo();
                tableInfo.SetValue(listData[i]);

                T cData = Activator.CreateInstance(cType, tableInfo) as T;
                listRet.Add(cData);
            }

            return listRet;
        }
#endif //UNITY_EDITOR
    }
}
