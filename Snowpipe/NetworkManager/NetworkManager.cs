using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using ProjectS.Protocol;
using BestHTTP;

namespace ProjectS
{
    public partial class NetworkManager : ManagerBase
    {
        public class ServerInfo
        {
            public string name;

            public string login_url;
            public int login_port;

            public string game_url;
            public int game_port;
        }

        private static ServerInfo[] serverInfos = new ServerInfo[]
        {
            new ServerInfo()
            {
                 name = "DEV",
                 login_url = "http://172.20.40.224",
                 login_port = 10003,
                 game_url = "http://172.20.40.224",
                 game_port = 10000,
            },

            new ServerInfo()
            {
                 name = "QA",
                 login_url = "--",
                 login_port = 0,
                 game_url = "--",
                 game_port = 0,
            },

            new ServerInfo()
            {
                 name = "JSJLocal",
                 login_url = "http://172.20.41.156",
                 login_port = 10003,
                 game_url = "http://172.20.41.156",
                 game_port = 10000,
            },
        };

        public static ServerInfo GetCurrServerInfo()
        {
#if QA
            return serverInfos[1];
#endif
            //DEV
            return serverInfos[0];

            ////JSJLocal
            //return serverInfos[2];
        }

        private const string FMT_IP_PORT = "{0}:{1}";

        private     string      m_strServerUrl = string.Empty;

        private     string      m_strChatUrl = string.Empty;

        private     int         m_strPacketNO = 0;

        public int PacketNO
        {
            get
            {
                if (++m_strPacketNO == int.MaxValue)
                    m_strPacketNO = 1;

                return m_strPacketNO;
            }
        }

        public string SessionID { get; set; }

        public override IEnumerator Initialize(System.Action onComplete = null)
        {
            var serverInfo = GetCurrServerInfo();
            m_strServerUrl = string.Format(FMT_IP_PORT, serverInfo.login_url, serverInfo.login_port);
            Debug.Log("Login Server : " + m_strServerUrl);
            m_strPacketNO = 0;
            yield break;
        }

        public void SetServerUrl(string ip, int port)
        {
            Debug.LogFormat("<size=20><color=yellow>SetServerUrl : [{0}]:{1}\n</color></size>", ip, port);

            if (string.IsNullOrEmpty(ip))
            {
                Debug.LogError("invalid ip!!!!!");
                return;
            }

            m_strServerUrl = string.Format(FMT_IP_PORT, ip, port);
        }

        public void SetChatUrl(string ip, int port)
        {
            Debug.LogFormat("<size=20><color=yellow>SetChatUrl : [{0}]:{1}\n</color></size>", ip, port);

            if (string.IsNullOrEmpty(ip))
            {
                Debug.LogError("invalid ip!!!!!");
                return;
            }

            m_strChatUrl = string.Format(FMT_IP_PORT, ip, port);
        }

#region CDN Check maintenance
        public void CheckMaintenance(System.Action<bool> onComplete)
        {
            StartCoroutine(RoutineCheckMaintenance(onComplete));
        }
        
        private IEnumerator RoutineCheckMaintenance(System.Action<bool> onComplete)
        {
            string target_path = string.Format(GameConstant.CDN_MAINTENANCE_URL, Common.ENVIRONMENT, Common.APP_VERSION);

            Debug.LogFormat("<color=yellow>점검 체크(CDN):{0}</color>", target_path);

            using (UnityWebRequest req = UnityWebRequest.Get(target_path))// WWW www = new WWW(target_path))
            {   
                yield return req;

                //! 에러 이거나 파일이 없을경우 정상 진행.. 파일이 있을경우 점검중 표시 후, 정상 진행.

                if (req.error != null) // 정상진행..
                {
                    onComplete(false);

                    yield break;
                }
                else // 점검중..
                {
                    Debug.Log("<color=yellow>[서버 점검 상태]: 점검 파일이 존재함.(CDN)</color>");

                    Managers.Window.EnqueuePopup(Managers.LZ["server_maintenance_cdn"], () => onComplete(true));

                    //UIEventDispatcher.UIEventMIC.Say(new UILoadingEventParam(false));
                }
            }

        }

#endregion CDN Check maintenance

#region New FrameWork
        private const string aeskey = "--";

        private static byte[] MakeEncByte(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return new byte[0];
            }

#if USE_LOG
            Debug.LogFormat("request: {0}", message);
#endif //USE_LOG

            var encrypted = AESENC.AES.AESEncrypt256(message, aeskey);

            return System.Text.Encoding.UTF8.GetBytes(encrypted);
        }

        private static string MakeDesString(byte[] param)
        {
            var utf8Decoded = System.Text.Encoding.UTF8.GetString(param);

            //Debug.LogFormat("utf8Decoded: {0}", utf8Decoded);

            var message = AESENC.AES.AESDecrypt256(utf8Decoded, aeskey);

#if USE_LOG
            Debug.LogFormat("receive: {0}", message);
#endif //USE_LOG

            return message;
        }

        private const HTTPMethods HTTP_METHOD = HTTPMethods.Post;
        private const int TIMEOUT_SEC = 30;
        private const bool DISABLE_CACHE = true;

