using UnityEngine;
using System;
using System.Collections;
using Utils;
////using GameState;

namespace ProjectS
{
    public partial class Managers : MonoSingletonInitializer<Managers>, IAsyncInitializer
    {
        #region Manager instance
        private     SystemConfig            m_cSysConfig;        
        private     NetworkManager          m_cNetMgr = null;
        private     AssetManager            m_cAssetMgr = null;
        private     ObjectPoolManager       m_cPoolMgr = null;
        private     SoundManager            m_cSoundMgr = null;
        private     SceneManager            m_cSceneMgr = null;
        private     LocalDataManager        m_cLocalDataMgr = null;
        private     WorldManager            m_cWorldMgr = null;
        private     QuestManager            m_cQuestMgr = null;
        private     WindowManager           m_cWinMgr = null;
        private     SFXManager              m_cSFXMgr = null;
        private     FXManager               m_cFXMgr = null;
        private     LocalizationManager     m_cLocalizationMgr = null;
        private     GameDataManager         m_cGameDataMgr = null;

        #endregion Manager instance

        public ScreenFader Fader { get; private set; }

        public EInitState State { get; private set; }

        #region Unity Event
#if UNITY_EDITOR
        private void Start()
        {                
            Debug.Log("Managers Start()");

            if (IsStartInitializer)
                return;

            if(Initializer == null)
                RunInitialize();
            else
                StartCoroutine(Initializer);
        }

        private void Update()
        {
            if (Camera.allCamerasCount == 0)
                Debug.LogError("Cam is Empty");

            if (Input.GetKeyDown(KeyCode.Q)
                && Input.GetKey(KeyCode.LeftControl))
                RestartGame();

        }
#endif //UNITY_EDITOR

        void OnApplicationQuit()
        {
            // Reset Notification
            UnRegisterNotification();

            // Set Notification
            RegisterAllNotification();
        }

        #endregion Unity Event

        #region Implement IInitializer
        public IEnumerator Initializer { get; private set; }
        public bool IsStartInitializer { get { return State != EInitState.None; } }
        public bool IsInitialized { get { return State == EInitState.End; } }

        public void RunInitialize()
        {
            Debug.Log("Initialize() : " + State);

            if (State != EInitState.None)
                return;

            Initializer = null;

            var obj = this.gameObject;
            if (obj != null && obj.activeSelf)
            {
                if (!IsStartInitializer)
                {
                    Debug.Log("Start Initializer Coroutine");
                    StartCoroutine(Initialize(null));
                }
            }
            else
            {
                Initializer = null;
                Initializer = Initialize(null);
                Debug.Log("Cache Initializer");
            }
        }

        public IEnumerator Initialize(Action onComplete = null)
        {
            Debug.Log("Init_Async");
            if (IsStartInitializer || IsInitialized)
                yield break;

            yield return SetState(EInitState.Start);

            SetBasicSetting();

            // SystemConfig
            m_cSysConfig = Resources.Load<SystemConfig>("System/SystemConfig");

            // LocalData
            yield return SetState(EInitState.LocalData);
            m_cLocalDataMgr = CreateManager<LocalDataManager>();
            yield return m_cLocalDataMgr.Initialize();
            yield return SetState(EInitState.LocalData_End);
            
            RefreshBasicLocalData();

            // Network
            yield return SetState(EInitState.Network);
            m_cNetMgr = CreateManager<NetworkManager>();
            yield return m_cNetMgr.Initialize();
            yield return SetState(EInitState.Network_End);

            // Asset
            yield return SetState(EInitState.Asset);
            m_cAssetMgr = CreateManager<AssetManager>();
            yield return m_cAssetMgr.Initialize();
            yield return SetState(EInitState.Asset_End);

            // Data
            yield return SetState(EInitState.GameData);
            m_cGameDataMgr = CreateManager<GameDataManager>();
            yield return m_cGameDataMgr.Initialize();
            yield return SetState(EInitState.GameData_End);

            // Localization (After Asset, LocalData)
            yield return SetState(EInitState.Localization);
            m_cLocalizationMgr = CreateManager<LocalizationManager>();
            yield return m_cLocalizationMgr.Initialize();
            yield return SetState(EInitState.Localization_End);

            // Window
            yield return SetState(EInitState.Window);
            m_cWinMgr = CreateManager<WindowManager>();
            yield return m_cWinMgr.Initialize();
            yield return SetState(EInitState.Window_End);

            // SFX
            yield return SetState(EInitState.SFX);
            m_cSFXMgr = CreateManager<SFXManager>();
            yield return m_cSFXMgr.Initialize();
            yield return SetState(EInitState.SFX_End);

            // FX
            yield return SetState(EInitState.FX);
            m_cFXMgr = CreateManager<FXManager>();
            yield return m_cFXMgr.Initialize();
            yield return SetState(EInitState.FX_End);

            // Scene
            yield return SetState(EInitState.Scene);
            m_cSceneMgr = CreateManager<SceneManager>();
            yield return m_cSceneMgr.Initialize();
            yield return SetState(EInitState.Scene_End);

            // Sound : TODO : Change to BGM Manager
            yield return SetState(EInitState.Sound);
            m_cSoundMgr = CreateManager<SoundManager>();
            yield return m_cSoundMgr.Initialize();
            yield return SetState(EInitState.Sound_End);
            
            // ObjectPool
            yield return SetState(EInitState.ObjectPool);
            m_cPoolMgr = CreateManager<ObjectPoolManager>();
            yield return m_cPoolMgr.Initialize();
            yield return SetState(EInitState.ObjectPool_End);

            // World
            yield return SetState(EInitState.Contents);
            m_cWorldMgr = CreateManager<WorldManager>();
            yield return m_cWorldMgr.Initialize();
            yield return SetState(EInitState.Contents_End);

            // Quest
            yield return SetState(EInitState.Quest);
            m_cQuestMgr = CreateManager<QuestManager>();
            yield return m_cQuestMgr.Initialize();
            yield return SetState(EInitState.Quest_End);

            yield return SetState(EInitState.End);

#if !UNITY_EDITOR
            Application.targetFrameRate = LocalData.GetFrameRate();
#endif

            onComplete?.Invoke();
        }

