using UnityEngine;
using UnityEngine.UI;


namespace ProjectS
{
    public partial class NavigationBar : MonoBehaviour
    {
        [System.Flags]
        public enum ItemFlagTypes
        {
            //None = 0,
            HomeButton = 1 << 0,
            BackButton = 1 << 1,
        }

        [Header("NaviBar Child GameObjects")]
        [SerializeField]
        private     GameObject  m_objTitleRoot = null;
        [SerializeField]
        private     Text        m_txtTitle = null;
        [SerializeField]
        private     Text        m_txtSubTitle = null;
        [SerializeField]
        private     ButtonEx    m_btnHome = null;
        [SerializeField]
        private     ButtonEx    m_btnBack = null;

        private     GameObject  m_obj = null;

        private void Awake()
        {
            if (m_btnHome != null)
                m_btnHome.onClick.Subscribe(OnClickHome);
            if (m_btnBack != null)
                m_btnBack.onClick.Subscribe(OnClickBack);
        }

        public void SetActive(bool bEnable)
        {
            if (m_obj == null)
                m_obj = gameObject;

            m_obj.SetActive(bEnable);
        }

        public void SetButtonInfo(ItemFlagTypes flags)
        {
            if (m_btnHome != null)
                m_btnHome.gameObject.SetActive((flags & ItemFlagTypes.HomeButton) != 0);
            if (m_btnBack != null)
                m_btnBack.gameObject.SetActive((flags & ItemFlagTypes.BackButton) != 0);
        }

        public bool GetActive(ItemFlagTypes item)
        {
            switch (item)
            {
                case ItemFlagTypes.HomeButton:
                    return m_btnHome != null && m_btnHome.gameObject.activeSelf;
                case ItemFlagTypes.BackButton:
                    return m_btnBack != null && m_btnBack.gameObject.activeSelf;
                default:
                    return false;
            }
        }

        public void Refresh()
        {
            Refresh_GoodsInfo();
        }

        public void SetUITitle(string strTitle, string strSubTitle = null)
        {
            if (m_txtTitle != null)
                m_txtTitle.text = strTitle;

            if (string.IsNullOrWhiteSpace(strSubTitle))
            {
                if (m_txtSubTitle != null)
                    m_txtSubTitle.gameObject.SetActive(false);
            }
            else
            {
                if (m_txtSubTitle != null)
                {
                    m_txtSubTitle.gameObject.SetActive(true);
                    m_txtSubTitle.text = strSubTitle;
                }
            }

            if (m_objTitleRoot != null)
                m_objTitleRoot.SetActive(!string.IsNullOrWhiteSpace(strTitle));
        }

        public void OnClickBack()
        {
            if(Managers.Window.IsLastWindow())
            {
                Managers.World.ReturnToShelter();
                return;
            }

            Managers.Window.CloseLast(false);
        }

        public void OnClickHome()
        {
            Managers.World.ReturnToShelter();
        }
    }
}
