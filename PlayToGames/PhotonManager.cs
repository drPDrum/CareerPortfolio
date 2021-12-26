using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

namespace gameRevengers
{
	public class PhotonManager : MonoSingleton<PhotonManager>
	{
		private enum ECallBackState	//Call back except exception
		{
			None = -1,
			ConnectedToPhoton = 0,
			ConnectedToMaster,
			JoinedLobby,
			RecievedRoomList,
			PhotonRandomJoinFailed,
			LeftLobby,
			PhotonCustomRoomPropertiesChanged,
			CreateRoom,
			JoinedRoom,
			LeftRoom
		}
		
		private enum EErrorState	//Call Back Error
		{
			OnPhotonCreateRoomFailed,
			OnPhotonJoinRoomFailed,
			OnFailedToConnectToPhoton,
			OnConnectionFail,
			OnDisconnectedFromPhoton,
		}

		private enum ECallState	//Call Function
		{
			None = -1,
			Connect,
			JoinLobby,
			JoinRoom,
			CreateRoom,
			OffLineRoom
		}

		private const int 		PHOTON_LOBBY_MAIN = 0;
		private const int 		PHOTON_LOBBY_PVE = 1;
		private const int 		PHOTON_LOBBY_PVP = 2;
		private const int 		PHOTON_LOBBY_RAID = 3;
		private const string	PHOTON_VER = "0.1022";
		
		private static byte[] m_arrMaxPlayer = { 50, 3, 9, 3 };
		private static TypedLobby[] m_arrLobbyType = {
			new TypedLobby("mainLobby", LobbyType.SqlLobby),
			new TypedLobby("pveLobby", LobbyType.SqlLobby),
			new TypedLobby("pvpLobby", LobbyType.SqlLobby),
			new TypedLobby("raidLobby", LobbyType.SqlLobby)
		};

		private bool				m_bDebugMode = false;
		private	bool				m_bConnectedToMaster = false;
		public	bool				m_bJoinRoom = false;
		
		private string				m_strLastRoomName = null;
		private int					m_nLastPlayMode = 0;
		private bool				m_bPlayedGame = false;
		private int					m_nGroup = 0;
		
		private ECallState			m_eCall = ECallState.None;
		private ECallBackState		m_eCallBack = ECallBackState.None;

		private	UIManager			m_cUIManager;
		private	GameManager			m_cGameManager;
		private	CharacterManager	m_cCharacterManager;

		protected override void Awake()
		{
			base.Awake();

			m_cUIManager		= UIManager.Instance;
			m_cGameManager		= GameManager.Instance;
			m_cCharacterManager	= CharacterManager.Instance;
		}


		#region Photon Call Function (about connection)
		/// Connecting to the Photon Cloud might fail due to:
		/// - Invalid AppId (calls: OnFailedToConnectToPhoton(). check exact AppId value)
		/// - Network issues (calls: OnFailedToConnectToPhoton())
		/// - Invalid region (calls: OnConnectionFail() with DisconnectCause.InvalidRegion)
		/// - Subscription CCU limit reached (calls: OnConnectionFail() with DisconnectCause.MaxCcuReached. also calls: OnPhotonMaxCccuReached())
		public void PhotonConnect()
		{
			if(m_bDebugMode)
			{
				m_eCall = ECallState.Connect;
				string strLog = "|PHOTON CALL| PhotonConnect : " + PhotonNetwork.connectionState.ToString() + "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				Debug.Log(strLog);
			}
			
			if (PhotonNetwork.offlineMode)
				PhotonNetwork.offlineMode = false;
			
			if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
				return;
			
			PhotonNetwork.automaticallySyncScene = false;			
			PhotonNetwork.autoJoinLobby = false;
			
			StartCoroutine(ConnectPhoton());
		}

		public void PhotonConnect(int nGroup)
		{
			if(m_bDebugMode)
			{
				Debug.Log("PhotonConnect : Group : " + nGroup);
			}
			m_nGroup = nGroup;
			PhotonConnect();
		}
		
