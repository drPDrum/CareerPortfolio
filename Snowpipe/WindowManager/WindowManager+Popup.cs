using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProjectS
{
    public partial class WindowManager
    {
        public enum EPopupType
        {
            Game,
            System,
        }

        public enum EBtnCountType
        {
            None,
            One,
            Two,
        }

        private class PopupPair
        {
            public SystemPopup CPopup { get; private set; }
            public Queue<SystemPopup.PopupInfo> QPopupInfo { get; private set; }

            public PopupPair(SystemPopup cPopup)
            {
                CPopup = cPopup;
                QPopupInfo = new Queue<SystemPopup.PopupInfo>();
            }

            public bool IsValid => CPopup != null && QPopupInfo != null;
            public bool IsOpened => CPopup != null && CPopup.gameObject.activeSelf;
        }

        private PopupPair[] m_arrPopupPair;
        private const string COMMON_CONFIRM = "UICOMMON_CONFIRM";
        private const string COMMON_CANCEL = "UICOMMON_CANCEL";

        public bool IsActivePopup
        {
            get
            {
                if (m_arrPopupPair == null)
                    return false;

                for (int i = 0; i < this.m_arrPopupPair.Length; ++i)
                {
                    if (this.m_arrPopupPair[i].IsOpened)
                        return true;
                }

                return false;
            }
        }

        private void ExcutePopupInfo(EPopupType ePopupType, SystemPopup.PopupInfo sInfo)
        {
            var cPopupPair = GetPopupPair(ePopupType);
            if (cPopupPair == null || !cPopupPair.IsValid)
                return;

            if (cPopupPair.IsOpened)
            {
                cPopupPair.QPopupInfo.Enqueue(sInfo);
            }
            else
            {
                cPopupPair.CPopup.SetPopupInfo(sInfo);
                RefreshModalState();
                var wnd_Last = GetLastWindow();
                if (wnd_Last != null)
                    wnd_Last.OnEvent_OutLastDepth(false);
            }
        }

        #region One Button

        public void EnqueuePopup(string strContents, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            var strBtnOk = Managers.LZ[COMMON_CONFIRM];
            EnqueuePopupOneButton(strContents, strBtnOk, null, true, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents, string strBtnOk, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            EnqueuePopupOneButton(strContents, strBtnOk, null, true, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents, Action onCallbackBtnOk, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            var strBtnOk = Managers.LZ[COMMON_CONFIRM];
            EnqueuePopupOneButton(strContents, strBtnOk, onCallbackBtnOk, true, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents, 
            string strBtnOk, Action onCallbackBtnOk,
            bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            EnqueuePopupOneButton(strContents, strBtnOk, onCallbackBtnOk, true, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents, 
            string strBtnOk, Action onCallbackBtnOk, bool bOkClose,
            bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            EnqueuePopupOneButton(strContents, strBtnOk, onCallbackBtnOk, bOkClose, bBackClose, eType);
        }

        private void EnqueuePopupOneButton(string strContents,
            string strBtnOk, Action onCallbackBtnOk, bool bOkClose,
            bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            SystemPopup.PopupInfo sInfo = new SystemPopup.PopupInfo()
            {
                strContents         = strContents,
                strBtnOk            = strBtnOk,
                onCallbackBtnOk     = onCallbackBtnOk,
                bOkClose            = bOkClose,
                eCountType          = EBtnCountType.One,

                bBackClose          = bBackClose
            };

            ExcutePopupInfo(eType, sInfo);
        }

        #endregion One Button

        #region Two Button



        public void EnqueuePopup(string strContents, string strBtnOk, string strBtnNo,
           Action onCallbackBtnBack = null, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            EnqueuePopupTwoButton(strContents, strBtnOk, null, true, strBtnNo, null, true, onCallbackBtnBack, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents, Action onCallbackBtnOk, Action onCallbackBtnNo,
            Action onCallbackBtnBack = null, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            var strBtnOk = Managers.LZ[COMMON_CONFIRM];
            var strBtnNo = Managers.LZ[COMMON_CANCEL];
            EnqueuePopupTwoButton(strContents, strBtnOk, onCallbackBtnOk, true, strBtnNo, onCallbackBtnNo, true, onCallbackBtnBack, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents,
            string strBtnOk, Action onCallbackBtnOk,
            string strBtnNo, Action onCallbackBtnNo,
            Action onCallbackBtnBack = null, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            EnqueuePopupTwoButton(strContents, strBtnOk, onCallbackBtnOk, true, strBtnNo, onCallbackBtnNo, true, onCallbackBtnBack, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents,
            Action onCallbackBtnOk, bool bOkClose,
            Action onCallbackBtnNo, bool bNoClose,
            Action onCallbackBtnBack = null, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            var strBtnOk = Managers.LZ[COMMON_CONFIRM];
            var strBtnNo = Managers.LZ[COMMON_CANCEL];

            EnqueuePopupTwoButton(strContents, strBtnOk, onCallbackBtnOk, bOkClose, strBtnNo, onCallbackBtnNo, bNoClose, onCallbackBtnBack, bBackClose, eType);
        }

        public void EnqueuePopup(string strContents,
            string strBtnOk, Action onCallbackBtnOk, bool bOkClose,
            string strBtnNo, Action onCallbackBtnNo, bool bNoClose,
            Action onCallbackBtnBack = null, bool bBackClose = true, EPopupType eType = EPopupType.Game)
        {
            EnqueuePopupTwoButton(strContents, strBtnOk, onCallbackBtnOk, bOkClose, strBtnNo, onCallbackBtnNo, bNoClose, onCallbackBtnBack, bBackClose, eType);
        }

        private void EnqueuePopupTwoButton(string strContents,
            string strBtnOk, Action onCallbackBtnOk, bool bOkClose,
            string strBtnNo, Action onCallbackBtnNo, bool bNoClose,
            Action onCallbackBtnBack = null, bool bBackClose = true,
            EPopupType eType = EPopupType.Game)
        {
            SystemPopup.PopupInfo sInfo = new SystemPopup.PopupInfo()
            {
                strContents         = strContents,
                strBtnOk            = strBtnOk,
                onCallbackBtnOk     = onCallbackBtnOk,
                bOkClose            = bOkClose,
                strBtnNo            = strBtnNo,
                onCallbackBtnNo     = onCallbackBtnNo,
                bNoClose            = bNoClose,

                onCallbackBtnBack   = onCallbackBtnBack,
                bBackClose          = bBackClose,
                eCountType          = EBtnCountType.Two,
            };

            ExcutePopupInfo(eType, sInfo);
        }


        #endregion Two Button

        #region Localization
        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    EnqueuePopup_Lz(localKey_Title, localKey_Contents, "SYSTEM_POPUP_STANDARD_CONFIRM", null, null, null, null, null, popupType);
        //}

        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    string localKey_Ok,
        //    Action okCallback,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    EnqueuePopup_Lz(localKey_Title, localKey_Contents, localKey_Ok, okCallback, null, null, null, null, popupType);
        //}

        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    Action okCallback,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    EnqueuePopup_Lz(localKey_Title, localKey_Contents, "SYSTEM_POPUP_STANDARD_CONFIRM", okCallback, null, null, null, null, popupType);
        //}

        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    string localKey_Ok,
        //    Action okCallback,
        //    string localKey_Cancle,
        //    Action cancelCallback,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    EnqueuePopup_Lz(localKey_Title, localKey_Contents, localKey_Ok, okCallback, null, null, localKey_Cancle, cancelCallback, popupType);
        //}

        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    Action okCallback,
        //    Action cancelCallback,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    EnqueuePopup_Lz(localKey_Title, localKey_Contents, "SYSTEM_POPUP_STANDARD_OK", okCallback, null, null, "SYSTEM_POPUP_STANDARD_CANCEL", cancelCallback, popupType);
        //}

        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    Action okCallback,
        //    Action noCallback,
        //    Action cancelCallback,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    EnqueuePopup_Lz(localKey_Title, localKey_Contents, "SYSTEM_POPUP_STANDARD_OK", okCallback, "SYSTEM_POPUP_STANDARD_NO", noCallback, "SYSTEM_POPUP_STANDARD_CANCEL", cancelCallback, popupType);
        //}

        //public void EnqueuePopup_Lz(
        //    string localKey_Title,
        //    string localKey_Contents,
        //    string localKey_Ok,
        //    Action okCallback,
        //    string localKey_No,
        //    Action noCallback,
        //    string localKey_Cancel,
        //    Action cancelCallback,
        //    PopupType popupType = PopupType.Game
        //    )
        //{
        //    var str_Title    = string.IsNullOrEmpty(localKey_Title)    ? null : Managers.Localization[localKey_Title];
        //    var str_Contents = string.IsNullOrEmpty(localKey_Contents) ? null : Managers.Localization[localKey_Contents];
        //    var str_Ok       = string.IsNullOrEmpty(localKey_Ok)       ? null : Managers.Localization[localKey_Ok];
        //    var str_No       = string.IsNullOrEmpty(localKey_No)       ? null : Managers.Localization[localKey_No];
        //    var str_Cancel   = string.IsNullOrEmpty(localKey_Cancel)   ? null : Managers.Localization[localKey_Cancel];

        //    EnqueuePopup(str_Title, str_Contents, str_Ok, okCallback, str_No, noCallback, str_Cancel, cancelCallback, popupType);
        //}
        #endregion Localization

        public void OnPopupClose(EPopupType popupType)
        {
            var popupPair = GetPopupPair(popupType);
            if (popupPair == null || !popupPair.IsValid)
                return;

            if (popupPair.QPopupInfo.Count <= 0)
            {
                RefreshModalState();

                var wnd_Last = GetLastWindow();
                if (wnd_Last != null)
                    wnd_Last.OnEvent_OnLastDepth();

                return;
            }

            var popupInfo = popupPair.QPopupInfo.Dequeue();
            popupPair.CPopup.SetPopupInfo(popupInfo);
        }

        private PopupPair GetPopupPair(EPopupType eType)
        {
            if (m_arrPopupPair == null)
                return null;

            var idx_PopupType = (int)eType;
            if (!m_arrPopupPair.CheckIndex(idx_PopupType))
                return null;

            return m_arrPopupPair[idx_PopupType];
        }
    }
}
