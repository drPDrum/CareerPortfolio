using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjectS
{
    public class SceneManager : ManagerBase
    {
        public enum ELoadingState
        {
            None,
            LoadingStart,
            Loading,
            LoadingEnd,
        }

        private Action  m_onSceneLoadEnd = null;

        public ELoadingState LoadingState { get; private set; } = ELoadingState.None;
        public string CurSceneName { get; private set; } = string.Empty;
        public SceneControllerBase CurSceneController { get; private set; } = null;                
        public ESceneType CurSceneType { get { return CurSceneController?.SceneType ?? ESceneType.None; } }
        public CSceneParam CurSceneParam { get; private set; } = null;

        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            CurSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            CurSceneController = GameObject.FindObjectOfType<SceneControllerBase>();

            if (CurSceneController == null)
            {
#if UNITY_EDITOR
                if(CurSceneName == null || !CurSceneName.ToLower().Contains("test"))
#endif
                Debug.LogError("Scene Controll is Empty in " + CurSceneName);
            }

            yield break;
        }

        /// <summary>
        /// Scene Controller와 로딩처리를 하는 관리 함수
        /// </summary>
        public void LoadScene(string strScene, CSceneParam cParam = null, bool bUseAssetBundle = false, ELoadingType eLoadingID = ELoadingType.None, 
            bool bClearFXObjects = true, bool bAsyncLoad = true, Action onComplete = null)
        {
            Debug.Log("ScenenManager :: Load Scene [To : " + strScene + "] : [From : " + CurSceneName + "]\n" +
                "UseAssetBundle? : " + bUseAssetBundle + " Loading ID : " + eLoadingID + " ClearFX? : " + bClearFXObjects + " AsyncLoad? " + bAsyncLoad);

            if (LoadingState != ELoadingState.None)
                return;

            if(string.Compare(strScene, CurSceneName) == 0)
            {
                if(string.Compare(strScene, CConst.SCENE_NAME_LOADING) == 0)
                {
                    Debug.LogError("Can't loading in loading SCENE");
                    return;
                }

                LoadScene(CConst.SCENE_NAME_LOADING, cParam);
                return;
            }

            CurSceneParam = cParam;
            m_onSceneLoadEnd = null;
            m_onSceneLoadEnd = onComplete;
            LoadingState = ELoadingState.LoadingStart;

            Action onSceneLoad = () =>
            {
                Debug.Log("ScenenManager :: onSceneLoad : " + LoadingState + " : " + CurSceneName);
                if(bClearFXObjects)
                {
                    Managers.FX.RetrieveAllItems();
                    Managers.SFX.RetrieveAllItems();
                }

                // 씬 종료 이벤트 콜
                CurSceneController?.OnEndScene();

                Managers.Window.Clear();

                LoadingState = ELoadingState.Loading;
                CurSceneName = strScene;

                CurSceneController = null;

#if USE_FULL_BUILD                
                if(bUseAssetBundle)
                {
                    var arrSplit = strScene.Split('/');
                    if(arrSplit.Length > 1)
                        strScene = arrSplit[arrSplit.Length - 1];
                    Debug.Log("New SceneName = " + strScene);
                }                

                bUseAssetBundle = false;
#endif //USE_FULL_BUILD

                if(bAsyncLoad)
                    StartCoroutine(LoadSceneAsync(strScene, bUseAssetBundle, false));
                else
                    LoadSceneSync(strScene, bUseAssetBundle, false);

            };

            Managers.Window.ShowLoading(eLoadingID, onSceneLoad);            
        }

        /// <summary>
        /// 실제 씬 로딩처리를 하는 함수. Async
        /// </summary>
        public IEnumerator LoadSceneAsync(string strScene, bool bUseAssetBundle, bool bAdditive, Action onComplete = null)
        {
            AsyncOperation operation = null;

            if (bUseAssetBundle)
            {
                operation = Managers.Asset.LoadSceneAsync(strScene, bAdditive);
            }
            else
            {
                if (bAdditive)
                    operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(strScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                else
                    operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(strScene);
            }

            yield return new WaitUntil(() => operation.progress >= 1f);

            onComplete?.Invoke();
        }

        /// <summary>
        /// 실제 씬 로딩처리를 하는 함수. Sync
        /// </summary>
        public void LoadSceneSync(string strScene, bool bUseAssetBundle, bool bAdditive)
        {
            if (bUseAssetBundle)
            {
                Managers.Asset.LoadScene(strScene, bAdditive);
            }
            else
            {
                if (bAdditive)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(strScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(strScene);
                }
            }
        }

        /// <summary>
        /// 씬 언로딩처리를 하는 함수. ASync
        /// </summary>
        public IEnumerator UnLoadSceneAsync(string strScene, Action onComplete = null)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(strScene);
            var operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);

            yield return new WaitUntil(() => operation.progress >= 1.0f);

            yield return null;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 각 SceneController에서 Init이 종료될 때 호출해주세요
        /// </summary>
        public void OnEndLoading(SceneControllerBase cController)
        {
            Debug.Log("SceneManager :: OnEndLoading");

            System.Action onSceneLoadEnd = () =>
            {
                LoadingState = ELoadingState.None;

                if(CurSceneController == null)
                {
                    Debug.LogError("Scene Controller is Empty in " + CurSceneName);
                    return;
                }

                CurSceneController.OnStartScene();                

                m_onSceneLoadEnd?.Invoke();
                m_onSceneLoadEnd = null;
            };

            LoadingState = ELoadingState.LoadingEnd;
            CurSceneController = cController;

            Managers.Window.OutLoading(onSceneLoadEnd);
        }
    }
}

