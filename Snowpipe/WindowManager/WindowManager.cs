using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils;

namespace ProjectS
{
    public partial class WindowManager : ManagerBase
    {
        [SerializeField]
        private     RectTransform       m_transCanvasRoot = null;
        [SerializeField]
        private     Canvas              m_canvasRoot = null;
        [SerializeField]
        private     Camera              m_camUI = null;
        [SerializeField]
        private     EventSystem         m_eventSystem = null;
        [SerializeField]
        private     RectTransform       m_transHolderWorld = null;
        [SerializeField]
        private     RectTransform       m_transHolderWinNormal = null;
        [SerializeField]
        private     RectTransform       m_transHolderNavi = null;
        [SerializeField]
        private     RectTransform       m_transHolderWinPopup = null;
        [SerializeField]
        private     RectTransform       m_transHolderToast = null;
        [SerializeField]
        private     RectTransform       m_transHolderGlobalPopup = null;
        [SerializeField]
        private     RectTransform       m_transHolderNetAjection = null;
        [SerializeField]
        private     RectTransform       m_transHolderLoading = null;
        [SerializeField]
        private     RectTransform       m_transHolderSingleLineNoti = null;
        [SerializeField]
        private     RectTransform       m_transHolderSystemPopup = null;
        [SerializeField]
        private     RectTransform       m_transModal = null;

        private     NavigationBar       m_cNaviBar = null;
        private     GameObject          m_objNetBlock;
        private     GameObject          m_objNetBlockDesc;

        private readonly Dictionary<WindowID, WindowBase> m_dicWindowInstance = new Dictionary<WindowID, WindowBase>();
        private readonly LinkedList<WindowBase> m_listWindowStack = new LinkedList<WindowBase>();

        public Vector2 Resolution => m_transCanvasRoot?.rect.size ?? Vector2.zero;
        public Rect Rect => m_transCanvasRoot?.rect ?? Rect.zero;
        public RectTransform CanvasRootTransform => m_transCanvasRoot;
        public RectTransform HolderWorldTransform => m_transHolderWorld;
        public EventSystem EventSystem => m_eventSystem;
        public NavigationBar NavBar => GetNavigationBar();
        public Camera UiCamera => m_camUI;

        public bool IsNetBlockOn
        {
            get
            {
                if (m_objNetBlock == null)
                    return false;

                return m_objNetBlock.activeSelf;
            }
        }

        public bool IsNetBlockDescOn
        {
            get
            {
                if (m_objNetBlockDesc == null)
                    return false;

                return m_objNetBlockDesc.activeSelf;
            }
        }

        public bool IsInStack(WindowID windowID)
        {
            var wnd = m_dicWindowInstance.GetOrNull(windowID);
            if (wnd == null)
                return false;

            return wnd.Node_WindowStack.List == this.m_listWindowStack;
        }


