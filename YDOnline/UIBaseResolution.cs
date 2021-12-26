using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIBaseResolution : UIBase
    {
        [SerializeField]
        private Transform[]     m_arrPivots = null;

        private const float     UI_BASIC_RATIO = 0.5625f;   // 16:9
        private const int       UI_BASIC_WIDTH = 1280;
        private const int       UI_BASIC_HEIGHT = 720;

        protected override void Awake()
        {
            base.Awake();

            SetResolutionPivot();
        }

        private void SetResolutionPivot()
        {
            int nWidth = Screen.width;
            int nHeight = Screen.height;

            float fRatio = (float)nHeight / (float)nWidth;

            if(fRatio > UI_BASIC_RATIO)
            {
                m_root.manualWidth = UI_BASIC_WIDTH;
                m_root.manualHeight = (int)(UI_BASIC_WIDTH * fRatio + 0.5f);
            }
            else
            {
                m_root.manualWidth = (int)(UI_BASIC_HEIGHT * (float)nWidth / (float)nHeight + 0.5f);
                m_root.manualHeight = UI_BASIC_HEIGHT; //
            }

            if(m_arrPivots == null)
                return;

            float fWidth = m_root.manualWidth * 0.5f;
            float fHeight = m_root.manualHeight * 0.5f;

            for(int i = 0 ; i < m_arrPivots.Length ; ++i)
                if(m_arrPivots[i] == null)
                    return;

            Vector3 vPos = m_arrPivots[0].localPosition;
            vPos.y = fHeight;
            m_arrPivots[0].localPosition = vPos;

            vPos = m_arrPivots[1].localPosition;
            vPos.y = -fHeight;
            m_arrPivots[1].localPosition = vPos;

            vPos = m_arrPivots[2].localPosition;
            vPos.x = -fWidth;
            m_arrPivots[2].localPosition = vPos;

            vPos = m_arrPivots[3].localPosition;
            vPos.x = fWidth;
            m_arrPivots[3].localPosition = vPos;
        }
    }
}