		/// Offline mode can be set to re-use your multiplayer code in singleplayer game modes.
		/// When this is on PhotonNetwork will not create any connections and there is near to
		/// no overhead. Mostly usefull for reusing RPC's and PhotonNetwork.Instantiate
		
		public void JoinLobby()	//Called loading scene.
		{
			int nType = CheckLobbyType();

			if(m_bDebugMode)
			{
				m_eCall = ECallState.JoinLobby;			
				string strLog = "|PHOTON CALL| JoinLobby : " + nType + "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				Debug.Log(strLog);
			}
			
			if(nType < 0)
			{
				PhotonNetwork.Disconnect();
				return;
			}
			
			PhotonNetwork.JoinLobby(m_arrLobbyType[nType]);
		}
		
		public void JoinRoom()
		{
			int nType = CheckLobbyType();

			string strLog = null;
			if(m_bDebugMode)
			{
				m_eCall = ECallState.JoinRoom;
				strLog = "|PHOTON CALL| JoinRoom : " + nType;
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
			}
			
			if(nType < 0)
				return;
			
			if(m_bPlayedGame && !string.IsNullOrEmpty(m_strLastRoomName))
			{
				PhotonNetwork.JoinRoom(m_strLastRoomName);
			}
			else
			{
				if(nType == PHOTON_LOBBY_MAIN)
				{
					if(m_bDebugMode)
					{
						strLog += "\t|PHOTON FUNCTION LOG : JoinLobby| : Matchmaking Lobby.";
						Debug.Log("Photon Main Lobby");
					}
					PhotonNetwork.JoinRandomRoom();
				}
				else if(nType == PHOTON_LOBBY_PVP)
				{
					if(m_nGroup == 0)
					{
						Debug.LogError("Group can't be Zero");
						return;
					}
					if(m_bDebugMode)
					{
						strLog += "\t|PHOTON FUNCTION LOG : JoinLobby| : Matchmaking Ingame for PVP.";
					}

					int nPlayMode = m_cGameManager.nPlayerMode;
//					Debug.Log("PVP Play Mode : " + nPlayMode);
					PhotonNetwork.JoinRandomRoom(null, m_arrMaxPlayer[nType], MatchmakingMode.FillRoom, 
					                             m_arrLobbyType[nType], "C0 = " + nPlayMode + " AND C" + m_nGroup + " < 3 ");
					// Only Use C0~C9. 			more filter variations: 
					// "C0 = 1 AND C2 > 50"		// "C5 = \"Map2\" AND C2 > 10 AND C2 < 20"
				}
				else
				{
					int nPlayMode = m_cGameManager.nBattleMode;
					if(m_bDebugMode)
					{
						strLog += "\t|PHOTON FUNCTION LOG : JoinLobby| : Matchmaking Ingame for PVE.";
					}
					PhotonNetwork.JoinRandomRoom(null, m_arrMaxPlayer[nType], MatchmakingMode.FillRoom, 
					                             m_arrLobbyType[nType], "C0 = " + nPlayMode);
					// Only Use C0~C9. 			more filter variations: 
					// "C0 = 1 AND C2 > 50"		// "C5 = \"Map2\" AND C2 > 10 AND C2 < 20"
				}
			}
			if(m_bDebugMode)
				Debug.Log(strLog);
		}
		
