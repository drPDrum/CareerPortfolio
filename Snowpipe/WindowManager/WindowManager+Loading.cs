using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectS
{
    public partial class WindowManager
    {
        private Loading_Image m_cLoadingPage = null;
        public bool IsLoading { get; private set; } = false;

        private Loading_Image GetLoading()
        {
            if (m_cLoadingPage != null)
                return m_cLoadingPage;

            Loading_Image cResult = null;
            if (cResult == null)
            {
                var obj = Managers.Asset.LoadAssetGameObject<GameObject>("System_UI/Loading_Image", false, m_transHolderLoading);
                if (obj == null)
                    throw new System.Exception(string.Format("로딩 프리펩을 찾을 수 없습니다"));

                obj.transform.Reset();
                cResult = obj.GetComponent<Loading_Image>();
            }

            cResult.transform.Reset();

            return cResult;
        }

        public void ShowLoading(ELoadingType eLoadingID, System.Action onEndShow)
        {
            if(IsLoading || eLoadingID == ELoadingType.None)
            {
                onEndShow?.Invoke();
                return;
            }

            m_cLoadingPage = GetLoading();
            if(m_cLoadingPage == null)
            {
                onEndShow?.Invoke();
                return;
            }

            m_cLoadingPage.gameObject.SetActive(true);
            m_cLoadingPage.ShowLoading(eLoadingID, onEndShow);
            IsLoading = true;
        }

        public void OutLoading(System.Action endCallback)
        {
            if (!IsLoading || m_cLoadingPage == null)
            {
                endCallback?.Invoke();
                return;
            }

            m_cLoadingPage.OutLoading(endCallback);
            m_cLoadingPage.gameObject.SetActive(false);
            IsLoading = false;            
        }
    }
}