        #region Initialize
        public override IEnumerator Initialize(System.Action onComplete = null)
        {
#if UNITY_EDITOR
            m_canvasRoot = GameObject.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (m_canvasRoot == null || !m_canvasRoot.gameObject.name.Contains("_Main"))
            {
#endif
            yield return null;
            
            var objCanvasMain = Managers.Asset.LoadAssetGameObject<GameObject>("System/Canvas_Main", parent: this.transform);            

            yield return null;
            m_canvasRoot = objCanvasMain.GetComponent(typeof(Canvas)) as Canvas;

            var canvasPerfect = objCanvasMain.GetComponentsInChildren<UGUICanvasPerfect>();
            if (canvasPerfect != null)
                canvasPerfect.ForEach((x) => x.UpdateSizeDelta());
#if UNITY_EDITOR
            }

            m_eventSystem = GameObject.FindObjectOfType(typeof(EventSystem)) as EventSystem;
            if (m_eventSystem == null)
            {
#endif
            yield return null;
            var objEventMain = Managers.Asset.LoadAssetGameObject<GameObject>("System/EventSystem_Main", parent: this.transform);
            
            yield return null;
            m_eventSystem = objEventMain.GetComponent(typeof(EventSystem)) as EventSystem;
            int defaultValue = m_eventSystem.pixelDragThreshold;
            m_eventSystem.pixelDragThreshold =
                    Mathf.Max(
                         defaultValue,
                         (int)(defaultValue * Screen.dpi / 160f));
#if UNITY_EDITOR
            }
#endif
            m_transCanvasRoot = m_canvasRoot.transform as RectTransform;
            if (m_transCanvasRoot != null)
                m_transCanvasRoot.localPosition = new Vector3(-5000.0f, -5000.0f, 0);

            m_camUI = m_transCanvasRoot.GetComponentInChildren<Camera>();

            yield return null;
            m_transHolderWorld = m_transCanvasRoot.Find("Holder_World") as RectTransform;
            m_transHolderWinNormal = m_transCanvasRoot.Find("Holder_WindowNormal") as RectTransform;
            m_transHolderNavi = m_transCanvasRoot.Find("Holder_Navi") as RectTransform;
            m_transHolderWinPopup = m_transCanvasRoot.Find("Holder_WindowPopup") as RectTransform;
            m_transHolderToast = m_transCanvasRoot.Find("Holder_Toast") as RectTransform;
            m_transHolderGlobalPopup = m_transCanvasRoot.Find("Holder_Popup") as RectTransform;
            m_transHolderNetAjection = m_transCanvasRoot.Find("Holder_NetAjection") as RectTransform;
            m_transHolderLoading = m_transCanvasRoot.Find("Holder_Loading") as RectTransform;
            m_transHolderSingleLineNoti = m_transCanvasRoot.Find("Holder_SingleLineNoti") as RectTransform;
            m_transHolderSystemPopup = m_transCanvasRoot.Find("Holder_SystemPopup") as RectTransform;
            m_transModal = m_transCanvasRoot.Find("Modal") as RectTransform;

            yield return null;
            if (m_transModal != null)
            {
                var btn_Modal = m_transModal.gameObject.GetOrAddComponent(typeof(ButtonEx)) as ButtonEx;
                btn_Modal.onClick.Subscribe(() => this.CloseLast(false));
                m_transModal.gameObject.SetActive(false);
            }

            yield return null;

            // 팝업 설정
            this.m_arrPopupPair = new PopupPair[2];
            var prefSystemPopup = Resources.Load<GameObject>("System_UI/SystemPopup");
            if (prefSystemPopup == null)
            {
                Debug.LogError("System_UI/SystemPopup for Common is Not Found");
            }
            else
            {
                var objPopup = GameObject.Instantiate(prefSystemPopup, m_transHolderGlobalPopup) as GameObject;
                objPopup.transform.Reset();
                objPopup.gameObject.SetActive(false);
                var cPopup = objPopup.GetComponent<SystemPopup>();
                cPopup.Init(EPopupType.Game);

                m_arrPopupPair[0] = new PopupPair(cPopup);

                objPopup = GameObject.Instantiate(prefSystemPopup, m_transHolderSystemPopup) as GameObject;
                objPopup.transform.Reset();
                objPopup.gameObject.SetActive(false);
                cPopup = objPopup.GetComponent<SystemPopup>();
                cPopup.Init(EPopupType.System);

                m_arrPopupPair[1] = new PopupPair(cPopup);
            }

            if (prefSystemPopup == null)
            {
                Debug.LogError("System_UI/Window_System_Popup is Not Found");
            }
            else
            {
             
            }

            yield return null;

            if (m_transHolderNavi != null && (m_cNaviBar == null))
            {
                var trans = m_transHolderNavi.Find("NavigationBar");
                if(trans != null)
                {
                    m_cNaviBar = trans.GetComponent<NavigationBar>();
                    m_cNaviBar?.SetActive(false);
                }
            }
        }
        #endregion