		public void CreateRoom()
		{
			int nType = CheckLobbyType();

			string strLog = null;
			if(m_bDebugMode)
			{
				m_eCall = ECallState.CreateRoom;
				
				strLog = "|PHOTON CALL| CreateRoom : " + nType;
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
			}
			
			if(nType < 0)
				return;
			
			RoomOptions roomOption = new RoomOptions();
			
			roomOption.isOpen = true;				//253	in roompropertie hashtable key.
			roomOption.isVisible = true;
			
			roomOption.customRoomProperties = new ExitGames.Client.Photon.Hashtable();
			
			int nGameMode = m_cGameManager.nBattleMode;

			if(m_bDebugMode)
			{
				if(nGameMode <= ComType.SCENE_TYPE_LOBBY)
				{
					strLog += "\t|PHOTON FUNCTION LOG : CreateRoom| : Make LobbyScene Room.";
				}
			}
			if(nGameMode < ComType.SCENE_TYPE_PVE)
			{
				if(m_bDebugMode)
					strLog += "\t|PHOTON FUNCTION LOG : CreateRoom| : Make PVP Room.";

				roomOption.customRoomProperties.Add("C0", nGameMode);
				for(int i = 1 ; i <= 3 ; ++i)
				{
					roomOption.customRoomProperties.Add("C" + i, 0);
				}
				roomOption.customRoomPropertiesForLobby = new string[] {"C0", "C1", "C2", "C3"};
			}
			else
			{
				if(m_bDebugMode)
					strLog += "\t|PHOTON FUNCTION LOG : CreateRoom| : Make Ingame Room.";

				roomOption.customRoomProperties.Add("C0", nGameMode);
				roomOption.customRoomPropertiesForLobby = new string[] {"C0"};
			}
			roomOption.maxPlayers = m_arrMaxPlayer[nType];
			PhotonNetwork.CreateRoom("", roomOption, m_arrLobbyType[nType]);
			if(m_bDebugMode)
				Debug.Log(strLog);
		}
		
		public void SetOfflineMode()
		{
			if(m_bDebugMode)
			{
				m_eCall = ECallState.OffLineRoom;
				Debug.Log("|PHOTON CALL FUNCTION| SetOfflineMode");
				Debug.Log("|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString());
			}
			/// Offline mode step
			/// 1. Disconnect network.
			/// 2. set trigger offlineMode on.
			/// 3. createRoom
			if(PhotonNetwork.connected)
			{
				/// Makes this client disconnect from the photon server, 
				/// a process that leaves any room and calls OnDisconnectedFromPhoton on completion.
				PhotonNetwork.Disconnect();
			}
			
			PhotonNetwork.offlineMode = true;
			PhotonNetwork.CreateRoom("SingleRoom");
		}
		#endregion Photon Call Function (about connection)
		
		#region Photon Callback Function except exception callback.
		
		void OnConnectedToPhoton()
		{			
			string strPlayerName = m_cGameManager.GetCharName();
			if(string.IsNullOrEmpty(strPlayerName))
			{
				//Debug.LogError("Name is null!");
				strPlayerName = "EmptyUser : " + DateTime.Now.ToString();
			}
			
			PhotonNetwork.playerName = strPlayerName;

			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.ConnectedToPhoton;
				string strLog = "|PHOTON CALLBACK| OnConnectedToPhoton() : " + strPlayerName + "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				Debug.Log(strLog);
			}
		}
		
