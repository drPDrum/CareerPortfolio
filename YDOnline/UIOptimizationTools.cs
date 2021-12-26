#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DrumTool : MonoBehaviour 
{
    private const string 	EXPORT_LOCATION = "../LogFiles/";    
    private const string 	EMPTY_KEY = "Empty";

	private const string    ASSET_ROOT = "/Resources/";
	private const string    PREFAB_ROOT = "UI";
	private const string    FONT_1 = "notosans_m";
	private const string    FONT_2 = "FjallaOne-Regular";
	private const string    ATLAS_MAIN = "main";
	
	private const int       FONT_1_DEPTH = 800;
	private const int       FONT_2_DEPTH = 830;
    
    private static Dictionary<string, List<string>> m_dicUseSpriteList = new Dictionary<string, List<string>>();
    private static Dictionary<string, UIAtlas>      m_dicAtlasList = new Dictionary<string, UIAtlas>();

    [MenuItem("Utils/UI Optimization/Selected Prefabs Information Log", false)]
    static void SelectedInformation()
    {
        GameObject go = Selection.activeGameObject;
        Transform t = go.transform;

        string strLog = "[Selected Atlas and Prefabs Information Log]\n\n";
        strLog += "Path\tObject Name\tActive State\tType\tAtlas(Font) Name\tImage Name(Initial Text)\tDepth\t비고";
        
        m_dicUseSpriteList.Clear();
        m_dicAtlasList.Clear();

        AddPrefabLogRec(ref strLog, t);        
        AddUsingAtlasInformation(ref strLog);
        
        m_dicUseSpriteList.Clear();
        m_dicAtlasList.Clear();
        
//        Debug.Log(strLog);   
        WriteLogFile("SelectedPrefab", strLog);
        Debug.Log("Selected Atlas and Prefabs Information Log End");
    }

    [MenuItem("Utils/UI Optimization/Set Label Depth", false)]
    static void SetLabelDepth()
    {
        GameObject go = Selection.activeGameObject;
        
        SetLabelDepthRec(go.transform);


        Debug.Log("SetLabelDepth End");
    }

    [MenuItem("Utils/UI Optimization/Set Main Depth Over 10", false)]
    static void SetMainDepthAdd()
    {
        GameObject go = Selection.activeGameObject;

        if(CheckMainDepthRec(go.transform))
        {
            Debug.Log("No need to use this tool.");
        }
        else
        {
            SetMainDepthOverRec(go.transform);
            Debug.Log("Set Main Depth Over 10 END");
        }
    }

    [MenuItem("Utils/UI Optimization/GetAllPrefabName", false)]
    static void GetAllPrefabName()
    {
        string strDirRoot = string.Format("{0}{1}{2}", Application.dataPath, ASSET_ROOT, PREFAB_ROOT);
        string strLog = "[All Prefabs Name Log]\n";

        m_dicUseSpriteList.Clear();
        m_dicAtlasList.Clear();

        GetResourceName(ref strLog, strDirRoot);
        AddNotUsedAtlasInformation(ref strLog);

        WriteLogFile("PrefabName", strLog);

        m_dicUseSpriteList.Clear();
        m_dicAtlasList.Clear();
    }

    [MenuItem("Utils/UI Optimization/GetAllPrefabInfo..need huge loading", false)]
    static void GetAllPrefabInfo()
    {
        string strDirRoot = string.Format("{0}{1}{2}", Application.dataPath, ASSET_ROOT, PREFAB_ROOT);
        string strLog = "[All Prefabs Infomation Log]\n";

        m_dicUseSpriteList.Clear();
        m_dicAtlasList.Clear();

        SearchResourceByType(ref strLog, strDirRoot, "Prefab");
        AddNotUsedAtlasInformation(ref strLog);

        WriteLogFile("Prefabs", strLog);

        m_dicUseSpriteList.Clear();
        m_dicAtlasList.Clear();
    }

    static void AddPrefabLogRec(ref string strLog, Transform t, string strHierachy = "/")
    {       
        UISprite spr = t.GetComponent<UISprite>();
        UILabel lab = t.GetComponent<UILabel>();
        UITexture tex = t.GetComponent<UITexture>();

        GameObject obj = t.gameObject;
        strHierachy += obj.name + "/";
        strLog += "\n" + strHierachy + "\t" + obj.name;
        if(obj.activeSelf)
            strLog += "\tActive";
        else
            strLog += "\tInactive";

        if(spr != null)
        {
            strLog += "\tSprite";
            if(spr.atlas == null)
                strLog += "\tAtlas NULL";
            else
            {
                strLog += "\t" + spr.atlas.name;
                
                if(spr.GetAtlasSprite() == null)
                {
                    if(!m_dicUseSpriteList.ContainsKey(EMPTY_KEY))
                    {
                        List<string> list = new List<string>();            
                        list.Add(spr.spriteName);
                        
                        m_dicUseSpriteList.Add(EMPTY_KEY, list);
                    }
                    else
                    {
                        if(!m_dicUseSpriteList[EMPTY_KEY].Contains(spr.spriteName))
                        {
                            m_dicUseSpriteList[EMPTY_KEY].Add(spr.spriteName);
                        }
                    }
                }
                else
                {
                    if(!m_dicUseSpriteList.ContainsKey(spr.atlas.name))
                    {
                        List<string> list = new List<string>();                        
                        list.Add(spr.spriteName);
                        
                        m_dicUseSpriteList.Add(spr.atlas.name, list);
                        
                        m_dicAtlasList.Add(spr.atlas.name, spr.atlas);
                    }
                    else
                    {
                        if(!m_dicUseSpriteList[spr.atlas.name].Contains(spr.spriteName))
                        {
                            m_dicUseSpriteList[spr.atlas.name].Add(spr.spriteName);
                        }
                    }
                }
            }
            strLog += "\t" + spr.spriteName;
            strLog += "\t" + spr.depth.ToString();

            if(spr.GetAtlasSprite() == null)
            {
                strLog += "\tEmpty Sprite.";
            }
        }
        else if(lab != null)
        {
            strLog += "\tFont";
            if(lab.ambigiousFont != null)
                strLog += "\t" + lab.ambigiousFont.name;
            else if(lab.bitmapFont != null)
                strLog += "\t" + lab.bitmapFont.name;
            else if(lab.trueTypeFont != null)
                strLog += "\t" + lab.trueTypeFont.name;
            else
                strLog += "\t";

            strLog += "\t\t" + lab.depth.ToString();
//            strLog += "\t" + lab.text;
        }
        else if(tex != null)
        {
            strLog += "\tTexture\t"; 
            if(tex.mainTexture == null)
                strLog += "\tTexture NULL";
            else
                strLog += "\t" + tex.mainTexture.name;
            strLog += "\t" + tex.depth.ToString();
        }
        else
        {
            UIWidget wid = t.GetComponent<UIWidget>();
            if(wid)
            {
                strLog += "\tWidget\t\t";
                strLog += "\t" + wid.depth.ToString();
            }
            else
            {
                strLog += "\tEmpty Object";
            }
        }

        if(t.childCount > 0)
        {
            for(int i = 0 ; i < t.childCount ; ++i)
            {
                AddPrefabLogRec(ref strLog, t.GetChild(i), strHierachy);
            }
        }
    }
    
    static void AddUsingAtlasInformation(ref string strLog)
    {
        strLog += "\n\n[Using Atlas Information]\n\n";
        foreach(string strKey in m_dicUseSpriteList.Keys)
        {
            List<string> listSprite = m_dicUseSpriteList[strKey];
            strLog += strKey + " Atlas Information\n";
            for(int i = 0 ; i < listSprite.Count ; ++i)
            {
                strLog += "\t" + listSprite[i];
            }
            strLog += "\n";
        }
    }
        
    static void WriteLogFile(string strType, string strLog)
    {
        string strFile = string.Format(
            "{0}{1}/{2}_{3}_{4}_{5}_{6}_{7}_{8}.txt", 
            EXPORT_LOCATION, 
            strType, strType,
            System.DateTime.Now.Year, 
            System.DateTime.Now.Month, 
            System.DateTime.Now.Day, 
            System.DateTime.Now.Hour, 
            System.DateTime.Now.Minute, 
            System.DateTime.Now.Second);
        
        string strPath = Path.GetDirectoryName(strFile);
        if(!Directory.Exists(strPath))
        {
            Directory.CreateDirectory(strPath);
        }
        
        if(File.Exists(strFile))
        {
            using (FileStream stream = new FileStream(strFile, FileMode.Append))
            {
                using (StreamWriter bw = new StreamWriter(stream))
                {
                    bw.WriteLine(strLog);
                }
            }
        }
        else
        {
            using (FileStream stream = new FileStream(strFile, FileMode.CreateNew))
            {
                using (StreamWriter bw = new StreamWriter(stream))
                {
                    bw.WriteLine(strLog);
                }
            }
        }
        
        strFile = strFile.Replace(@"/", @"\");
        
#if UNITY_IOS
        System.Diagnostics.Process.Start("open", strFile);
#else
        if(File.Exists(strFile))
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + strFile);
        }
        else
        {
            System.Diagnostics.Process.Start("explorer.exe", strFile);
        }
#endif
    }    

    static void SetLabelDepthRec(Transform t)
    {
        UILabel lab = t.GetComponent<UILabel>();
        
        if(lab != null)
        {
            var font = lab.bitmapFont;
            if(font != null)
            {
                switch(font.name)
                {
                case FONT_1:
                    Debug.LogFormat("Object[{0}] Depth change from {0} to {1} : {2}", t.name, lab.depth, FONT_1_DEPTH, FONT_1);
                    lab.depth = FONT_1_DEPTH;
                    break;
                case FONT_2:
                    Debug.LogFormat("Object[{0}] Depth change from {0} to {1} : {2}", t.name, lab.depth, FONT_2_DEPTH, FONT_2);
                    lab.depth = FONT_2_DEPTH;
                    break;
                default:
                    Debug.LogError("Add " + lab.bitmapFont.name + " Font to TOOL");
                    break;
                }
            }
        }

        if(t.childCount > 0)
        {
            for(int i = 0 ; i < t.childCount ; ++i)
            {
                SetLabelDepthRec(t.GetChild(i));
            }
        }
    }

    static bool CheckMainDepthRec(Transform t)
    {
        UISprite spr = t.GetComponent<UISprite>();

        if(spr != null && spr.atlas != null)
        {
            var strAtlas = spr.atlas.name;

            if(string.Compare(strAtlas, ATLAS_MAIN) == 0)
            {
                if(spr.depth < 10)
                {
                    Debug.Log("Need to Use this tool.");
                    return false;
                }
            }
        }

        if(t.childCount > 0)
        {
            for(int i = 0 ; i < t.childCount ; ++i)
            {
                if(!CheckMainDepthRec(t.GetChild(i)))
                    return false;
            }
        }

        return true;
    }

    static void SetMainDepthOverRec(Transform t)
    {
        UISprite spr = t.GetComponent<UISprite>();

        if(spr != null && spr.atlas != null)
        {
            var strAtlas = spr.atlas.name;

            if(string.Compare(strAtlas, ATLAS_MAIN) == 0)
            {
                Debug.LogFormat("[{0}] changed from[{1}] to [{2}]", spr.name, spr.depth, spr.depth + 10);
                spr.depth += 10;
            }
        }

        if(t.childCount > 0)
        {
            for(int i = 0 ; i < t.childCount ; ++i)
            {
                SetMainDepthOverRec(t.GetChild(i));
            }
        }
    }

    static void SearchResourceByType(ref string strLog, string strRoot, string strType)
    {

        string[] allFilesList = Directory.GetFiles(strRoot, "*", SearchOption.AllDirectories);
        
        for(int i = 0 ; i < allFilesList.Length ; ++i)
        {
            if(!allFilesList[i].Contains(".meta") && allFilesList[i].Contains(".prefab"))
            {
                string[] paths = allFilesList[i].Split('\\');
                if(paths.Length < 2)
                {
                    Debug.LogError("Path Length Error.");
                    return;
                }
                string[] arrPaths = allFilesList[i].Split('/');
                string strPath = arrPaths[arrPaths.Length - 1].Replace('\\', '/');
                string[] arrNameSplit = strPath.Split('/');

                strLog += "\n[" + strType + " Name : " + arrNameSplit[arrNameSplit.Length - 1] + "]\n\n";

                string strFilePath = string.Format("Assets{0}{1}", ASSET_ROOT, strPath);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(strFilePath);
                if(obj == null)
                {
                    strLog += "\tError : Object Load Asset Error.";
                    continue;
                }
                switch(strType)
                {
                case "Atlas":
                    UIAtlas atlas = obj.GetComponent<UIAtlas>();
                    if(atlas == null)
                    {
                        strLog += "\tError : Atlas Script call Error.";
                        continue;
                    }
                    AddAtlasLog(ref strLog, atlas);
                    break;
                case "Prefab":

                    strLog += "Path\tObject Name\tActive State\tType\tAtlas(Font) Name\tImage Name\tDepth\t비고";

                    Transform objTrans = obj.transform;
                    if(objTrans == null)
                    {
                        strLog += "\tError : Transform Error.";
                    }
                    AddPrefabLogRec(ref strLog, obj.transform);
                    break;
                }
                strLog += "\n";
            }
        }
    }

    static void GetResourceName(ref string strLog, string strRoot)
    {

        string[] allFilesList = Directory.GetFiles(strRoot, "*", SearchOption.AllDirectories);

        strLog += "\nName\tPath\t비고\n";
        for(int i = 0 ; i < allFilesList.Length ; ++i)
        {
            if(!allFilesList[i].Contains(".meta") && allFilesList[i].Contains(".prefab"))
            {
                string[] paths = allFilesList[i].Split('\\');
                if(paths.Length < 2)
                {
                    Debug.LogError("Path Length Error.");
                    return;
                }

                string[] arrPaths = allFilesList[i].Split('/');
                string strPath = arrPaths[arrPaths.Length - 1].Replace('\\', '/');

                string[] arrNameSplit = strPath.Split('/');

                strLog += arrNameSplit[arrNameSplit.Length - 1] + "\t" + strPath + "\n";
            }
        }
    }

    static void AddNotUsedAtlasInformation(ref string strLog)
    {
        strLog += "\n\n[Not Used Atlas Information]\n\n";
        foreach(string strKey in m_dicUseSpriteList.Keys)
        {
            if(m_dicAtlasList.ContainsKey(strKey))
            {
                UIAtlas atlas = m_dicAtlasList[strKey];
                if(atlas == null)
                {
                    Debug.LogError("Atlas is Empty");
                    return;
                }
                BetterList<string> listInAtlas = atlas.GetListOfSprites();

                if(listInAtlas == null)
                {
                    Debug.LogError("Sprite List is Empty : " + atlas.name);
                    return;
                }

                List<string> listUsed = m_dicUseSpriteList[strKey];
                strLog += "[Not Use Image in " + atlas.name + "]\n";

                foreach(var atlasData in listInAtlas)
                {
                    strLog += atlasData + '\t';
                }

                strLog += '\n';
            }
        }
    }

    static void AddAtlasLog(ref string strLog, UIAtlas atlas)
    {
        if(atlas == null)
        {
            return;
        }

        for(int i = 0 ; i < atlas.spriteList.Count ; ++i)
        {
            UISpriteData data = atlas.spriteList[i];
            if(data == null)
            {
                strLog += "Data Error.";
            }
            else
            {
                strLog += data.name;
                strLog += '\t' + data.x.ToString();
                strLog += '\t' + data.y.ToString();
                strLog += '\t' + data.width.ToString();
                strLog += '\t' + data.height.ToString();
                strLog += '\t' + data.borderLeft.ToString();
                strLog += '\t' + data.borderRight.ToString();
                strLog += '\t' + data.borderBottom.ToString();
                strLog += '\t' + data.borderTop.ToString();
                strLog += '\t' + data.paddingLeft.ToString();
                strLog += '\t' + data.paddingRight.ToString();
                strLog += '\t' + data.paddingBottom.ToString();
                strLog += '\t' + data.paddingTop.ToString();
                strLog += '\n';
            }
        }
    }
}

#endif