        #region Mono Life Cycle
        private void Awake()
        {
            //Awake_Background();
            //Awake_Touch();
        }
        private void Update()
        {
            //Update_Touch();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            //OnDisable_Background();
        }

        private void OnDestroy()
        {
            if (m_canvasRoot != null)
                GameObject.DestroyImmediate(m_canvasRoot.gameObject);
            if (m_eventSystem != null)
                GameObject.DestroyImmediate(m_eventSystem.gameObject);
        }

        #endregion

        #region For Window

        private NavigationBar GetNavigationBar()
        {
            if (m_cNaviBar == null)
            {
                Debug.LogError("NaviBar Setting ERROR");
            }

            return m_cNaviBar;
        }

        /// <summary>
        /// 윈도우 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="windowId">획득 하려는 윈도우 ID</param>
        /// <param name="ifNotExistCreate">인스턴스가 없을 경우 인스턴스를 생성할 것인가? true : 생성한다 // false : 생성하지 않는다</param>
        /// <returns></returns>
        public WindowBase GetWindow(WindowID windowId, bool ifNotExistCreate)
        {
            WindowBase result = null;
            if (m_dicWindowInstance.TryGetValue(windowId, out result))
            {
                if (result == null)
                    m_dicWindowInstance.Remove(windowId);
            }

            if (result != null)
                return result;

            if (!ifNotExistCreate)
                return null;

            GameObject objWindow = null;
            if(IsSystemUI(windowId))
                objWindow = Managers.Asset.LoadAssetGameObject<GameObject>(string.Format("System_UI/{0}", windowId.ToString()), false, m_transHolderWinNormal);
            else
                objWindow = Managers.Asset.LoadAssetGameObject<GameObject>(string.Format("UI_Window/{0}", windowId.ToString()), true, m_transHolderWinNormal);

            result = objWindow.GetComponent(typeof(WindowBase)) as WindowBase;
            result.SetWindowId(windowId);
            this.m_dicWindowInstance.Add(windowId, result);

            if (result.IsPopup)
                result.transform.SetParent(this.m_transHolderWinPopup);

            var rectTransform = result.transform as RectTransform;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            if (objWindow.activeSelf)
                objWindow.gameObject.SetActive(false);

            return result;
        }

        public WindowBase GetLastWindow()
        {
            return m_listWindowStack?.Last?.Value;
        }