		void OnConnectedToMaster()
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.ConnectedToMaster;
				string strLog = "|PHOTON CALLBACK| OnConnectedToMaster" + "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				Debug.Log(strLog);
			}
			m_bConnectedToMaster = true;
		}
		
		void OnJoinedLobby()
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.JoinedLobby;
				string strLog = "|PHOTON CALLBACK| OnJoinedLobby. \t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				Debug.Log(strLog);
			}
			JoinRoom();
		}
		
		void OnReceivedRoomListUpdate()
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.RecievedRoomList;			
				string strLog = "|PHOTON CALLBACK| OnReceivedRoomListUpdate. \t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				strLog += "\t|PHOTON FUNCTION LOG : OnReceivecRoomListUpdate|\t";
				
				if(PhotonNetwork.GetRoomList() == null || PhotonNetwork.GetRoomList().Length == 0)
					strLog += "RoomList is NULL.";
				
				foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
				{
					strLog += "RoomList = " + roomInfo.name + "(" + roomInfo.playerCount + "/" + roomInfo.maxPlayers + ")\t";
					strLog += "isOpen : "+ roomInfo.open + "\t";
					strLog += "isVisible : " + roomInfo.visible + "\t";
					strLog += "CustomPropertie\t" + GetCustomPropertiesLog(roomInfo.customProperties);
				}
				
				Debug.Log(strLog);
			}
		}
		
		void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.PhotonRandomJoinFailed;
				string strLog = "|PHOTON CALLBACK| OnPhotonRandomJoinFailed : [" + codeAndMsg[0].ToString() + "] = " + codeAndMsg[1].ToString();
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				
				Debug.Log(strLog);
			}
			CreateRoom();
		}
		
		void OnLeftLobby()
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.LeftLobby;

				string strLog = "|PHOTON CALLBACK| OnLeftLobby.";
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();

				Debug.Log(strLog);
			}
		}
		
		void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			/// Key 248 : master.ID;
			/// Key 249 : roomOption.cleanupCacheOnLeave
			/// Key 250 : customRoomPropertiesForLobby
			/// Key 253 : roomOption.isOpen
			/// Key 254 : roomOption.isVisible
			/// Key 255 : roomOption.maxPlayers
			/// 
			/// roomOption.customRoomProperties.Add("NA", "Albion");
			/// Key = NA, Value = Albion
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.PhotonCustomRoomPropertiesChanged;
				string strLog = "|PHOTON CALLBACK| OnPhotonCustomRoomPropertiesChanged";
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				
				strLog += "\t|PHOTON FUNCTION LOG : OnPhotonCustomRoomPropertiesChanged|\t"
					+ GetCustomPropertiesLog(propertiesThatChanged);
				
				Debug.Log(strLog);
			}
		}
		
		void OnCreatedRoom()
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.CreateRoom;

				string strLog = "|PHOTON CALLBACK| OnCreateRoom.";
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();

				Debug.Log(strLog);
			}
		}
		
		void OnJoinedRoom()
		{
			m_bJoinRoom = true;
			PhotonNetwork.isMessageQueueRunning = false;

			string strLog = null;
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.JoinedRoom;
				strLog = "|PHOTON CALLBACK| OnJoinedRoom : Connected room name = " + PhotonNetwork.room.name;
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();

				Room room = PhotonNetwork.room;
				
				strLog += "\t|PHOTON FUNCTION LOG : OnJoinedRoom : JoinedRoomInformation|\t";			
				strLog += "Name = " + room.name + "(" + room.playerCount + "/" + room.maxPlayers + ")\t";
				strLog += "isOpen : "+ room.open + "\t";
				strLog += "isVisible : " + room.visible + "\t";
				strLog += "isAutoCleanUp : " + room.autoCleanUp + "\t";
				strLog += "CustomPropertie\t" + GetCustomPropertiesLog(room.customProperties);

				strLog += "\tLastPlayedMode : " + m_nLastPlayMode;
			}
			if(!PhotonNetwork.offlineMode && m_nLastPlayMode > ComType.SCENE_TYPE_LOBBY)
			{
				if(m_bPlayedGame)
				{
					if(m_bDebugMode)
						strLog += "\tPlayedGame.";
					m_cUIManager.SetLoadingUIInitial(0);
				}
				else
				{
					if(m_bDebugMode)
						strLog += "\tNew Game.";
					SetIngameState(true);
				}
			}
			if(m_bDebugMode)
				Debug.Log(strLog);
		}
		
		void OnLeftRoom()
		{
			if(m_bDebugMode)
			{
				m_eCallBack = ECallBackState.LeftRoom;
				string strLog = "|PHOTON CALLBACK| OnLeftRoom() : bPlayedGame = " + m_bPlayedGame;
				strLog += "\t|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString();
				Debug.Log(strLog);
			}
			if(m_nLastPlayMode >= ComType.SCENE_TYPE_PVP && m_nLastPlayMode < ComType.SCENE_TYPE_PVE)
			{
				ExitGames.Client.Photon.Hashtable propertie = PhotonNetwork.room.customProperties;
				if((int)propertie["C" + m_nGroup] <= 0)
					Debug.LogError("Photon PVP Room matching ERROR!");
				else
				{
					propertie["C" + m_nGroup] = (int)propertie["C" + m_nGroup] - 1;
					PhotonNetwork.room.SetCustomProperties(propertie);
				}
			}

			m_cCharacterManager.ResetCharacter();
			m_cGameManager.ResetArmorPart();
			
			m_bConnectedToMaster = false;
			m_bJoinRoom = false;

			if(m_bPlayedGame)
			{
				Action<string> action = m_cUIManager.GetEvent<string>(KeyValues.EVT_LOG);
				if(action != null)
					action.Invoke("Connection Lost.");
				else
				{
					//Debug.LogError("Event Action is NULL");
				}
				
				//case : 1 Reconnect room
//				StartCoroutine(ReConnectRoom());
				
				//case : 2 Reconnect Lobby
				m_cGameManager.nBattleMode = ComType.SCENE_TYPE_LOBBY;
				SetIngameState(false);
				m_cUIManager.CallLoadingScene();
			}
			else
			{
				m_cUIManager.CallLoadingScene();
			}
		}
		#endregion Photon Callback Function except exception callback.
		
		#region Photon Exception Callback Function.
		
		void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			if(m_bDebugMode)
				Debug.LogError("|PHOTON CALLBACK| OnFailedToConnectToPhoton : " + cause);
