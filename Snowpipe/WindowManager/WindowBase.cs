using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ProjectS
{
    public abstract class WindowBase : MonoBehaviour
    {
        // WindowManager에서 스택 관리 하기 위한 Node
        public LinkedListNode<WindowBase> Node_WindowStack { get; private set; }
        public WindowID WindowId { get; private set; }

        [Header("WindowBase")]
        [SerializeField]
        protected Animation m_anim = null;
        [SerializeField]
        protected AnimationClip m_animClip_OpenWindow;
        [SerializeField]
        protected Image m_imgTouchCloseArea = null;
        //[SerializeField]
        //private bool _bView3D = false;

        [Header("WindowBase :: Popup Setting")]
        // Popup형태 (이전 스택의 UI와 겹침) 여부
        public bool IsPopup = false;
        public bool UseBlurBackground = true;

        [Header("WindowBase :: GNB setting -Non popup type only-")]
        // NavigationBar 아이템 활성화 여부
        [EnumFlag]
        public NavigationBar.ItemFlagTypes m_flagNavBarSetting;
        public DataFileEnum.EGoodsType[] m_arrGoodsTypes;

        protected string m_strTitleKey;
        protected string m_strSubTitleKey;

        protected bool IsApplicationQuitting { get; private set; } = false;

        public bool IsLastDepth => Node_WindowStack.List != null && Node_WindowStack.Next == null && !Managers.Window.IsActivePopup;

        protected virtual void Awake()
        {
            if (m_anim != null)
                m_anim.playAutomatically = false;

            Node_WindowStack = new LinkedListNode<WindowBase>(this);

            if (m_imgTouchCloseArea != null)
            {
                var btn = m_imgTouchCloseArea.GetOrAddComponent<ButtonEx>();
                btn.onClick.Subscribe(() => CloseSelf());
            }

            var strTypeName = this.GetType().Name;
            m_strTitleKey = strTypeName;

            Application.quitting += OnQuitting;
        }

        protected virtual void OnDestroy()
        {
            Application.quitting -= OnQuitting;
        }

        /// <summary>
        /// 뒤로가기, 새로열기 등 화면 전면에 나오면 무조건 호출됨
        /// </summary>
        public virtual void Refresh()
        {
        }

        public virtual void RefreshNavNBackground()
        {
            RefreshNav();
            RefreshBackground();
            RefreshBgm();
            RefreshTitleText();
        }

        public virtual void RefreshNav()
        {
            if (IsPopup)
                return;

            var cNavBar = Managers.Window.NavBar;

            if (cNavBar == null)
                return;

            if ((int)m_flagNavBarSetting == 0 && m_arrGoodsTypes.Length == 0)
            {
                cNavBar.SetActive(false);
                return;
            }

            cNavBar.SetActive(true);
            cNavBar.SetButtonInfo(m_flagNavBarSetting);
            cNavBar.SetGoodsInfoUI(m_arrGoodsTypes);
            RefreshTitleText();
        }

        public virtual void RefreshBackground()
        {
            //TODO
        }

        public virtual void RefreshBgm()
        {
            //TODO
        }

        /// <summary>
        /// 스택에 없던 WIndow가 새로열린 직후에만 호출됨
        /// </summary>
        public virtual void OnEvent_AfterOpen()
        {
            SafeAddAnimClip(m_animClip_OpenWindow);
            if (m_anim != null && m_animClip_OpenWindow != null)
                m_anim.Play(m_animClip_OpenWindow.name);
        }

        /// <summary>
        /// 다음과 같은 경우에 이벤트가 호출됩니다.
        /// 
        /// 1. 신규로 OpenWindow 될 때
        /// 2. 이후에 켜진 Window가 모두 Close되어 재 Open될때
        /// </summary>
        public virtual void OnEvent_OnLastDepth()
        {
            this.Refresh();
        }

        /// <summary>
        /// 다음과 같은 경우에 이벤트가 호출됩니다.
        /// 
        /// 1. Popup을 포함한 화면 최상단에서 출력되던 도중, 다른 Window 혹은 Popup이 OpenWindow 되기 직전
        /// 2. Window가 스택에서 제거되어 완전히 종료될 때
        /// </summary>
        /// <param name="isClose">
        /// true : Close로 인한 Inactive, 즉 윈도우 스택에서 Pop 될 때
        /// false : OpenWindow로 인한 Inactive
        /// </param>
        public virtual void OnEvent_OutLastDepth(bool isClose)
        {
            if (m_anim != null && m_animClip_OpenWindow != null)
            {
                if (!isClose && m_anim.IsPlaying(m_animClip_OpenWindow.name))
                {
                    m_anim.Stop();
                    this.SampleAnimClip(m_animClip_OpenWindow.name, 1f);
                }
            }
        }

        public virtual bool CloseSelf()
        {
            return Managers.Window.CloseWindow(this);
        }

        protected void SafeAddAnimClip(AnimationClip animClip)
        {
            if (animClip == null)
                return;

            if (m_anim == null)
            {
                m_anim = this.gameObject.GetOrAddComponent(typeof(Animation)) as Animation;
                m_anim.playAutomatically = false;
            }

            if (m_anim.GetClip(animClip.name) != null)
                m_anim.RemoveClip(animClip.name);

            m_anim.AddClip(animClip, animClip.name);
        }

        protected void SampleOpenAnimClip(float factor)
        {
            if (m_animClip_OpenWindow == null)
                return;

            SampleAnimClip(m_animClip_OpenWindow, factor);
        }

        public void SampleAnimClip(string clipName, float factor)
        {
            if (m_anim == null
                || string.IsNullOrEmpty(clipName)
                || m_anim.GetClip(clipName) == null)
                return;

            m_anim.Stop();
            m_anim[clipName].enabled = true;
            m_anim[clipName].time = m_anim[clipName].length * factor;
            m_anim[clipName].weight = 1;
            m_anim.Sample();
            m_anim[clipName].enabled = false;
        }

        protected void SampleAnimClip(AnimationClip clip, float factor)
        {
            if (clip == null)
                return;

            SampleAnimClip(clip.name, factor);
        }

        private void OnQuitting()
        {
            IsApplicationQuitting = true;
        }

        public void SetWindowId(WindowID windowId)
        {
            this.WindowId = windowId;
        }

        public void SetTitleText(string strTitleKey, string strSubTitleKey = null)
        {
            m_strTitleKey = strTitleKey;
            m_strSubTitleKey = strSubTitleKey;

            RefreshTitleText();
        }

        public virtual void RefreshTitleText()
        {
            string strTitle = null;
            if (!Managers.LZ.GetText(m_strTitleKey, out strTitle))
                strTitle = null;

            string strSubTitle = null;
            if (!Managers.LZ.GetText(m_strSubTitleKey, out strSubTitle))
                strSubTitle = null;

            Managers.Window.NavBar.SetUITitle(strTitle, strSubTitle);
        }
    }
}