        #endregion Implement IInitializer

        private void SetBasicSetting()
        {
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#else
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
            Application.runInBackground = true;

            SecureInt.GenerateKey();
            SecureFloat.GenerateKey();
            SecureLong.GenerateKey();

            DontDestroyOnLoad(this.gameObject);

            Fader = new ScreenFader(Color.black, 0.5f);
            TimeUtil.Init(this);
        }

        private IEnumerator SetState(EInitState state)
        {
            this.State = state;
            OnStateChange?.Invoke(state);
            yield return null;
        }

        private T CreateManager<T>() where T : ManagerBase
        {
            // 새로운 인스턴스 생성
            GameObject go_NewManager = new GameObject(typeof(T).Name);
            go_NewManager.transform.SetParent(this.transform);

            // 컴포넌트 부착
            var comp_T = go_NewManager.AddComponent(typeof(T)) as T;
            go_NewManager.isStatic = true;

            return comp_T;
        }

        public void RefreshBasicLocalData()
        {
            SetScreenResolution(LocalData.GetResolution());
            SetGraphicQuality(LocalData.GetGameQuality());
            SetGraphicTier(UnityEngine.Rendering.GraphicsTier.Tier3);

            int nRate = LocalData.GetFrameRate();
            Application.targetFrameRate = nRate;

            Debug.LogFormat("<color=green>Frame Rate : {0}</color>", nRate);
        }

        public void SetScreenResolution(EResolution eResolution)
        {
            int w, h;

            switch (eResolution)
            {

                case EResolution._1280x720_:
                    w = 1280;
                    h = 720;
                    break;
                case EResolution._1920x1080_:
                    w = 1920;
                    h = 1080;
                    break;

                case EResolution._2560x1440_:
                    w = 2560;
                    h = 1440;
                    break;
                default:
                //case EResolution.Unlimited:
                    w = Screen.width;
                    h = Screen.height;
                    break;
            }

            Debug.LogFormat("<color=green> Set Screen Resolution :{0} x {1}</color>", w, h);
            StartCoroutine(WaitForScreenChange(w, h));
        }

        private IEnumerator WaitForScreenChange(int nWidth, int nHeight, bool bFullScreen = true)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Screen.fullScreen = false;
#else
            Screen.fullScreen = bFullScreen;
#endif
            // Checking Frame End......
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Screen.SetResolution(width, height, Screen.fullScreen);
            Screen.SetResolution(nWidth, nHeight, false);
        }

        public void SetGraphicQuality(EGameQuality eQuality)
        {
            int nQuality = 0;
            switch(eQuality)
            {
                case EGameQuality.High:     nQuality = 5;       break;
                case EGameQuality.Medium:   nQuality = 3;       break;
                default:                    nQuality = 0;       break;
            }

            QualitySettings.SetQualityLevel(nQuality);

            Debug.LogFormat("<color=green>Graphic Quality : {0}</color>", eQuality);
        }

        public void SetGraphicTier(UnityEngine.Rendering.GraphicsTier tier)
        {
            Graphics.activeTier = tier;
        }
    }
}