//			m_bConnectedToPhoton = false;
		}
		
		void OnConnectionFail(DisconnectCause cause)
		{
			if(m_bDebugMode)
				Debug.LogError("|PHOTON CALLBACK| OnConnectionFail : " + cause);
		}
		
//		void OnDisconnectedFromPhoton()
//		{
//			Debug.Log("|PHOTON CALLBACK| OnDisconnectedFromPhoton");
//			m_bConnectedToPhoton = false;
//			StartCoroutine(ReConnectPhoton());
//		}
		
		//should do join again
		void OnPhotonCreateRoomFailed(object[] codeAndMsg)
		{
			if(m_bDebugMode)
				Debug.LogError("|PHOTON CALLBACK| OnPhotonCreateRoomFailed. : [" + codeAndMsg[0] + "] = " + codeAndMsg[1]);
		}
		
		//Use when player rejoin the ingame.
		void OnPhotonJoinRoomFailed(object[] codeAndMsg)
		{
			if(m_bDebugMode)
			{
				Debug.Log("|PHOTON CALLBACK| OnPhotonJoinRoomFailed.");
				if(codeAndMsg != null && codeAndMsg.Length > 0)
				{
					string strLog = "|PHOTON FUNCTION LOG : OnPhotonJoinRoomFailed|\t";
					for(int i = 0 ; i < codeAndMsg.Length ; ++i)
					{
						strLog += "[" + i + "] = " + codeAndMsg[i];
					}
					
					Debug.Log(strLog);
				}
			}
			//exception for join room failed. when room doesen't exist now.
			m_cGameManager.nBattleMode = ComType.SCENE_TYPE_LOBBY;
			SetIngameState(false);
			m_cUIManager.CallLoadingScene();
		}
		
		void OnPhotonMaxCccuReached()		
		{
			if(m_bDebugMode)
				Debug.LogError("|PHOTON CALLBACK| OnPhotonMaxCccuReached()");
		}		
		
