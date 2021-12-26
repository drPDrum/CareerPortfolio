using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace ProjectS
{
    public partial class Managers
    {
        #region Manager Get Properties
        public static SystemConfig SystemConfig { get { return Instance == null ? null : Instance.m_cSysConfig; } }
        public static LocalDataManager LocalData { get { return Instance == null ? null : Instance.m_cLocalDataMgr; } }
        public static LocalizationManager LZ { get { return Instance == null ? null : Instance.m_cLocalizationMgr; } }
        public static NetworkManager Net { get { return Instance == null ? null : Instance.m_cNetMgr; } }
        public static AssetManager Asset { get { return Instance == null ? null : Instance.m_cAssetMgr; } }
        public static ObjectPoolManager Pool { get { return Instance == null ? null : Instance.m_cPoolMgr; } }
        public static SoundManager Sound { get { return Instance == null ? null : Instance.m_cSoundMgr; } }
        public static SceneManager Scene { get { return Instance == null ? null : Instance.m_cSceneMgr; } }
        public static WorldManager World { get { return Instance == null ? null : Instance.m_cWorldMgr; } }
        public static QuestManager Quest { get { return Instance == null ? null : Instance.m_cQuestMgr; } }
        public static WindowManager Window { get { return Instance == null ? null : Instance.m_cWinMgr; } }        
        public static GameDataManager GameData { get { return Instance == null ? null : Instance.m_cGameDataMgr; } }
        public static SFXManager SFX { get { return Instance == null ? null : Instance.m_cSFXMgr; } }        
        public static FXManager FX { get { return Instance == null ? null : Instance.m_cFXMgr; } }
        #endregion Manager Get Properties

        #region Properties
        public static bool IsValid => m_instance != null && m_instance.State == EInitState.End;

        public static UnityEvent<EInitState> OnStateChange { get; } = new UnityEvent<EInitState>();

        #endregion Properties

        #region Methods
        public static void Release()
        {
            if (m_instance == null)
                return;
            Pool.Clear(false);

            LZ.Clear();
            Sound.Clear();
            GameData.Clear();
            Asset.Clear();
            World.Clear();
            SecurePlayerPrefs.Init();

            Resources.UnloadUnusedAssets();
            GameObject.Destroy(m_instance.gameObject);
            
            System.GC.Collect();
        }

        public static void RestartGame()
        {
            Debug.LogFormat("<color=red>Call GameRestart</color>");

            if(Scene != null && Scene.CurSceneType == ESceneType.Logo)
            {
                Managers.Scene.LoadScene(CConst.SCENE_NAME_LOADING, bAsyncLoad: false, onComplete: () =>
                {
                    Managers.RestartGame();
                });
            }
            else
            {
                Release();
                UserInfo.Clear();
                UnityEngine.SceneManagement.SceneManager.LoadScene(CConst.SCENE_NAME_LOGO);
            }
        }
        #endregion Methods

        
    }
}