        public void Request<T>(CProtocolBase req, System.Action<Response<T>> resp) where T : CResponseDataBase
        {
            System.Uri uri = new System.Uri(m_strServerUrl);

            DoRequest(uri, MakeEncByte(Newtonsoft.Json.JsonConvert.SerializeObject(req)), (originalReq, response) =>
            {
                OnCommonCallback(originalReq, response, resp);
            });
        }

        public void RequestChat<T>(CProtocolBase req, System.Action<Response<T>> resp) where T : CResponseDataBase
        {
            System.Uri uri = new System.Uri(m_strChatUrl);

            DoRequest(uri, MakeEncByte(Newtonsoft.Json.JsonConvert.SerializeObject(req)), (originalReq, response) =>
            {
                OnCommonCallback(originalReq, response, resp);
            });
        }

        private void DoRequest(System.Uri uri, byte[] byteRawData, OnRequestFinishedDelegate onCallback)
        {
            var req = new HTTPRequest(uri, HTTP_METHOD, onCallback);
            req.RawData = byteRawData;
            req.Timeout = System.TimeSpan.FromSeconds(TIMEOUT_SEC);
            req.DisableCache = DISABLE_CACHE;

            req.Send();
        }

        private void OnCommonCallback<T>(HTTPRequest req, HTTPResponse resp, System.Action<Response<T>> callback) where T : CResponseDataBase
        {
            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    {
                        //Debug.LogFormat("요청이 정상적으로 완료 되었습니다 : {0}", resp.DataAsText);
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Response<T>>(MakeDesString(resp.Data));
                        
                        if (!CheckCommonError(result))
                            callback?.Invoke(result);
                    }
                    break;
                default:
                    Debug.LogErrorFormat("State : {0} \nException : {1}\nWhere : {2}", req.State, req.Exception.ToString(), typeof(T).FullName);
                    Debug.LogError("서버 개발자에게 바로 위에 로그 보내주면서 접속이 안되요. 하시면 됩니다.");
                    break;
            }
        }

        private bool CheckCommonError(Result cResult)
        {
            switch (cResult.status)
            {
                case 200:
                    return false;

                case 300:
                case 500:
                case 600:
                case 800:
                case 900:
                    OpenNetworkErrorPopup(cResult.status, true);
                    return true;
                // 세션오류.
                // TODO : 세션 재연결
                case 700:
                    OpenNetworkErrorPopup(cResult.status, true);
                    return true;
                default:
                    OpenNetworkErrorPopup(cResult.status, false);
                    return false;
            }
        }

        public void OpenNetworkErrorPopup(int nStatus, bool bCriticalError = false)
        {
            string strCode = string.Format("Code_{0}", nStatus);
            string strText = null;
            if (Managers.LZ.GetText(strCode, out strText))
            {
                if(bCriticalError)
                {
                    Managers.Window.EnqueuePopup(Managers.LZ[strCode], onCallbackBtnOk: () =>
                    {
                        Managers.RestartGame();
                    }, bBackClose: false, eType: WindowManager.EPopupType.System);
                }
                else
                {
                    Managers.Window.EnqueuePopup(Managers.LZ[strCode], bBackClose: false, eType: WindowManager.EPopupType.System);
                }
            }
            else if(bCriticalError)
            {
                Managers.Window.EnqueuePopup(string.Format("{0} : {1}", Managers.LZ["network_error"], nStatus), onCallbackBtnOk: () =>
                {
                    Managers.RestartGame();
                }, bBackClose: false, eType: WindowManager.EPopupType.System);
            }
            //if(Managers.LZ.get)
            
        }


#endregion New FrameWork

#if UNITY_EDITOR
#region Static Methods

        public static void RequestInEditor<T>(CProtocolBase req, System.Action<Response<T>> resp) where T : CResponseDataBase
        {
            var serverInfo = GetCurrServerInfo();
            var strServerURL = string.Format(FMT_IP_PORT, serverInfo.game_url, serverInfo.game_port);

            System.Uri uri = new System.Uri(strServerURL);

            var httpReq = new HTTPRequest(uri, HTTPMethods.Post,  (originalReq, response) =>
            {
                OnCommonCallbackInEditor(originalReq, response, resp);
            });

            httpReq.RawData = MakeEncByte(Newtonsoft.Json.JsonConvert.SerializeObject(req));
            httpReq.Timeout = System.TimeSpan.FromSeconds(TIMEOUT_SEC);
            httpReq.DisableCache = DISABLE_CACHE;
            httpReq.Send();
        }

        private static void OnCommonCallbackInEditor<T>(HTTPRequest req, HTTPResponse resp, System.Action<Response<T>> callback) where T : CResponseDataBase
        {
            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    {
                        //Debug.LogFormat("요청이 정상적으로 완료 되었습니다 : {0}", resp.DataAsText);
                        Debug.Log(typeof(T).FullName);
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Response<T>>(MakeDesString(resp.Data));
                        callback?.Invoke(result);
                    }
                    break;
                default:
                    Debug.LogError("ERROR State : " + req.State + " in " + typeof(T).FullName);
                    break;
            }
        }
#endregion
#endif
    }
}