//		//Not Use the CustomAuthentication
		void OnCustomAuthenticationFailed(string debugMessage)
		{
			if(m_bDebugMode)
				Debug.LogError("|PHOTON CALLBACK| OnCustomAuthenticationFailed : " + debugMessage);
		}
		
		#endregion Photon Exception Callback Function.
		
		#region Photon InGame Callback Function		
		void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			if(m_bDebugMode)
				Debug.Log("|PHOTON CALLBACK| OnPhotonInstantiate : " + info.ToString());
		}
		
		void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
		{
			if(m_bDebugMode)
				Debug.Log("|PHOTON CALLBACK| OnPhotonPlayerConnected : " + newPlayer.name);
		}
		
		void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
		{
			if(m_bDebugMode)
				Debug.Log("|PHOTON CALLBACK| OnPhotonPlayerDisconnected : " + otherPlayer.name);
		}
		
		void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
			if(m_bDebugMode)
			{
				string strLog = "|PHOTON CALLBACK| OnPhotonPlayerPropertiesChanged";
				for(int i = 0 ; i < playerAndUpdatedProps.Length ; ++i)
				{
					strLog += " [" + i + "] = " + playerAndUpdatedProps[i].ToString() + "\t";
				}
				Debug.Log(strLog);
			}
		}		
		#endregion InGame Callback Function
		
		
		
		
		
		#region private method
		
		private int CheckLobbyType()
		{
			int nMode = m_cGameManager.nBattleMode;
			if(nMode == ComType.SCENE_TYPE_LOBBY)
			{
				return PHOTON_LOBBY_MAIN;
			}
			else if(nMode >= ComType.SCENE_TYPE_PVP && nMode < ComType.SCENE_TYPE_PVE)
			{
				return PHOTON_LOBBY_PVP;
			}
			else if(nMode >= ComType.SCENE_TYPE_PVE && nMode < ComType.SCENE_TYPE_RAID)
			{
				return PHOTON_LOBBY_PVE;
			}
			else if(nMode >= ComType.SCENE_TYPE_RAID)
			{
				return PHOTON_LOBBY_RAID;
			}
			return -1;
		}
		
		private string GetCustomPropertiesLog(ExitGames.Client.Photon.Hashtable hashCustomProperties)
		{
			string strLog = "";

			if(!m_bDebugMode)
				return strLog;			

			foreach(object objKey in hashCustomProperties.Keys)
			{
				try
				{
					string[] arrStr = (string[])hashCustomProperties[objKey];
					if(arrStr == null)
					{
						Debug.Log("Array is null");
					}
					else
					{
						for(int i = 0 ; i < arrStr.Length ; ++i)
							strLog += " Key : " + objKey + " Value : " + arrStr[i] + "\t";
					}
				}
				catch
				{
					strLog += " Key : " + objKey + " Value : " + hashCustomProperties[objKey] + "\t";
				}
			}

			return strLog;
		}
		
		private IEnumerator ConnectPhoton()
		{
			int nCount = 0;
			while(!m_bConnectedToMaster)
			{
				++nCount;
				//Connect to Photon as configured in the editor
				//				Debug.Log("|PHOTON Call| ConnectPhoton : Count = " + nCount);
				PhotonNetwork.ConnectUsingSettings(PHOTON_VER);
				
				yield return new WaitForSeconds(5.0f);
				if(nCount > 5)
					break;
			}
			
			if(!m_bConnectedToMaster)
			{
				SetOfflineMode();
			}
		}
		#endregion private method
		
		#region Check Photon State
		
		public bool IsConnectedPhoton()
		{
			if(PhotonNetwork.offlineMode)
				return true;
			return m_bConnectedToMaster;
		}
		
		public void SetIngameState(bool bEnable)
		{
			if(m_bDebugMode)
			{
				Debug.Log("|PHOTON CALL FUNCTION| SetIngameState : " + bEnable);
				Debug.Log("|PHOTON State Check| Call : " + m_eCall.ToString() + " CallBack : " + m_eCallBack.ToString());
			}

			if(bEnable)
			{
				if(!m_bJoinRoom)
					return;
				m_bPlayedGame = true;
				m_strLastRoomName = PhotonNetwork.room.name;
			}
			else
			{
				m_bPlayedGame = false;
				m_strLastRoomName = null;
			}
		}
		#endregion
	}
}