        public WindowID GetLastWindowId()
        {
            var wnd_Last = GetLastWindow();
            if (wnd_Last == null)
                return WindowID.NONE;

            return wnd_Last.WindowId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowId"></param>
        /// <returns>현재 켜져있던 Window가 Open을 거부했을 경우 
        /// 요청한 Window는 activeFalse상태로 리턴됩니다. </returns>
        public WindowBase OpenWindow(WindowID windowId)
        {
            var window = GetWindow(windowId, true);

            if (!window.IsPopup)
            {
                var node_Window = m_listWindowStack.Last;
                while (node_Window != null)
                {
                    var closeWindow = node_Window.Value;
                    node_Window = node_Window.Previous;

                    if (!closeWindow.gameObject.activeSelf)
                        break;

                    closeWindow.OnEvent_OutLastDepth(false);
                    closeWindow.gameObject.SetActive(false);
                }
            }
            else
            {
                var node_Window = m_listWindowStack.Last;
                if (node_Window != null)
                    node_Window.Value.OnEvent_OutLastDepth(false);
            }

            if (window.Node_WindowStack.List != null)
                window.Node_WindowStack.List.Remove(window.Node_WindowStack);

            m_listWindowStack.AddLast(window.Node_WindowStack);
            window.gameObject.SetActive(true);
            window.transform.SetAsLastSibling();

            window.OnEvent_AfterOpen();
            window.OnEvent_OnLastDepth();

            RefreshModalState();
            window.RefreshNavNBackground();

            return window;
        }

        public T OpenWindow<T>(WindowID windowId) where T : WindowBase
        {
            return OpenWindow(windowId) as T;
        }

        public void RefreshLast()
        {
            var windowNode = m_listWindowStack.Last;
            if (windowNode == null)
                return;

            // 팝업이 아닌 Window까지 찾는다.
            while (windowNode != null && windowNode.Value.IsPopup)
            {
                var prevWindowNode = windowNode.Previous;
                if (prevWindowNode == null
                    || !prevWindowNode.Value.gameObject.activeSelf)
                    break;

                windowNode = windowNode.Previous;
            }

            // 갱신
            while (windowNode != null)
            {
                windowNode.Value.Refresh();
                windowNode = windowNode.Next;
            }
        }

        public bool CloseLast(bool bForce)
        {
            if (m_arrPopupPair != null)
            {
                for (int i = m_arrPopupPair.Length - 1; i >= 0; --i)
                {
                    var pair = m_arrPopupPair[i];
                    if (pair != null && pair.IsOpened)
                        return pair.CPopup.AutoCancel();
                }
            }

            if (!bForce)
            {
                if ((Managers.Scene != null && Managers.Scene.LoadingState != SceneManager.ELoadingState.None)
                    || (m_objNetBlock != null && m_objNetBlock.activeSelf))
                    return false;
            }

            if (m_listWindowStack == null || m_listWindowStack.Count == 0)
            {
                return true;
            }

            var node_CloseWindow = m_listWindowStack.Last;
            if (node_CloseWindow == null || node_CloseWindow.Value == null)
            {
                m_listWindowStack.RemoveLast();
                return true;
            }

            return node_CloseWindow.Value.CloseSelf();
        }

        public bool CloseWindow(WindowID windowID)
        {
            return CloseWindow(m_dicWindowInstance.GetOrNull(windowID));
        }

        public bool CloseWindow(WindowBase closeWindow)
        {
            if (closeWindow == null)
                return true;

            // 이미 꺼져 있는거여도 리턴
            if (closeWindow.Node_WindowStack.List == null)
                return true;

            // 일단 스택에서 제거
            var node_CloseWindow = closeWindow.Node_WindowStack;
            node_CloseWindow.List.Remove(node_CloseWindow);

            closeWindow.OnEvent_OutLastDepth(true);

            // 켜져 있는데 팝업이 아니라면 마지막 스택부터 팝업이 아닐때 까지 킨다.
            if (closeWindow.gameObject.activeSelf && !closeWindow.IsPopup)
            {
                var node_OpenWindow = m_listWindowStack.Last;
                while (node_OpenWindow != null)
                {
                    var openWindow = node_OpenWindow.Value;
                    if (!openWindow.IsPopup)
                    {
                        openWindow.RefreshNavNBackground();
                        break;
                    }

                    node_OpenWindow = node_OpenWindow.Previous;
                }

                while (node_OpenWindow != null)
                {
                    var openWindow = node_OpenWindow.Value;
                    node_OpenWindow = node_OpenWindow.Next;

                    openWindow.gameObject.SetActive(true);
                    openWindow.OnEvent_OnLastDepth();
                }
            }
            else
            {
                var node_OpenWindow = m_listWindowStack.Last;
                if (node_OpenWindow != null)
                    node_OpenWindow.Value.OnEvent_OnLastDepth();
            }

            // 윈도우 닫기
            closeWindow.gameObject.SetActive(false);
            RefreshModalState();

            return true;
        }

        public override void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// 주의! 모든 WindowEvent를 거치지 않고 스택상의 Window가 Off됩니다.
        /// </summary>
        /// <param name="bDestroy"></param>
        public void Clear(bool bDestroy)
        {
            var node_closeWindow = m_listWindowStack.Last;
            while (node_closeWindow != null)
            {
                var closeWindow = node_closeWindow.Value;
                node_closeWindow = node_closeWindow.Previous;

                closeWindow.gameObject.SetActive(false);
            }

            m_listWindowStack.Clear();

            if (bDestroy)
            {
                foreach (var pair in m_dicWindowInstance)
                {
                    if (pair.Value != null)
                        GameObject.Destroy(pair.Value.gameObject);
                }

                m_dicWindowInstance.Clear();
            }

            if (m_cNaviBar != null)
                m_cNaviBar.SetActive(false);

            RefreshModalState();
        }

        public bool IsLastWindow()
        {
            return m_listWindowStack.Count == 1;
        }
        #endregion

        public Vector2 ConvertScreenToCanvasPoint(Vector2 vScreen)
        {
            Vector2 result;

            if (this.m_transCanvasRoot == null
                || this.m_canvasRoot == null
                || this.m_canvasRoot.worldCamera == null)
                return Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    this.m_transCanvasRoot, vScreen, this.m_canvasRoot.worldCamera, out result);

            return result;
        }

        public Vector2 ConvertWorldToCanvasPoint(Vector3 vWorld)
        {
            return this.m_transCanvasRoot.InverseTransformPoint(vWorld);
        }

        public Vector2 ConvertViewportToCanvasPoint(Vector3 vViewport)
        {
            var vWorld = this.m_canvasRoot.worldCamera.ViewportToWorldPoint(vViewport);
            return this.m_transCanvasRoot.InverseTransformPoint(vWorld);
        }

        public Vector2 ConvertViewportToWorldPoint(Vector3 vViewport)
        {
            return this.m_canvasRoot.worldCamera.ViewportToWorldPoint(vViewport);
        }

        public void SetActiveNetBlock(bool bActiveAjection, bool bActiveDesc)
        {
#if DevClient
            var log = $"NetBlock : {bActiveAjection}\n\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}";
            Debug.Log(log);
#endif

            if (m_objNetBlock != null)
                m_objNetBlock.SetActive(bActiveAjection);

            if (m_objNetBlockDesc != null)
                m_objNetBlockDesc.SetActive(bActiveDesc);
        }

        public void RefreshModalState()
        {
            if (m_transModal == null)
                return;

            RectTransform rttr_Parent = null;
            int idx_Sibling = -1;

            m_transModal.gameObject.SetActive(false);
            m_transModal.SetParent(m_transCanvasRoot);

            // 시스템 팝업이 살아있다면
            if (m_arrPopupPair != null)
            {
                for (int i = m_arrPopupPair.Length - 1; i >= 0; --i)
                {
                    var pair = m_arrPopupPair[i];
                    if (pair.IsOpened)
                    {
                        rttr_Parent = pair.CPopup.transform.parent as RectTransform;
                        idx_Sibling = pair.CPopup.transform.GetSiblingIndex();
                    }
                }
            }

            if (rttr_Parent == null)
            {
                if (m_listWindowStack == null || m_listWindowStack.Count <= 0)
                    return;

                var node_Window = m_listWindowStack.Last;
                if (node_Window == null)
                    return;


                while (node_Window != null)
                {
                    var window = node_Window.Value;
                    node_Window = node_Window.Previous;

                    if (!window.gameObject.activeSelf)
                        break;

                    if (window.UseBlurBackground)
                    {
                        rttr_Parent = window.transform.parent as RectTransform;
                        idx_Sibling = window.transform.GetSiblingIndex();
                        break;
                    }
                }
            }

            if (rttr_Parent != null)
            {
                m_transModal.gameObject.SetActive(true);
                m_transModal.SetParent(rttr_Parent);
                m_transModal.SetSiblingIndex(idx_Sibling);

                m_transModal.anchoredPosition = Vector3.zero;
                m_transModal.sizeDelta = new Vector2(1800, 800);
            }
        }
    }
}